namespace CLOContactSystem.Api.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly JoinedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
