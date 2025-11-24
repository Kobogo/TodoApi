using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ToDoAPI.Models
{
    public class StaticTask
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public TimeSpan? TimeOfDay { get; set; }
        public DateTime? LastCompletedDate { get; set; }
        public DateTime? LastShownDate { get; set; }
        public List<DayOfWeek>? RepeatDays { get; set; }
    }
}
