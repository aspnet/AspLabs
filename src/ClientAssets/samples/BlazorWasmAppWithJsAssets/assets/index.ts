document.addEventListener('readystatechange', ev => {
    if(document.readyState === 'complete'){
        const text = document.createElement('p');
        text.innerText = 'Hello from BlazorWasmAppWithJsAssets!';
        document.body.appendChild(text);
    }
});