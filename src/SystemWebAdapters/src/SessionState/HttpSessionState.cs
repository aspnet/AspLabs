// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Linq;
using System.Web.Adapters;
using System.Web.Adapters.SessionState;

namespace System.Web.SessionState;

public class HttpSessionState
    // Implement ICollection for compatibility with .NET Framework
    : ICollection
{
    private readonly ISessionState _remoteState;
    private readonly SessionUpdate _stateUpdate;
    private bool _completed;

    internal bool Completed => _completed;

    internal ISessionUpdate Updates => _stateUpdate;

    internal void Complete() => _completed = true;

    public HttpSessionState(ISessionState remoteState)
    {
        _remoteState = remoteState;
        _stateUpdate = new SessionUpdate();
        _completed = false;
    }

    public string SessionID => _remoteState.SessionID;

    public int Count => _remoteState.Keys
                        .Union(_stateUpdate.UpdatedKeys)
                        .Where(k => !_stateUpdate.RemovedKeys.Contains(k))
                        .Count();

    public bool IsReadOnly => _remoteState.IsReadOnly;

    public bool IsNewSession => _remoteState.IsNewSession;

    public int TimeOut
    {
        get => _stateUpdate.Timeout ?? _remoteState.Timeout;
        set
        {
            CheckCompleted();
            _stateUpdate.Timeout = value;
        }
    }

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public void Abandon()
    {
        CheckCompleted();
        _stateUpdate.Abandon = true;
    }

    public object? this[string name]
    {
        get
        {
            if (_stateUpdate.RemovedKeys.Contains(name))
            {
                return null;
            }

            return _stateUpdate[name] ?? _remoteState[name];
        }
        set
        {
            CheckCompleted();
            _stateUpdate.RemovedKeys.Remove(name);
            _stateUpdate[name] = value;
        }
    }

    public void Add(string name, object value)
    {
        CheckCompleted();
        _stateUpdate.RemovedKeys.Remove(name);
        _stateUpdate[name] = value;
    }

    public void Remove(string name)
    {
        CheckCompleted();
        _stateUpdate.RemovedKeys.Add(name);
        _stateUpdate.Values.Remove(name);
    }

    public void RemoveAll()
    {
        Clear();
    }

    public void Clear()
    {
        CheckCompleted();
        _stateUpdate.RemovedKeys.Clear();
        _stateUpdate.Values.Clear();
        foreach (var key in _remoteState.Keys)
        {
            _stateUpdate.RemovedKeys.Add(key);
        }
    }

    private void CheckCompleted()
    {
        if (Completed)
        {
            throw new InvalidOperationException("Session state cannot be changed after it is committed to remote session store");
        }
    }

    public void CopyTo(Array array, int index)
    {
        foreach (var keyName in this)
        {
            array.SetValue(keyName, index++);
        }
    }

    public IEnumerator GetEnumerator() => _remoteState.Keys
                        .Union(_stateUpdate.UpdatedKeys)
                        .Where(k => !_stateUpdate.RemovedKeys.Contains(k))
                        .GetEnumerator();
}
