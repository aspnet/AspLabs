using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DocFx.XmlComments;

public class XmlParameterComment
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string? Example { get; set; }

    public static List<XmlParameterComment> GetXmlParameterListComment(XPathNavigator navigator, string xpath, XmlCommentParserContext context)
    {
        var iterator = navigator.Select(xpath);
        var result = new List<XmlParameterComment>();
        if (iterator == null)
        {
            return result;
        }
        foreach (XPathNavigator nav in iterator)
        {
            var name = nav.GetAttribute("name", string.Empty);

            if (!string.IsNullOrEmpty(name))
            {
                var description = nav.GetXmlValue(context);
                var example = nav.GetAttribute("example", string.Empty);
                result.Add(new XmlParameterComment { Name = name, Description = description, Example = example });
            }
        }

        return result;
    }
}
