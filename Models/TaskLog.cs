using System;
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class TaskLog
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        // Datoen loggen gælder for (f.eks. 2024-05-20)
        public DateTime Date { get; set; }

        // Hvor mange opgaver blev gennemført i alt den dag
        public int TasksCompleted { get; set; }

        // Hvad var målet for den dag (standard f.eks. 3)
        public int DailyGoal { get; set; }

        // En hjælpemetode til at se om målet blev nået
        public bool IsGoalReached => TasksCompleted >= DailyGoal;

        // Valgfrit: Hvor mange point blev optjent denne dag?
        public int PointsEarned { get; set; }
    }
}