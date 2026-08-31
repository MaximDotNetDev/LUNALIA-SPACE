using SchoolJournal.Application.Common.Interfaces;

namespace SchoolJournal.Application.Common.Services;

public sealed class AuditContext : IAuditContext
{
    private object? _oldState;
    private object? _newState;

    public void TrackOldState(object state)
    {
        _oldState = state;
    }

    public object? GetOldState()
    {
        return _oldState;
    }

    public void TrackNewState(object state)
    {
        _newState = state;
    }

    public object? GetNewState()
    {
        return _newState;
    }
}