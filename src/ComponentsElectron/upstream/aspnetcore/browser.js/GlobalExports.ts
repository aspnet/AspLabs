import { platform } from './Environment';
import { navigateTo, internalFunctions as uriHelperInternalFunctions } from './Services/UriHelper';
import { internalFunctions as httpInternalFunctions } from './Services/Http';
import { attachRootComponentToElement } from './Rendering/Renderer';
import { Pointer } from './Platform/Platform';

// Make the following APIs available in global scope for invocation from JS
window['Blazor'] = {
  navigateTo,

  _internal: {
    attachRootComponentToElement,
    http: httpInternalFunctions,
    uriHelper: uriHelperInternalFunctions
  }
};
