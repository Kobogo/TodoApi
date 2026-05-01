using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models
{
    public class StaticTask
    {
        [Key]
        public int Id { get; set; }

        [Column("userId")]
        public int? UserId { get; set; }

        [Column("title")]
        public required string Title { get; set; }

        [Column("isCompleted")]
        public bool IsCompleted { get; set; }

        [Column("timeOfDay")]
        public TimeSpan? TimeOfDay { get; set; }

        [Column("lastCompletedDate")]
        public DateTime? LastCompletedDate { get; set; }

        [Column("lastShownDate")]
        public DateTime? LastShownDate { get; set; }

        [Column("repeatDays")]
        public List<DayOfWeek>? RepeatDays { get; set; }

        [Column("points")]
        public int Points { get; set; } = 2;

        [Column("timeBonusMinutes")]
        public int? TimeBonusMinutes { get; set; } = 15;

        [Column("isRepeatable")]
        public bool IsRepeatable { get; set; }
    }
}
