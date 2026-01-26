namespace TodoApi.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Child"; // "Parent" eller "Child"
    public int? FamilyId { get; set; }
    public int TotalPoints { get; set; } = 0;
    public decimal SavingsBalance { get; set; } = 0;
    public string? FamilyName { get; set; }
}