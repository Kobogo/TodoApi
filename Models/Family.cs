using System;
using System.Collections.Generic;

namespace TodoApi.Models
{
    public class Family
    {
        public int Id { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relation: En familie har mange brugere
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}