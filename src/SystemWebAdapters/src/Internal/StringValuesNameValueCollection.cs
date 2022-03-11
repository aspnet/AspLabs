using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace System.Web.Internal
{
    internal class StringValuesNameValueCollection : NameValueCollection
    {
        public static NameValueCollection Empty { get; } = new StringValuesNameValueCollection();

        public StringValuesNameValueCollection()
            : this(Enumerable.Empty<KeyValuePair<string, StringValues>>())
        {
        }

        public StringValuesNameValueCollection(IEnumerable<KeyValuePair<string, StringValues>> values)
        {
            foreach (var item in values)
            {
                foreach (var str in item.Value)
                {
                    Add(item.Key, str);
                }
            }

            IsReadOnly = true;
        }
    }
}
