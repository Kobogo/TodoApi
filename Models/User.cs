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

    [Column("dailyGoal")]
    public int DailyGoal { get; set; } = 3;

    [Column("minutesLeftToday")]
    public int MinutesLeftToday { get; set; } = 240; // Standard 4 timer

    [Column("saturdayBonusPot")]
    public int SaturdayBonusPot { get; set; } = 0; // Her opspares overskydende tid

    [Column("lastTimerUpdate")]
    public DateTime LastTimerUpdate { get; set; } = DateTime.Now;

    [Column("isTimerRunning")]
    public bool IsTimerRunning { get; set; } = false;

    [Column("bonusMinutesEarnedToday")]
    public int BonusMinutesEarnedToday { get; set; } = 0;

    [Column("isPaused")]
    public bool IsPaused { get; set; } = false;

    [Column("achievementPoints")]
    public int AchievementPoints { get; set; } = 0; // Bruges til pakker

    [Column("currentMultiplier")]
    public double CurrentMultiplier { get; set; } = 1.0; // Standard er 1x point

    [Column("multiplierExpiry")]
    public DateTime? MultiplierExpiry { get; set; } // Hvornår udløber buffen?

    [Column("totalTasksCompleted")]
    public int TotalTasksCompleted { get; set; } = 0; // Til at tracke stats

    [Column("emeralds")]
    public int Emeralds { get; set; } = 0; // Bruges til at tracke emeralder
}