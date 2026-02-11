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


        public DbSet<PushSubscriptionEntity> PushSubscriptions { get; set; } = null!;
        public DbSet<StaticTask> StaticTasks { get; set; } = null!;
        public DbSet<DynamicTask> DynamicTasks { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<TaskLog> TaskLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TodoApi");
            modelBuilder.UseSerialColumns();
            base.OnModelCreating(modelBuilder);

            // --- FAMILY MAPPING ---
            modelBuilder.Entity<Family>(entity =>
            {
                entity.ToTable("families");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.FamilyName).HasColumnName("familyName").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            // --- USER MAPPING ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Username).HasColumnName("username");
                entity.Property(e => e.PasswordHash).HasColumnName("passwordHash");
                entity.Property(e => e.Role).HasColumnName("role");
                entity.Property(e => e.FamilyId).HasColumnName("familyId");
                entity.Property(e => e.TotalPoints).HasColumnName("totalPoints");
                entity.Property(e => e.SavingsBalance).HasColumnName("savingsBalance");
                entity.Property(e => e.FamilyName).HasColumnName("familyName");

                // RELATION: User -> Family
                entity.HasOne<Family>()
                    .WithMany(f => f.Users)
                    .HasForeignKey(e => e.FamilyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- PUSH SUBSCRIPTION MAPPING ---
            modelBuilder.Entity<PushSubscriptionEntity>(entity =>
            {
                entity.ToTable("pushSubscriptions");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("userId");
                entity.Property(e => e.Endpoint).HasColumnName("endpoint");
                entity.Property(e => e.P256dh).HasColumnName("p256dh");
                entity.Property(e => e.Auth).HasColumnName("auth");
                entity.Property(e => e.CreatedAt).HasColumnName("createdAt");

                // RELATION: PushSubscription -> User
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- STATICTASK MAPPING ---
            modelBuilder.Entity<StaticTask>(builder =>
            {
                builder.ToTable("staticTasks");
                builder.Property(e => e.Id).HasColumnName("id");
                builder.Property(e => e.UserId).HasColumnName("userId");
                builder.Property(e => e.Title).HasColumnName("title");
                builder.Property(e => e.IsCompleted).HasColumnName("isCompleted");
                builder.Property(e => e.TimeOfDay).HasColumnName("timeOfDay");
                builder.Property(e => e.LastCompletedDate).HasColumnName("lastCompletedDate");
                builder.Property(e => e.LastShownDate).HasColumnName("lastShownDate");

                // RELATION: StaticTask -> User
                builder.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

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

                builder.HasData(
                    new StaticTask { Id = 1, Title = "Tømme opvaskemaskine & flyde den igen", IsCompleted = false },
                    new StaticTask { Id = 2, Title = "Tørre støv af", IsCompleted = false },
                    new StaticTask { Id = 3, Title = "Dække bord + tørre bord af", IsCompleted = false },
                    new StaticTask { Id = 4, Title = "Støvsuge hele huset", IsCompleted = false },
                    new StaticTask { Id = 5, Title = "Vaske gulv", IsCompleted = false },
                    new StaticTask { Id = 6, Title = "Hænge vasketøj op med en voksen", IsCompleted = false },
                    new StaticTask { Id = 7, Title = "Skylle af efter aftensmaden", IsCompleted = false },
                    new StaticTask { Id = 8, Title = "Pille tøj ned af tørrestativet", IsCompleted = false },
                    new StaticTask { Id = 9, Title = "Lægge tøj sammen + lægge tøj på plads med en voksen", IsCompleted = false },
                    new StaticTask { Id = 10, Title = "Tømme skraldespande", IsCompleted = false },
                    new StaticTask { Id = 11, Title = "Ordne badeværelser med en voksen", IsCompleted = false },
                    new StaticTask { Id = 12, Title = "Slå græs", IsCompleted = false },
                    new StaticTask { Id = 13, Title = "Fejrne ukrudt (min. 1 spand)", IsCompleted = false },
                    new StaticTask { Id = 14, Title = "Være med til at lave aftensmad", IsCompleted = false },
                    new StaticTask { Id = 15, Title = "Fylde op i køleskab med sodavand", IsCompleted = false },
                    new StaticTask { Id = 16, Title = "Give kattene mad", IsCompleted = false },
                    new StaticTask { Id = 17, Title = "Rede senge (Alle)", IsCompleted = false }
                );
            });

            // --- DYNAMICTASK MAPPING ---
            modelBuilder.Entity<DynamicTask>(builder =>
            {
                builder.ToTable("dynamicTasks");
                builder.Property(e => e.Id).HasColumnName("id");
                builder.Property(e => e.UserId).HasColumnName("userId");
                builder.Property(e => e.Title).HasColumnName("title");
                builder.Property(e => e.IsCompleted).HasColumnName("isCompleted");
                builder.Property(e => e.TimeOfDay).HasColumnName("timeOfDay");
                builder.Property(e => e.LastCompletedDate).HasColumnName("lastCompletedDate");
                builder.Property(e => e.LastShownDate).HasColumnName("lastShownDate");

                // RELATION: DynamicTask -> User
                builder.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

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

            // --- SEED DATA: FAMILIE ---
            modelBuilder.Entity<Family>().HasData(
                new Family {
                    Id = 1,
                    FamilyName = "Bang",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // --- TASKLOG MAPPING ---
            modelBuilder.Entity<TaskLog>(entity =>
            {
                entity.ToTable("taskLogs");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("userId");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.TasksCompleted).HasColumnName("tasksCompleted");
                entity.Property(e => e.DailyGoal).HasColumnName("dailyGoal");
                entity.Property(e => e.PointsEarned).HasColumnName("pointsEarned");

                // Relation: TaskLog -> User
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}