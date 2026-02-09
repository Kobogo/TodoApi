using TodoApi.Models;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace TodoApi.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context, IConfiguration configuration)
        {
            // SKIFT DETTE: EnsureCreated() er upålidelig med schemas.
            // Brug Migrate() i Program.cs som vi aftalte,
            // eller kør den her for at være sikker:
            context.Database.Migrate();

            // 1. Tjek om admin-brugeren findes
            var seedName = configuration["SEED_USER_NAME"] ?? "Far";

            // Vi pakker det ind i en Try-Catch eller sikrer os tabellen er der
            if (context.Users.Any(u => u.Username == seedName)) return;

            // 2. Hent password
            var seedPass = configuration["SEED_USER_PASS"] ?? "1234";

            // 3. Hash passwordet
            string hashedContext = BCrypt.Net.BCrypt.HashPassword(seedPass);

            var adminUser = new User
            {
                Username = seedName,
                PasswordHash = hashedContext,
                Role = "Admin",
                FamilyId = 1,
                FamilyName = "Bang",
                TotalPoints = 0,
                SavingsBalance = 0
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }
}