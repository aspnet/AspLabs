const resources = [];
export async function beforeStart(wasmOptions, extensions) {
    // Simple way of detecting we are in web assembly

    if (!extensions || !extensions.multipart) {
        return;
    }

    try {
        const integrity = extensions.multipart['app.bundle'];
        const bundleResponse = await fetch('app.bundle', { integrity: integrity });
        const bundleFromData = await bundleResponse.formData();
        for (let value of bundleFromData.values()) {
            resources.push([value, URL.createObjectURL(value)]);
        }

    } catch (error) {
        console.log(error);
    }

    wasmOptions.loadBootResource = function (type, name, defaultUri, integrity) {
        for (const [resource, objectUrl] of resources) {
            if (resource.name === name) {
                return objectUrl;
            }
        }
        const res = resources;
        return defaultUri;
    }
}

export async function afterStarted(blazor) {
    for (const [value, url] of resources) {
        URL.revokeObjectURL(url);
    }
}
