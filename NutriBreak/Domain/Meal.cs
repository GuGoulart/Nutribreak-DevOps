using System;

namespace NutriBreak.Domain
{
    public class Meal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Calories { get; set; }
        public string TimeOfDay { get; set; } = string.Empty;

        public DateTime ConsumedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}