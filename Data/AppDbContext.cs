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
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- USER MAPPING (Vigtigt for Neon match) ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users"); // Mapper til den tabel du ser i Neon
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Username).HasColumnName("username");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.Role).HasColumnName("role");
                entity.Property(e => e.FamilyId).HasColumnName("familyid");
            });

            // --- StaticTask konfiguration ---
            modelBuilder.Entity<StaticTask>(builder =>
            {
                builder.ToTable("static_tasks"); // Vi sikrer os de også er små bogstaver
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

                builder.HasData(
                    new StaticTask { Id = 1, Title = "Tømme opvaskemaskine & flyde den igen", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null },
                    new StaticTask { Id = 2, Title = "Tørre støv af", IsCompleted = false, RepeatDays = new List<DayOfWeek>(), TimeOfDay = null }
                    // ... tilføj selv de resterende 15 her hvis nødvendigt
                );
            });

            // --- DynamicTask konfiguration ---
            modelBuilder.Entity<DynamicTask>(builder =>
            {
                builder.ToTable("dynamic_tasks");
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
            });
        }
    }
}