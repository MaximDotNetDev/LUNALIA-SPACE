using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SchoolJournal.Client.Core.Features.Operations.Attendances;

public sealed partial class StudentAttendanceUIModel : ObservableObject
{
    public Guid StudentId { get; init; }
    public required string FullName { get; init; }
    public Guid? OriginalAttendanceId { get; init; }

    [ObservableProperty]
    private string? _status;

    [ObservableProperty]
    private string? _comment;
}