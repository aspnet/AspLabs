// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace DocFx.XmlComments;

internal static class XmlCommentTransformer
{
    private static readonly XslCompiledTransform _transform = InitializeTransform();

    private static XslCompiledTransform InitializeTransform()
    {
        var assembly = typeof(XmlCommentTransformer).Assembly;
        var xsltFilePath = $"{assembly.GetName().Name}.DocFx.XmlComments.Resources.XmlCommentTransform.xsl";
        using var stream = assembly.GetManifestResourceStream(xsltFilePath);
        using var reader = XmlReader.Create(stream);
        var xsltSettings = new XsltSettings(true, true);
        var transform = new XslCompiledTransform();
        transform.Load(reader, xsltSettings, new XmlUrlResolver());
        return transform;
    }

    public static string Transform(string xml)
    {
        using var ms = new StringWriter();
        using var writer = new XHtmlWriter(ms);
        var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        _transform.Transform(doc.CreateNavigator(), writer);
        return ms.ToString();
    }
}
