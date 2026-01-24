namespace TodoApi.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // "Parent" eller "Child"
    public int? FamilyId { get; set; } // For at binde familien sammen
}