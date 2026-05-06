using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models
{

    public class UserAchievement
    {
        [Key]
        public int Id { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        [Column("achievementId")]
        public int AchievementId { get; set; }

        [Column("unlockedAt")]
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

        [Column("isRewardClaimed")]
        public bool IsRewardClaimed { get; set; } = false;

        // Navigation properties
        public Achievement? Achievement { get; set; }
    }
}