using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Web.Adapters.SessionState;

internal class SessionValues : NameObjectCollectionBase
{
    public void Add(string key, object? value) => BaseAdd(key, value);

    public void Clear() => BaseClear();

    public void Remove(string key) => BaseRemove(key);

    public new IEnumerable<string> Keys
    {
        get
        {
            foreach (var key in base.Keys)
            {
                yield return (string)key!;
            }
        }
    }

    public object? this[string key]
    {
        get => BaseGet(key);
        set => BaseSet(key, value);
    }
}
