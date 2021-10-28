document.addEventListener('readystatechange', ev => {
    if(document.readyState === 'complete'){
        const text = document.createElement('p');
        text.innerText = 'Hello from SampleRazorClassLibraryWithJsAssets!';
        document.body.appendChild(text);
    }
});