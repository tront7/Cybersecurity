using System;
using System.IO;
using System.Media;
using System.Runtime.Versioning;

namespace CybersecurityBot
{
    [SupportedOSPlatform("windows")]
    public static class VoiceGreeting
    {
        private const string FileName = "greeting.wav";

        public static void Play()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
            if (!File.Exists(path))
                return;  // Silently skip – no console dependency

            try
            {
                using var p = new SoundPlayer(path);
                p.PlaySync();
            }
            catch (Exception)
            {
                // Ignore errors – we're in a WPF app, no console to write to
            }
        }
    }
}