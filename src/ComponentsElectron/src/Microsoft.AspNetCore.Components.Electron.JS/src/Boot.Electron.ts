import '@dotnet/jsinterop/dist/Microsoft.JSInterop';
import '@browserjs/GlobalExports';
import { OutOfProcessRenderBatch } from '@browserjs/Rendering/RenderBatch/OutOfProcessRenderBatch';
import { setEventDispatcher } from '@browserjs/Rendering/RendererEventDispatcher';
import { internalFunctions as navigationManagerFunctions } from '@browserjs/Services/NavigationManager';
import { renderBatch } from '@browserjs/Rendering/Renderer';
import { decode } from 'base64-arraybuffer';
import * as electron from 'electron';

function boot() {
  // TODO: In ComponentsElectron, duplicate the parts of WASM Blazor that receive incoming
  // notifications from the browser:
  //  - WebAssemblyEventDispatcher.DispatchEvent
  //  - JSInteropMethods.NotifyLocationChanged
  // ... and remove the following line
  setEventDispatcher((eventDescriptor, eventArgs) => DotNet.invokeMethodAsync('Microsoft.AspNetCore.Components.Web', 'DispatchEvent', eventDescriptor, JSON.stringify(eventArgs)));

  // Configure the mechanism for JS<->NET calls
  DotNet.attachDispatcher({
    beginInvokeDotNetFromJS: (callId: number, assemblyName: string | null, methodIdentifier: string, dotNetObjectId: number | null, argsJson: string) => {
      electron.ipcRenderer.send('BeginInvokeDotNetFromJS', [callId ? callId.toString() : null, assemblyName, methodIdentifier, dotNetObjectId || 0, argsJson]);
    },
    endInvokeJSFromDotNet: (callId: number, succeeded: boolean, resultOrError: any) => {
      electron.ipcRenderer.send('EndInvokeJSFromDotNet', [callId, succeeded, resultOrError]);
    }
  });

  // Wait until the .NET process says it is ready
  electron.ipcRenderer.once('components:init', async () => {
    // Confirm that the JS side is ready for the app to start
    electron.ipcRenderer.send('components:init', [
      navigationManagerFunctions.getLocationHref().replace(/\/index\.html$/, ''),
      navigationManagerFunctions.getBaseURI()]);
  });

  electron.ipcRenderer.on('JS.BeginInvokeJS', (_, asyncHandle, identifier, argsJson) => {
    DotNet.jsCallDispatcher.beginInvokeJSFromDotNet(asyncHandle, identifier, argsJson);
  });

  electron.ipcRenderer.on('JS.EndInvokeDotNet', (callId, success, resultOrError) => {
    DotNet.jsCallDispatcher.endInvokeDotNetFromJS(callId, success, resultOrError);
  });

  electron.ipcRenderer.on('JS.RenderBatch', (_, rendererId, batchBase64) => {
    var batchData = new Uint8Array(decode(batchBase64));
    renderBatch(rendererId, new OutOfProcessRenderBatch(batchData));
  });

  electron.ipcRenderer.on('JS.Error', (_, message) => {
    console.error(message);
  });
}

boot();
