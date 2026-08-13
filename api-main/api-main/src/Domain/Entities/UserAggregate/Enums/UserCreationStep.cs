namespace Domain.Entities.UserAggregate.Enums;

public enum UserCreationStep : byte
{
    PersonalDocuments = 0,
    SalaryDetail = 1,
    Permissions = 2,
    Completed = 3
}