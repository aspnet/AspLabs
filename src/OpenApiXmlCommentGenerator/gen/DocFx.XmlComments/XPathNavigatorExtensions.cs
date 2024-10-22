using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DocFx.XmlComments;

public static class XPathNavigatorExtensions
{
    public static string? GetXmlValue(this XPathNavigator navigator, XmlCommentParserContext xmlCommentParserContext)
    {
        if (navigator is null)
        {
            return null;
        }

        if (xmlCommentParserContext.SkipMarkup != false)
        {
            return navigator.InnerXml.TrimEachLine();
        }

        return null;
    }
}
