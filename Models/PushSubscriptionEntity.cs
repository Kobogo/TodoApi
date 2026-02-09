using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class PushSubscriptionEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        // Selve URL'en til Google/Apples push-server
        [Required]
        public string Endpoint { get; set; } = string.Empty;

        // Krypteringsnøgle 1
        public string P256dh { get; set; } = string.Empty;

        // Krypteringsnøgle 2
        public string Auth { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Denne hjælpe-klasse bruges til at modtage JSON-data fra React
    public class PushSubscriptionJSON
    {
        public string Endpoint { get; set; } = string.Empty;
        public KeysJSON Keys { get; set; } = new KeysJSON();
    }

    public class KeysJSON
    {
        public string P256dh { get; set; } = string.Empty;
        public string Auth { get; set; } = string.Empty;
    }
}