using System;

namespace CybersecurityBot
{
    public sealed class UserProfile
    {
        public string   Name         { get; }
        public DateTime SessionStart { get; }

        public string FormattedName =>
            string.IsNullOrWhiteSpace(Name)
                ? "User"
                : char.ToUpperInvariant(Name[0]) + Name[1..].ToLowerInvariant();

        public string TimeGreeting => SessionStart.Hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _    => "Good evening",
        };

        public string SessionDuration
        {
            get
            {
                var e = DateTime.Now - SessionStart;
                if (e.TotalSeconds < 60)  return "less than a minute";
                if (e.TotalMinutes < 60)  return $"{(int)e.TotalMinutes} minute(s)";
                return $"{(int)e.TotalHours} hour(s) and {e.Minutes} minute(s)";
            }
        }

        public UserProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            Name = name.Trim();
        }
    }
}
