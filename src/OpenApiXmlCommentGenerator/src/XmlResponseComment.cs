using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DocFx.XmlComments;

public class XmlResponseComment
{
    public string Code { get; set; }
    public string? Description { get; set; }

    public string? Example { get; set; }

    public static List<XmlResponseComment> GetXmlResponseCommentList(XPathNavigator navigator, string xpath, XmlCommentParserContext context)
    {
        var iterator = navigator.Select(xpath);
        var result = new List<XmlResponseComment>();
        if (iterator == null)
        {
            return result;
        }
        foreach (XPathNavigator nav in iterator)
        {
            var code = nav.GetAttribute("code", string.Empty);

            if (!string.IsNullOrEmpty(code))
            {
                var description = nav.GetXmlValue(context);
                var example = nav.GetAttribute("example", string.Empty);
                result.Add(new XmlResponseComment { Code = code, Description = description, Example = example });
            }
        }

        return result;
    }
}
