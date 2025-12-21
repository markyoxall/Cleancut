using System;

namespace CleanCut.Infrastructure.Data.Entities
{
    /// <summary>
    /// Database entity for storing user preferences per module.
    /// </summary>
    public class UserPreferenceEntity
    {
        public string ModuleName { get; set; } = string.Empty; // primary key
        public string PayloadJson { get; set; } = string.Empty; // serialized UserPreferences
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
