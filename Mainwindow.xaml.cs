// Import core system types used throughout this file
using System;
// Import platform support attributes for OS-specific APIs
using System.Runtime.Versioning;
// Import WPF base types like Window, RoutedEventArgs
using System.Windows;
// Import WPF control types such as Button, TextBox, Border
using System.Windows.Controls;
// Import types for inline text elements used in chat bubbles
using System.Windows.Documents;
// Import input-related types like KeyEventArgs
using System.Windows.Input;
// Import media types for brushes, colors and fonts
using System.Windows.Media;
// Import the dispatcher for UI thread scheduling
using System.Windows.Threading;

// Declare the namespace for the application's UI classes
namespace CybersecurityBot
{
    // XML doc: describes the role of the main window's code-behind
    /// <summary>
    /// Code-behind for the WPF main window.
    /// Handles UI event wiring, message rendering, and bridges between
    /// the WPF presentation layer and the shared domain classes.
    /// </summary>
    // Indicate this class targets Windows-only APIs
    [SupportedOSPlatform("windows")]
    // Partial class backing the XAML-defined MainWindow
    public partial class MainWindow : Window
    {
        // Section: session-scoped state variables
        // ── Session state ─────────────────────────────────────────────────────

        // User profile captured after name entry (nullable until set)
        private UserProfile?         _user;
        // Conversation context instance used to track topics and memory
        private readonly ConversationContext _context      = new();
        // Flag indicating whether the user's name has been entered
        private bool                 _nameEntered  = false;
        // Timer used to update session duration and message count display
        private readonly DispatcherTimer    _sessionTimer = new();

        // Section: design tokens (colour palette) used for chat bubbles
        // ── Design tokens (colour palette) ────────────────────────────────────

        // Background brush for bot chat bubbles
        private static readonly SolidColorBrush BotBubble  = Brush(22,  27,  34);
        // Background brush for user chat bubbles
        private static readonly SolidColorBrush UserBubble = Brush(31,  41,  55);
        // Foreground brush for bot text
        private static readonly SolidColorBrush BotText    = Brush(88,  166, 255);
        // Foreground brush for user text
        private static readonly SolidColorBrush UserText   = Brush(230, 237, 243);
        // Foreground brush for system / metadata text
        private static readonly SolidColorBrush SystemText = Brush(139, 148, 158);
        // Helper to create a SolidColorBrush from RGB values
        private static SolidColorBrush Brush(byte r, byte g, byte b)
            => new(Color.FromRgb(r, g, b));

        // Section: constructor and load handling
        // ── Constructor ───────────────────────────────────────────────────────

        // Main window constructor
        public MainWindow()
        {
            // Initialize XAML components
            InitializeComponent();
            // Hook the Loaded event to perform additional setup
            Loaded += OnLoaded;
        }

        // Section: initialization performed after window load
        // ── Initialisation ────────────────────────────────────────────────────

        // Called when the window has finished loading
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Play an optional voice greeting
            PlayVoiceGreeting();
            // Render the ASCII art into the left panel
            RenderAsciiArt();
            // Populate the topic chips from the response engine
            PopulateTopicChips();
            // Wire the conversation context activity log to the UI
            WireActivityLog();
            // Start the session timer to update UI labels
            StartSessionTimer();
            // Show welcome/system messages in the chat
            ShowWelcomeMessages();
            // Focus the input box so the user can type immediately
            InputBox.Focus();
        }

        // Section: voice greeting helper
        // ── Voice greeting ───────────────────────────────────────────────────

        // Play the predefined voice greeting asynchronously (static helper)
        private static void PlayVoiceGreeting() => VoiceGreeting.Play();

        // Section: ASCII art rendering
        // ── ASCII art ─────────────────────────────────────────────────────────

        // Fill the AsciiArt TextBlock with a multi-line ASCII banner
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

        // Section: topic chip creation
        // ── Topic chips ───────────────────────────────────────────────────────

        // Create and attach clickable topic chips from ResponseEngine.TopicList
        private void PopulateTopicChips()
        {
            foreach (var topic in ResponseEngine.TopicList)
            {
                var chip = new Button
                {
                    // Display text for the chip comes from the topic string
                    Content = topic,
                    // Apply the TopicChip style from XAML resources
                    Style   = (Style)FindResource("TopicChip"),
                };

                // When a chip is clicked, populate input and send the topic
                chip.Click += (_, _) =>
                {
                    // Require name entry before allowing topic selection
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

                // Add the chip button into the topics panel
                TopicsPanel.Children.Add(chip);
            }
        }

        // Section: connect context activity events to UI logging
        // ── Activity log wiring ───────────────────────────────────────────────

        // Subscribe to ConversationContext.OnActivity and append entries to the ActivityLog
        private void WireActivityLog()
        {
            _context.OnActivity += entry =>
                Dispatcher.Invoke(() =>
                {
                    ActivityLog.Text += entry + "\n";
                    LogScroller.ScrollToEnd();
                });
        }

        // Section: session timer logic
        // ── Session timer ─────────────────────────────────────────────────────

        // Configure and start a timer that updates session duration and message count
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

        // Section: initial welcome messages shown on startup
        // ── Welcome messages ──────────────────────────────────────────────────

        // Add greeting and hints into the chat panel
        private void ShowWelcomeMessages()
        {
            AddBotMessage("👋 Welcome to the Cybersecurity Awareness Bot — Liam!");
            AddBotMessage("I'm here to help you stay informed and protected in the digital world. 🛡");
            AddSystemMessage(
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                "To get started, type your name below and press Enter or Send.\n" +
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        // Section: central message dispatch and conversation flow
        // ── Message dispatch ──────────────────────────────────────────────────

        // Process the user's typed input and drive bot responses
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

                // Create the UserProfile and mark name as entered
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

            // Memory recall handling
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

            // Follow-up request handling using last topic
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

            // Standard topic lookup and response
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

        // Section: show compact summary of available topics
        // ── Topic grid summary ────────────────────────────────────────────────

       // Display a single-line summary of all topics in the system
       private void ShowTopicGrid()
{
    string summary = string.Join("    ", ResponseEngine.TopicList);
    AddSystemMessage(summary);
}

        // Section: chat bubble construction helpers
        // ── Chat bubble builders ──────────────────────────────────────────────

        // Add a bot-styled message into the chat panel
        private void AddBotMessage(string text)
            => AddChatBubble($"🤖  Liam\n{text}", BotBubble, BotText, HorizontalAlignment.Left);

       // Add a user-styled message into the chat panel
       private void AddUserMessage(string text)
{
    string label = _user?.FormattedName ?? "You";   // Already safe, but maybe warning elsewhere
    AddChatBubble($"👤  {label}\n{text}", UserBubble, UserText, HorizontalAlignment.Right);
}

        // Add a centered system message (italic) into the chat panel
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

        // Create and add a styled chat bubble to the chat panel
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

        // Scroll the chat view to the bottom asynchronously
        private void ScrollToBottom()
            => Dispatcher.InvokeAsync(
                () => ChatScroller.ScrollToEnd(),
                DispatcherPriority.Background);

        // Section: miscellaneous helper predicates (not currently used externally)
        // ── Helpers ───────────────────────────────────────────────────────────

        // Determine whether the input text is an exit command
        private static bool IsExitCommand(String input)
        {
            string t = input.Trim().ToLowerInvariant();
            return t is "exit" or "quit" or "bye";
        }

        // Determine whether the input asks the bot to recall memory
        private static bool IsMemoryRecallRequest(String input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("what do you know about me")
                || lower.Contains("what have you remembered")
                || lower.Contains("what do you remember");
        }

        // Section: graceful exit sequence
        // ── Exit sequence ─────────────────────────────────────────────────────

        // Show a goodbye message, log the session end, wait then shutdown
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

        // Section: UI event handlers for buttons and input
        // ── UI event handlers ─────────────────────────────────────────────────

        // Click handler for the Send button that forwards to SendMessage
        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        // KeyDown handler for the input box: send on Enter
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        // Click handler for the Clear button: clear chat and re-show hints
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