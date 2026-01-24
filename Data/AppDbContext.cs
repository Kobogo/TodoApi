using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // Til ValueComparer
using System.Text.Json; // Til JsonSerializer
using System.Collections.Generic;
using TodoApi.Models;
using System;
using System.Linq;

namespace TodoApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<StaticTask> StaticTasks { get; set; } = null!;
        public DbSet<DynamicTask> DynamicTasks { get; set; } = null!;
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- StaticTask konfiguration ---
            modelBuilder.Entity<StaticTask>(builder =>
            {
                builder.Property(e => e.RepeatDays)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => string.IsNullOrEmpty(v) ? new List<DayOfWeek>() : JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions)null),
                        new ValueComparer<List<DayOfWeek>>(
                            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                            c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c == null ? new List<DayOfWeek>() : c.ToList()
                        )
                    );

                // Seed-data
                builder.HasData(
                    new StaticTask { Id = 1, Title = "Tømme opvaskemaskine & flyde den igen", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 2, Title = "Tørre støv af", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 3, Title = "Dække bord + tørre bord af", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 4, Title = "Støvsuge hele huset", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 5, Title = "Vaske gulv", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 6, Title = "Hænge vasketøj op med en voksen", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 7, Title = "Skylle af efter aftensmaden", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 8, Title = "Pille tøj ned af tørrestativet", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 9, Title = "Lægge tøj sammen + lægge tøj på plads med en voksen", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 10, Title = "Tømme skraldespande", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 11, Title = "Ordne badeværelser med en voksen", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 12, Title = "Slå græs", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 13, Title = "Fejrne ukrudt (min. 1 spand)", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 14, Title = "Være med til at lave aftensmad", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 15, Title = "Fylde op i køleskab med sodavand", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 16, Title = "Give kattene mad", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 17, Title = "Rede senge (Alle)", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null }
                );
            });

            // --- DynamicTask konfiguration ---
            modelBuilder.Entity<DynamicTask>(builder =>
            {
                builder.Property(e => e.RepeatDays)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => string.IsNullOrEmpty(v) ? new List<DayOfWeek>() : JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions)null),
                        new ValueComparer<List<DayOfWeek>>(
                            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                            c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c == null ? new List<DayOfWeek>() : c.ToList()
                        )
                    );

                // Ingen seed-data for dynamiske opgaver — starter som tom tabel
            });
        }
    }
}
