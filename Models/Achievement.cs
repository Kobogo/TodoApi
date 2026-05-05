using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models
{

    public class Achievement
    {
        [Key]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("icon")]
        public string Icon { get; set; } = "🏆"; // Emoji eller URL

        [Column("category")]
        public string Category { get; set; } = "Tasks"; // Tasks, Streaks, Savings

        [Column("requirementValue")]
        public int RequirementValue { get; set; } // F.eks. 10 (opgaver) eller 500 (point)

        [Column("rewardAchievementPoints")]
        public int RewardAchievementPoints { get; set; } // Point til at købe pakker for
    }
}