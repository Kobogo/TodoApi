using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class DynamicTask
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public required string Title { get; set; }
        public bool IsCompleted { get; set; }
        public TimeSpan? TimeOfDay { get; set; }
        public DateTime? LastCompletedDate { get; set; }
        public DateTime? LastShownDate { get; set; }
        public List<DayOfWeek>? RepeatDays { get; set; }
        public int Points { get; set; } = 2;
        public int? BonusMinutesEarnedToday { get; set; } = 15;
    }
}
