using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
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
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- USER MAPPING (camelCase) ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users"); // Tabellen hedder bare 'users'
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Username).HasColumnName("username");
                entity.Property(e => e.PasswordHash).HasColumnName("passwordHash");
                entity.Property(e => e.Role).HasColumnName("role");
                entity.Property(e => e.FamilyId).HasColumnName("familyId");
                entity.Property(e => e.TotalPoints).HasColumnName("totalPoints");
                entity.Property(e => e.SavingsBalance).HasColumnName("savingsBalance");
            });

            // --- StaticTask konfiguration (camelCase) ---
            modelBuilder.Entity<StaticTask>(builder =>
            {
                builder.ToTable("staticTasks");
                builder.Property(e => e.Id).HasColumnName("id");
                builder.Property(e => e.Title).HasColumnName("title");
                builder.Property(e => e.IsCompleted).HasColumnName("isCompleted");
                builder.Property(e => e.UserId).HasColumnName("userId");
                builder.Property(e => e.TimeOfDay).HasColumnName("timeOfDay");
                builder.Property(e => e.LastCompletedDate).HasColumnName("lastCompletedDate");
                builder.Property(e => e.LastShownDate).HasColumnName("lastShownDate");

                builder.Property(e => e.RepeatDays)
                    .HasColumnName("repeatDays")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => string.IsNullOrEmpty(v) ? new List<DayOfWeek>() : JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions)null),
                        new ValueComparer<List<DayOfWeek>>(
                            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                            c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c == null ? new List<DayOfWeek>() : c.ToList()
                        )
                    );

                // Seed data (Valgfrit - fjern hvis du hellere vil styre det manuelt i DB)
                builder.HasData(
                    new StaticTask { Id = 1, Title = "Tømme opvaskemaskine", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), UserId = 1 },
                    new StaticTask { Id = 2, Title = "Tørre støv af", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), UserId = 1 }
                );
            });

            // --- DynamicTask konfiguration (camelCase) ---
            modelBuilder.Entity<DynamicTask>(builder =>
            {
                builder.ToTable("dynamicTasks");
                builder.Property(e => e.Id).HasColumnName("id");
                builder.Property(e => e.Title).HasColumnName("title");
                builder.Property(e => e.IsCompleted).HasColumnName("isCompleted");
                builder.Property(e => e.UserId).HasColumnName("userId");
                builder.Property(e => e.TimeOfDay).HasColumnName("timeOfDay");
                builder.Property(e => e.LastCompletedDate).HasColumnName("lastCompletedDate");
                builder.Property(e => e.LastShownDate).HasColumnName("lastShownDate");

                builder.Property(e => e.RepeatDays)
                    .HasColumnName("repeatDays")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => string.IsNullOrEmpty(v) ? new List<DayOfWeek>() : JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions)null),
                        new ValueComparer<List<DayOfWeek>>(
                            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                            c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c == null ? new List<DayOfWeek>() : c.ToList()
                        )
                    );
            });
        }
    }
}