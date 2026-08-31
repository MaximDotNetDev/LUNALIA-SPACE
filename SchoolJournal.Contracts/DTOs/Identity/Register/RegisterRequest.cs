using System.Text.Json.Serialization;
using SchoolJournal.Contracts.Enums.Identity;

namespace SchoolJournal.Contracts.DTOs.Identity.Register;

public sealed record RegisterRequest(
    string Login,
    string Password,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] RoleType Role
);