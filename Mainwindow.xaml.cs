using System;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CybersecurityBot
{
    /// <summary>
    /// Code-behind for the WPF main window.
    /// Handles UI event wiring, message rendering, and bridges between
    /// the WPF presentation layer and the shared domain classes.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class MainWindow : Window
    {
        // ── Session state ─────────────────────────────────────────────────────

        private UserProfile?         _user;
        private readonly ConversationContext _context      = new();
        private bool                 _nameEntered  = false;
        private readonly DispatcherTimer    _sessionTimer = new();

        // ── Design tokens (colour palette) ────────────────────────────────────

        private static readonly SolidColorBrush BotBubble  = Brush(22,  27,  34);
        private static readonly SolidColorBrush UserBubble = Brush(31,  41,  55);
        private static readonly SolidColorBrush BotText    = Brush(88,  166, 255);
        private static readonly SolidColorBrush UserText   = Brush(230, 237, 243);
        private static readonly SolidColorBrush SystemText = Brush(139, 148, 158);
        private static SolidColorBrush Brush(byte r, byte g, byte b)
            => new(Color.FromRgb(r, g, b));

        // ── Constructor ───────────────────────────────────────────────────────

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        // ── Initialisation ────────────────────────────────────────────────────

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PlayVoiceGreeting();
            RenderAsciiArt();
            PopulateTopicChips();
            WireActivityLog();
            StartSessionTimer();
            ShowWelcomeMessages();
            InputBox.Focus();
        }

        // ── Voice greeting ────────────────────────────────────────────────────

        private static void PlayVoiceGreeting() => VoiceGreeting.Play();

        // ── ASCII art ─────────────────────────────────────────────────────────

        private void RenderAsciiArt()
        {
            AsciiArt.Text =
                " ██████╗██╗   ██╗██████╗ \n" +
                "██╔════╝╚██╗ ██╔╝██╔══██╗\n" +
                "██║      ╚████╔╝ ██████╔╝\n" +
                "██║       ╚██╔╝  ██╔══██╗\n" +
                "╚██████╗   ██║   ██████╔╝\n" +
                " ╚═════╝   ╚═╝   ╚═════╝ \n" +
                "                         \n" +
                "██╗     ██╗ █████╗ ███╗  \n" +
                "██║     ██║██╔══██╗████╗ \n" +
                "██║     ██║███████║██╔██╗\n" +
                "██║     ██║██╔══██║██║╚██\n" +
                "███████╗██║██║  ██║██║ ╚═\n" +
                "╚══════╝╚═╝╚═╝  ╚═╝╚═╝  ";
        }

        // ── Topic chips ───────────────────────────────────────────────────────

        private void PopulateTopicChips()
        {
            foreach (var topic in ResponseEngine.TopicList)
            {
                var chip = new Button
                {
                    Content = topic,
                    Style   = (Style)FindResource("TopicChip"),
                };

                chip.Click += (_, _) =>
                {
                    if (!_nameEntered)
                    {
                        AddSystemMessage("⚠  Please enter your name first before selecting a topic.");
                        InputBox.Focus();
                        return;
                    }

                    // Strip the leading emoji (first 2 chars + optional space)
                    string keyword = topic.Length > 2 ? topic[2..].Trim() : topic;
                    InputBox.Text = keyword;
                    SendMessage();
                };

                TopicsPanel.Children.Add(chip);
            }
        }

        // ── Activity log wiring ───────────────────────────────────────────────

        private void WireActivityLog()
        {
            _context.OnActivity += entry =>
                Dispatcher.Invoke(() =>
                {
                    ActivityLog.Text += entry + "\n";
                    LogScroller.ScrollToEnd();
                });
        }

        // ── Session timer ─────────────────────────────────────────────────────

        private void StartSessionTimer()
        {
            _sessionTimer.Interval = TimeSpan.FromSeconds(1);
            _sessionTimer.Tick += (_, _) =>
            {
                if (_user is not null)
                    SessionLabel.Text =
                        $"⏱ {_user.SessionDuration}  |  💬 {_context.MessageCount} messages";
            };
            _sessionTimer.Start();
        }

        // ── Welcome messages ──────────────────────────────────────────────────

        private void ShowWelcomeMessages()
        {
            AddBotMessage("👋 Welcome to the Cybersecurity Awareness Bot — Liam!");
            AddBotMessage("I'm here to help you stay informed and protected in the digital world. 🛡");
            AddSystemMessage(
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                "To get started, type your name below and press Enter or Send.\n" +
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        // ── Message dispatch ──────────────────────────────────────────────────

        private void SendMessage()
        {
            string input = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            InputBox.Clear();

            // Exit command — always evaluated first
            if (CommandHelper.IsExit(input))
            {
                HandleExit();
                return;
            }

            // ── Phase 1: Name capture ─────────────────────────────────────────
            if (!_nameEntered)
            {
                if (!InputValidator.IsValidName(input))
                {
                    AddSystemMessage("⚠  Name must be at least 2 characters. Please try again.");
                    return;
                }

                _user        = new UserProfile(input);
                _nameEntered = true;

                _context.Log($"Session started for {_user.FormattedName}");

                AddUserMessage(input);
                AddBotMessage($"{_user.TimeGreeting}, {_user.FormattedName}! 👋 Great to meet you.");
                AddBotMessage(
                    "Click any topic chip on the left, or type your question below.\n\n" +
                    "💡 Special commands:\n" +
                    "  • 'tell me more'               — more detail on the last topic\n" +
                    "  • 'what do you know about me'  — see what I've remembered\n" +
                    "  • 'help'                       — list all available topics\n" +
                    "  • 'exit', 'quit', or 'bye'     — close the application");

                ShowTopicGrid();
                StatusLabel.Text = $"Chatting with {_user.FormattedName}";
                return;
            }

            // ── Phase 2: Main conversation ────────────────────────────────────
            AddUserMessage(input);

            // Sentiment detection
            var    sentiment = SentimentDetector.Detect(input);
            string prefix    = SentimentDetector.GetPrefix(sentiment);
            string emoji     = SentimentDetector.GetEmoji(sentiment);

            // Memory recall
            if (CommandHelper.IsMemoryRecall(input))
            {
                string recap = _context.BuildMemoryRecap();
                AddBotMessage(string.IsNullOrEmpty(recap)
                
                    ? $"I haven't learned much about you yet, {_user.FormattedName}!\n" +
                      "Mention your device, browser, or a security concern and I'll remember it."
                    : $"Based on our conversation, I know that {recap}.\n" +
                      "Is there anything specific I can help you with regarding these?");
                _context.Log("Memory recall requested");
                return;
            }

            // Follow-up request
            if (_context.IsFollowUp(input) && !string.IsNullOrEmpty(_context.LastTopic))
            {
                string? followUp = ResponseEngine.GetResponse(_context.LastTopic);
                if (followUp is not null)
                {
                    string message = string.IsNullOrEmpty(prefix)
                        ? $"Here's more on '{_context.LastTopic}':\n\n{followUp}"
                        : $"{prefix}Here's more on '{_context.LastTopic}':\n\n{followUp}";
                    AddBotMessage($"{emoji} {message}");
                    _context.Log($"Follow-up delivered for: {_context.LastTopic}");
                    return;
                }
            }

            // Standard topic response
            string? response = ResponseEngine.GetResponse(input);
            string? topicKey = ResponseEngine.GetMatchedTopicKey(input);

            if (topicKey is not null)
                _context.RecordMessage(input, topicKey);

            if (response is not null)
            {
                string full = string.IsNullOrEmpty(prefix) ? response : $"{prefix}\n{response}";
                AddBotMessage($"{emoji} {full}");

                // Periodic memory recap hint (every 4 messages)
                string recap = _context.BuildMemoryRecap();
                if (!string.IsNullOrEmpty(recap) && _context.MessageCount % 4 == 0)
                    AddSystemMessage($"💭 Remembered: {recap}");
            }
            else
            {
                AddBotMessage(
                    $"I didn't quite catch that, {_user.FormattedName}. 🤔\n\n" +
                    "Try asking about a topic from the panel on the left, or type things like:\n" +
                    "  • \"How do I create a strong password?\"\n" +
                    "  • \"Tell me about phishing\"\n" +
                    "  • \"What is ransomware?\"\n" +
                    "  • \"Give me a VPN tip\"\n\n" +
                    "Type 'help' to see all available topics.");
                _context.Log("Unrecognised input");
            }
        }

        // ── Topic grid summary ────────────────────────────────────────────────

       private void ShowTopicGrid()
{
    string summary = string.Join("    ", ResponseEngine.TopicList);
    AddSystemMessage(summary);
}

        // ── Chat bubble builders ──────────────────────────────────────────────

        private void AddBotMessage(string text)
            => AddChatBubble($"🤖  Liam\n{text}", BotBubble, BotText, HorizontalAlignment.Left);

       private void AddUserMessage(string text)
{
    string label = _user?.FormattedName ?? "You";   // Already safe, but maybe warning elsewhere
    AddChatBubble($"👤  {label}\n{text}", UserBubble, UserText, HorizontalAlignment.Right);
}

        private void AddSystemMessage(string text)
        {
            var block = new TextBlock
            {
                Text                = text,
                Foreground          = SystemText,
                FontFamily          = new FontFamily("Segoe UI"),
                FontSize            = 11,
                FontStyle           = FontStyles.Italic,
                TextWrapping        = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin              = new Thickness(0, 6, 0, 6),
            };

            ChatPanel.Children.Add(block);
            ScrollToBottom();
        }

        private void AddChatBubble(
            string text,
            SolidColorBrush background,
            SolidColorBrush foreground,
            HorizontalAlignment alignment)
        {
            bool isRight = alignment == HorizontalAlignment.Right;

            var border = new Border
            {
                Background          = background,
                CornerRadius        = new CornerRadius(10),
                Padding             = new Thickness(14, 10, 14, 10),
                Margin              = new Thickness(isRight ? 80 : 0, 4, isRight ? 0 : 80, 4),
                HorizontalAlignment = alignment,
                MaxWidth            = 620,
            };

            var block = new TextBlock
            {
                Foreground   = foreground,
                FontFamily   = new FontFamily("Consolas"),
                FontSize     = 12.5,
                TextWrapping = TextWrapping.Wrap,
                LineHeight   = 19,
            };

            // Split the sender label from the message body for distinct styling
            var parts = text.Split('\n', 2);
            if (parts.Length == 2)
            {
                block.Inlines.Add(new Run(parts[0] + "\n")
                {
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize   = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = SystemText,
                });
                block.Inlines.Add(new Run(parts[1]));
            }
            else
            {
                block.Text = text;
            }

            border.Child = block;
            ChatPanel.Children.Add(border);
            ScrollToBottom();
        }

        private void ScrollToBottom()
            => Dispatcher.InvokeAsync(
                () => ChatScroller.ScrollToEnd(),
                DispatcherPriority.Background);

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool IsExitCommand(String input)
        {
            string t = input.Trim().ToLowerInvariant();
            return t is "exit" or "quit" or "bye";
        }

        private static bool IsMemoryRecallRequest(String input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("what do you know about me")
                || lower.Contains("what have you remembered")
                || lower.Contains("what do you remember");
        }

        // ── Exit sequence ─────────────────────────────────────────────────────

        private async void HandleExit()
        {
            string name = _user is not null ? $", {_user.FormattedName}" : string.Empty;
            AddBotMessage(
                $"👋 Goodbye{name}! Stay safe out there. 🛡\n" +
                "The application will close in 3 seconds…");
            _context.Log($"Session ended{name}");

            await System.Threading.Tasks.Task.Delay(3_000);
            Application.Current.Shutdown();
        }

        // ── UI event handlers ─────────────────────────────────────────────────

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            _context.Log("Chat cleared by user");

            if (_nameEntered && _user is not null)
                AddBotMessage(
                    $"Chat cleared! What else can I help you with, {_user.FormattedName}?\n\n" +
                    "Type 'help' to see all available topics, or pick one from the left panel.");
            else
                ShowWelcomeMessages();
        }
    }
}