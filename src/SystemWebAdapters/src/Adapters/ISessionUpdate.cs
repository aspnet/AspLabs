using System.Collections.Generic;

namespace System.Web.Adapters;

/// <summary>
/// ISessionUpdate encapsulates changes to an ISessionState.
/// It's used as a means of clients sending modifications to session
/// state without having to re-send all session state items that
/// haven't changed.
/// </summary>
public interface ISessionUpdate
{
    int? Timeout { get; set; }

    bool Abandon { get; set; }

    IList<string> RemovedKeys { get; }

    object? this[string name] { get; set; }

    IEnumerable<string> UpdatedKeys { get; }
}
