using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Builder;

public static class BlazorPackagingApplicationBuilderExtensions
{
    public static IApplicationBuilder UseBundlingExtension(this IApplicationBuilder builder, PathString pathPrefix)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var webHostEnvironment = builder.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        var options = CreateStaticFilesOptions(webHostEnvironment.WebRootFileProvider);

        builder.MapWhen(
            ctx => ctx.Request.Path.StartsWithSegments(pathPrefix, out var rest) && rest.Equals("/app.bundle", StringComparison.OrdinalIgnoreCase),
            subBuilder =>
            {
                subBuilder.UseMiddleware<ContentEncodingNegotiator>();
                subBuilder.UseStaticFiles(options);
            });

        return builder;
    }

    public static IApplicationBuilder UseBundlingExtension(this IApplicationBuilder applicationBuilder) =>
        UseBundlingExtension(applicationBuilder, default);

    private static StaticFileOptions CreateStaticFilesOptions(IFileProvider webRootFileProvider)
    {
        var options = new StaticFileOptions();
        options.FileProvider = webRootFileProvider;
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        contentTypeProvider.Mappings.Add(".bundle", "multipart/form-data; boundary=\"--0a7e8441d64b4bf89086b85e59523b7d\"");
        contentTypeProvider.Mappings.Add(".br", "multipart/form-data; boundary=\"--0a7e8441d64b4bf89086b85e59523b7d\"");
        options.ContentTypeProvider = contentTypeProvider;
        options.OnPrepareResponse = fileContext =>
        {
            fileContext.Context.Response.Headers.Append(HeaderNames.CacheControl, "no-cache");

            var requestPath = fileContext.Context.Request.Path;
            var fileExtension = Path.GetExtension(requestPath.Value);
            if (string.Equals(fileExtension, ".gz") || string.Equals(fileExtension, ".br"))
            {
                var originalPath = Path.GetFileNameWithoutExtension(requestPath.Value);
                if (originalPath != null && contentTypeProvider.TryGetContentType(originalPath, out var originalContentType))
                {
                    fileContext.Context.Response.ContentType = originalContentType;
                }
            }
        };

        return options;
    }

    private class ContentEncodingNegotiator
    {
        // List of encodings by preference order with their associated extension so that we can easily handle "*".
        private static readonly StringSegment[] _preferredEncodings =
            new StringSegment[] { "br", "gzip" };


        private static readonly Dictionary<StringSegment, string> _encodingExtensionMap = new Dictionary<StringSegment, string>(StringSegmentComparer.OrdinalIgnoreCase)
        {
            ["br"] = ".br",
            ["gzip"] = ".gz"
        };


        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ContentEncodingNegotiator(RequestDelegate next, IWebHostEnvironment webHostEnvironment)
        {
            _next = next;
            _webHostEnvironment = webHostEnvironment;
        }


        public Task InvokeAsync(HttpContext context)
        {
            NegotiateEncoding(context);
            return _next(context);
        }

        private void NegotiateEncoding(HttpContext context)
        {
            var accept = context.Request.Headers.AcceptEncoding;


            if (StringValues.IsNullOrEmpty(accept))
            {
                return;
            }


            if (!StringWithQualityHeaderValue.TryParseList(accept, out var encodings) || encodings.Count == 0)
            {
                return;
            }


            var selectedEncoding = StringSegment.Empty;
            var selectedEncodingQuality = .0;


            foreach (var encoding in encodings)
            {
                var encodingName = encoding.Value;
                var quality = encoding.Quality.GetValueOrDefault(1);


                if (quality >= double.Epsilon && quality >= selectedEncodingQuality)
                {
                    if (quality == selectedEncodingQuality)
                    {
                        selectedEncoding = PickPreferredEncoding(context, selectedEncoding, encoding);
                    }
                    else if (_encodingExtensionMap.TryGetValue(encodingName, out var encodingExtension) && ResourceExists(context, encodingExtension))
                    {
                        selectedEncoding = encodingName;
                        selectedEncodingQuality = quality;
                    }


                    if (StringSegment.Equals("*", encodingName, StringComparison.Ordinal))
                    {
                        // If we *, pick the first preferrent encoding for which a resource exists.
                        selectedEncoding = PickPreferredEncoding(context, default, encoding);
                        selectedEncodingQuality = quality;
                    }


                    if (StringSegment.Equals("identity", encodingName, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedEncoding = StringSegment.Empty;
                        selectedEncodingQuality = quality;
                    }
                }
            }


            if (_encodingExtensionMap.TryGetValue(selectedEncoding, out var extension))
            {
                context.Request.Path = context.Request.Path + extension;
                context.Response.Headers.ContentEncoding = selectedEncoding.Value;
                context.Response.Headers.Append(HeaderNames.Vary, HeaderNames.ContentEncoding);
            }


            return;


            StringSegment PickPreferredEncoding(HttpContext context, StringSegment selectedEncoding, StringWithQualityHeaderValue encoding)
            {
                foreach (var preferredEncoding in _preferredEncodings)
                {
                    if (preferredEncoding == selectedEncoding)
                    {
                        return selectedEncoding;
                    }


                    if ((preferredEncoding == encoding.Value || encoding.Value == "*") && ResourceExists(context, _encodingExtensionMap[preferredEncoding]))
                    {
                        return preferredEncoding;
                    }
                }


                return StringSegment.Empty;
            }
        }


        private bool ResourceExists(HttpContext context, string extension) =>
            _webHostEnvironment.WebRootFileProvider.GetFileInfo(context.Request.Path + extension).Exists;
    }
}
