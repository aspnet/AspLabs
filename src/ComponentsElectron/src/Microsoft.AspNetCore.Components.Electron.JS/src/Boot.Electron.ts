import '@dotnet/jsinterop/dist/Microsoft.JSInterop';
import '@browserjs/GlobalExports';
import { OutOfProcessRenderBatch } from '@browserjs/Rendering/RenderBatch/OutOfProcessRenderBatch';
import { internalFunctions as uriHelperFunctions } from '@browserjs/Services/UriHelper';
import { renderBatch } from '@browserjs/Rendering/Renderer';
import { decode } from 'base64-arraybuffer';
import * as electron from 'electron';

function boot() {
  // Configure the mechanism for JS->.NET calls
  DotNet.attachDispatcher({
    beginInvokeDotNetFromJS: (callId, assemblyName, methodIdentifier, dotNetObjectId, argsJson) => {
      electron.ipcRenderer.send('BeginInvokeDotNetFromJS', [callId ? callId.toString() : null, assemblyName, methodIdentifier, dotNetObjectId || 0, argsJson]);
    }
  });

  // Wait until the .NET process says it is ready
  electron.ipcRenderer.once('components:init', async () => {
    // Confirm that the JS side is ready for the app to start
    electron.ipcRenderer.send('components:init', [
      uriHelperFunctions.getLocationHref().replace(/\/index\.html$/, ''),
      uriHelperFunctions.getBaseURI()]);
  });

  electron.ipcRenderer.on('JS.BeginInvokeJS', (_, asyncHandle, identifier, argsJson) => {
    DotNet.jsCallDispatcher.beginInvokeJSFromDotNet(asyncHandle, identifier, argsJson);
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
