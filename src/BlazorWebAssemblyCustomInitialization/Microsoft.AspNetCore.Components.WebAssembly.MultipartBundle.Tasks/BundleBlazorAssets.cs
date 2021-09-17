using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.AspNetCore.Components.WebAssembly.MultipartBundle.Tasks
{
    public class BundleBlazorAssets : Task
    {
        [Required]
        public ITaskItem[] PublishBlazorBootStaticWebAsset { get; set; }

        [Required]
        public string BundlePath { get; set; }

        [Output]
        public ITaskItem[] Extension { get; set; }

        public override bool Execute()
        {
            var bundle = new MultipartFormDataContent("--0a7e8441d64b4bf89086b85e59523b7d");
            foreach (var asset in PublishBlazorBootStaticWebAsset)
            {
                var name = Path.GetFileName(asset.GetMetadata("RelativePath"));
                var fileContents = File.OpenRead(asset.ItemSpec);
                var content = new StreamContent(fileContents);
                var disposition = new ContentDispositionHeaderValue("form-data");
                disposition.Name = name;
                disposition.FileName = name;
                content.Headers.ContentDisposition = disposition;
                var contentType = Path.GetExtension(name) switch
                {
                    ".js" => "text/javascript",
                    ".wasm" => "application/wasm",
                    _ => "application/octet-stream"
                };
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                bundle.Add(content);
            }

            using (var output = File.Open(BundlePath, FileMode.OpenOrCreate))
            {
                output.SetLength(0);
                bundle.CopyToAsync(output).ConfigureAwait(false).GetAwaiter().GetResult();
                output.Flush(true);
            }

            var bundleItem = new TaskItem(BundlePath);
            bundleItem.SetMetadata("RelativePath", "app.bundle");
            bundleItem.SetMetadata("ExtensionName", "multipart");

            Extension = new ITaskItem[] { bundleItem };

            return true;
        }
    }
}
