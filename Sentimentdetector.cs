using System;
using System.Collections.Generic;

namespace CybersecurityBot
{
    public static class SentimentDetector
    {
        public enum Sentiment { Positive, Negative, Worried, Confused, Angry, Neutral }

        private static readonly IReadOnlyDictionary<Sentiment, string[]> Keywords =
            new Dictionary<Sentiment, string[]>
            {
                [Sentiment.Positive] = new[] { "great", "thanks", "thank you", "awesome", "love", "cool",
                                               "good", "helpful", "nice", "excellent", "perfect", "amazing",
                                               "brilliant", "fantastic", "wonderful", "appreciate", "glad" },
                [Sentiment.Negative] = new[] { "scared", "afraid", "fear", "terrified", "nervous",
                                               "stressed", "worried", "anxious", "unsafe", "vulnerable", "overwhelmed" },
                [Sentiment.Confused] = new[] { "confused", "don't understand", "not sure", "what is",
                                               "what does", "explain", "how does", "help me understand",
                                               "lost", "unclear", "complicated", "i don't get", "what are" },
                [Sentiment.Angry]    = new[] { "angry", "frustrated", "annoyed", "hate", "stupid",
                                               "useless", "terrible", "worst", "awful", "ridiculous", "fed up" },
                [Sentiment.Worried]  = new[] { "hacked", "attacked", "breached", "stolen", "leaked",
                                               "compromised", "victim", "my account", "someone got in", "suspicious" },
            };

        private static readonly IReadOnlyDictionary<Sentiment, string[]> Prefixes =
            new Dictionary<Sentiment, string[]>
            {
                [Sentiment.Positive] = new[] { "I'm glad you're feeling positive! ",
                                               "Great to hear! ",
                                               "Love the enthusiasm! " },
                [Sentiment.Negative] = new[] { "I understand this can feel overwhelming — you're not alone. ",
                                               "It's completely normal to feel nervous about this. ",
                                               "Don't worry, I'm here to help you through this. " },
                [Sentiment.Confused] = new[] { "No worries — let me break this down simply for you. ",
                                               "Great question. Let me explain that clearly. ",
                                               "Happy to clarify. Here's a simple explanation: " },
                [Sentiment.Angry]    = new[] { "I hear your frustration — let me help sort this out. ",
                                               "I'm sorry you're feeling this way. Let me do my best to help. ",
                                               "I understand — let's tackle this together. " },
                [Sentiment.Worried]  = new[] { "That sounds serious — let's address this right away. ",
                                               "I can hear your concern. Here's what you should do: ",
                                               "Don't panic — here are the steps to take immediately: " },
                [Sentiment.Neutral]  = new[] { "", "", "" },
            };

        private static readonly Random _rng = new();

        public static Sentiment Detect(string input)
        {
            string lower = input.ToLowerInvariant();
            foreach (var (sentiment, words) in Keywords)
                foreach (var word in words)
                    if (lower.Contains(word, StringComparison.OrdinalIgnoreCase))
                        return sentiment;
            return Sentiment.Neutral;
        }

        public static string GetPrefix(Sentiment s)
        {
            var opts = Prefixes[s];
            return opts[_rng.Next(opts.Length)];
        }

        public static string GetEmoji(Sentiment s) => s switch
        {
            Sentiment.Positive => "😊",
            Sentiment.Negative => "😟",
            Sentiment.Confused => "🤔",
            Sentiment.Angry    => "😤",
            Sentiment.Worried  => "⚠️",
            _                  => "🤖",
        };
    }
}