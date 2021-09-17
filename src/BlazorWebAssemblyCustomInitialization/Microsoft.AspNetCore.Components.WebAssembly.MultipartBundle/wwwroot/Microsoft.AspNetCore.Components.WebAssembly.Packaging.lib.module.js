const resources = new Map();
export async function beforeStart(wasmOptions, extensions) {
    // Simple way of detecting we are in web assembly
    if (!extensions || !extensions.multipart) {
        return;
    }

    try {
        const integrity = extensions.multipart['app.bundle'];
        const bundleResponse = await fetch('app.bundle', { integrity: integrity, cache: 'no-cache' });
        const bundleFromData = await bundleResponse.formData();
        for (let value of bundleFromData.values()) {
            resources.set(value, URL.createObjectURL(value));
        }
        wasmOptions.loadBootResource = function (type, name, defaultUri, integrity) {
            return resources.get(name) ?? null;
        }
    } catch (error) {
        console.log(error);
    }
}

export async function afterStarted(blazor) {
    for (const [_, url] of resources) {
        URL.revokeObjectURL(url);
    }
}
