using System.ComponentModel;

namespace SchoolJournal.Domain.Enums;

public enum Gender
{
    None = 0,

    [Description("Чоловіча")]
    Male = 1,

    [Description("Жіноча")]
    Female = 2
}