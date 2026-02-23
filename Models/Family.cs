using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models
{
    public class Family
    {
        public int Id { get; set; }

        [Column("familyName")]
        public string FamilyName { get; set; } = string.Empty;

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relation: En familie har mange brugere
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}