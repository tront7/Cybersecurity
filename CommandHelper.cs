using System;

namespace CybersecurityBot
{
    public static class CommandHelper
    {
        public static bool IsExit(string input)
        {
            string t = input.Trim().ToLowerInvariant();
            return t is "exit" or "quit" or "bye";
        }

        public static bool IsMemoryRecall(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("what do you know about me") ||
                   lower.Contains("what have you remembered") ||
                   lower.Contains("what do you remember");
        }
    }

    public static class InputValidator
    {
        public const int MinNameLength = 2;
        public static bool IsValidName(string? name) =>
            !string.IsNullOrWhiteSpace(name) && name.Trim().Length >= MinNameLength;
    }
}