using System;

namespace CleanCut.Infrastructure.Data.Entities
{
    /// <summary>
    /// Database entity for storing user preferences per module and user.
    /// </summary>
    public class UserPreferenceEntity
    {
        /// <summary>
        /// Module name (part of composite primary key).
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// Application user name / identifier (part of composite primary key).
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Serialized JSON payload containing the preferences.
        /// </summary>
        public string PayloadJson { get; set; } = string.Empty;

        /// <summary>
        /// When the preference was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the preference was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
