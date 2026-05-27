using System;
using System.Collections.Generic;

namespace CybersecurityBot
{
    /// <summary>
    /// Tracks conversation state: last matched topic, message count, contextual
    /// memory (device / browser / concern), follow-up detection, and an activity-log event.
    /// </summary>
    public sealed class ConversationContext
    {
        // ── Delegate & event ──────────────────────────────────────────────────

        public delegate void ActivityLogHandler(string message);
        public event ActivityLogHandler? OnActivity;

        // ── Properties ────────────────────────────────────────────────────────

        public string LastTopic    { get; private set; } = string.Empty;
        public int    MessageCount { get; private set; } = 0;
        public string UserConcern  { get; private set; } = string.Empty;
        public string UserDevice   { get; private set; } = string.Empty;
        public string UserBrowser  { get; private set; } = string.Empty;

        // ── Follow-up phrases ─────────────────────────────────────────────────

        private static readonly HashSet<string> FollowUpPhrases = new(StringComparer.OrdinalIgnoreCase)
        {
            "tell me more", "more info", "explain more", "give me more",
            "another tip",  "more tips", "elaborate",    "go on",
            "continue",     "what else", "anything else", "more please",
            "expand",       "keep going", "go deeper",
        };

        // ── Public API ────────────────────────────────────────────────────────

        public bool IsFollowUp(string input)
        {
            string lower = input.Trim().ToLowerInvariant();
            foreach (var phrase in FollowUpPhrases)
                if (lower.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        public void RecordMessage(string input, string topic)
        {
            MessageCount++;
            LastTopic = topic;
            ExtractMemory(input);
            Fire($"User asked about: {topic}");
        }

        public void Log(string message) => Fire(message);

        public string BuildMemoryRecap()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(UserDevice))  parts.Add($"you're on {UserDevice}");
            if (!string.IsNullOrEmpty(UserBrowser)) parts.Add($"you use {UserBrowser}");
            if (!string.IsNullOrEmpty(UserConcern)) parts.Add($"you're concerned about {UserConcern}");
            return parts.Count > 0 ? string.Join(", ", parts) : string.Empty;
        }

        // ── Memory extraction ─────────────────────────────────────────────────

        private void ExtractMemory(string input)
        {
            string lower = input.ToLowerInvariant();

            if      (lower.Contains("windows"))                          SetDevice("Windows");
            else if (lower.Contains("mac") || lower.Contains("macos"))  SetDevice("Mac");
            else if (lower.Contains("android"))                          SetDevice("Android");
            else if (lower.Contains("iphone") || lower.Contains("ios")) SetDevice("iPhone / iOS");
            else if (lower.Contains("linux"))                            SetDevice("Linux");

            if      (lower.Contains("chrome"))  SetBrowser("Chrome");
            else if (lower.Contains("firefox")) SetBrowser("Firefox");
            else if (lower.Contains("edge"))    SetBrowser("Edge");
            else if (lower.Contains("safari"))  SetBrowser("Safari");
            else if (lower.Contains("brave"))   SetBrowser("Brave");

            if      (lower.Contains("hacked"))     SetConcern("being hacked");
            else if (lower.Contains("scammed"))    SetConcern("being scammed");
            else if (lower.Contains("phishing"))   SetConcern("phishing");
            else if (lower.Contains("malware"))    SetConcern("malware");
            else if (lower.Contains("ransomware")) SetConcern("ransomware");
            else if (lower.Contains("password"))   SetConcern("password security");
            else if (lower.Contains("privacy"))    SetConcern("privacy");
        }

        private void SetDevice(string v)  { if (UserDevice  != v) { UserDevice  = v; Fire($"Remembered: device → {v}");  } }
        private void SetBrowser(string v) { if (UserBrowser != v) { UserBrowser = v; Fire($"Remembered: browser → {v}"); } }
        private void SetConcern(string v) { if (UserConcern != v) { UserConcern = v; Fire($"Remembered: concern → {v}"); } }

        private void Fire(string message) =>
            OnActivity?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}