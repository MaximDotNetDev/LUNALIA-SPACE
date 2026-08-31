namespace SchoolJournal.Contracts.Common;

public sealed record PageRequest(int PageNumber = 1, int PageSize = 10)
{
    public int Skip => (PageNumber - 1) * PageSize;
}
