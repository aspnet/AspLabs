using System.Collections.Generic;

namespace System.Web.Adapters.SessionState;

/// <summary>
/// ISessionUpdate encapsulates changes to an ISessionState.
/// It's used as a means of clients sending modifications to session
/// state without having to re-send all session state items that
/// haven't changed.
/// </summary>
internal interface ISessionUpdate
{
    int? Timeout { get; }

    bool Abandon { get; }

    IEnumerable<string> RemovedItems { get; }

    SessionValues SessionValues { get; }
}
