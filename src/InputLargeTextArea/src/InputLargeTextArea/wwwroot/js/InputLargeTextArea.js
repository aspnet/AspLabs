(function () {
    const InputLargeTextArea = {
        init,
        getText,
        setText,
        enableTextArea,
    };

    function init(callbackWrapper, elem) {
        elem.addEventListener('change', function () {
            callbackWrapper.invokeMethodAsync('NotifyChange', elem.value.length);
        });
    }

    function getText(elem) {
        const textValue = elem.value;
        const utf8Encoder = new TextEncoder();
        const encodedTextValue = utf8Encoder.encode(textValue);
        return encodedTextValue;
    }

    async function setText(elem, streamRef) {
        const bytes = await streamRef.arrayBuffer();
        const utf8Decoder = new TextDecoder();
        const newTextValue = utf8Decoder.decode(bytes);
        elem.value = newTextValue;
    }

    function enableTextArea(elem, disabled) {
        elem.disabled = disabled;
    }

    // Make the following APIs available in global scope for invocation from JS
    window['InputLargeTextArea'] = InputLargeTextArea;
})();
