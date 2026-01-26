using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models;

public class User
{
    public int Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("role")]
    public string Role { get; set; } = "Child";

    [Column("familyId")]
    public int? FamilyId { get; set; }

    [Column("totalPoints")]
    public int TotalPoints { get; set; } = 0;

    [Column("savingsBalance")]
    public decimal SavingsBalance { get; set; } = 0;

    [Column("familyName")]
    public string? FamilyName { get; set; }
}