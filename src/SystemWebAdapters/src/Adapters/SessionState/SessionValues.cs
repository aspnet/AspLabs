using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Web.Adapters.SessionState;

internal class SessionValues : NameObjectCollectionBase
{
    public void Add(string key, object? value) => BaseAdd(key, value);

    public void Clear() => BaseClear();

    public void Remove(string key) => BaseRemove(key);

    public IEnumerable<(string, object?)> KeyValues
    {
        get
        {
            foreach (string? key in Keys)
            {
                if (key is not null)
                {
                    yield return (key, BaseGet(key));
                }
            }
        }
    }

    public object? this[string key]
    {
        get => BaseGet(key);
        set => BaseSet(key, value);
    }
}
