using System.IO;
using Microsoft.AspNetCore.Http;

namespace System.Web
{
    public class HttpPostedFileBase
    {
        public IFormFile FormFile { get; set; }

        public int ContentLength => (int)FormFile.Length;

        public string ContentType => FormFile.ContentType;

        public string FileName => FormFile.FileName;

        public Stream InputStream => FormFile.OpenReadStream();

        public void SaveAs(string fileName)
        {
            using var inputStream = File.OpenWrite(fileName);
            InputStream.CopyTo(inputStream);
        }
    }
}
