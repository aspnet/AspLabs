using System.Collections.Generic;
using System.Linq;

namespace System.Web.Adapters.SessionState;

/// <summary>
/// SessionUpdate encapsulates changes to an ISessionState.
/// It's used as a means of clients sending modifications to session
/// state without having to re-send all session state items that
/// haven't changed.
/// </summary>
internal class SessionUpdate
    : ISessionUpdate
{
    public object? this[string name]
    {
        get => Values[name];
        set => Values[name] = value;
    }

    public int? Timeout { get; set; }

    public bool Abandon { get; set; }

    public IList<string> RemovedKeys { get; set; } = new List<string>();

    public IEnumerable<string> UpdatedKeys => Values.KeyValues.Select(kvp => kvp.Key);

    internal SessionValues Values { get; set; } = new SessionValues();

}
