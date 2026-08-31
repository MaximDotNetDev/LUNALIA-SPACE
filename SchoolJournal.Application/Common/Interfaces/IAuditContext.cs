namespace SchoolJournal.Application.Common.Interfaces;

public interface IAuditContext
{
    public void TrackOldState(object state);
    public object? GetOldState();

    public void TrackNewState(object state);
    public object? GetNewState();
}