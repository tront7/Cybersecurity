#  Cybersecurity Awareness Bot — Liam (WPF Edition)

A C# / WPF desktop application that educates users about cybersecurity best practices through an interactive, keyword-driven chat interface. Designed to be extensible, professionally structured, and easy to run on any Windows machine with .NET 9 installed.

---

##  Features

| Feature | Detail |
|---------|--------|
| **Interactive chat** | Continuous conversation loop with keyword matching |
| **Sentiment detection** | Detects tone (positive, worried, confused, angry) and adapts replies |
| **Contextual memory** | Remembers mentioned device, browser, and security concerns |
| **Follow-up detection** | Recognises phrases like "tell me more" and expands on the last topic |
| **Session tracking** | Live session timer, message counter, and last-topic display |
| **Activity log** | Timestamped internal log of every significant event (delegate / event pattern) |
| **Topic chips** | Clickable buttons for all 18 major cybersecurity topics |
| **Voice greeting** | Optional `.wav` playback at startup |
| **Dark-themed UI** | GitHub‑style dark interface with custom chat bubbles and scrollbars |
| **ASCII branding** | Terminal‑style logo displayed in the left sidebar |

---

##  Project Structure

```
CybersecurityBot/
│
├── App.xaml / App.xaml.cs          # WPF application host (StartupUri = MainWindow.xaml)
├── MainWindow.xaml                 # WPF UI layout (sidebar + chat panel)
├── MainWindow.xaml.cs              # Code‑behind – bridges UI to domain classes
│
├── CommandHelper.cs                # Exit / memory‑recall detection
├── ConversationContext.cs          # Memory, follow‑up detection, activity log (delegate/event)
├── ResponseEngine.cs               # Keyword → response dictionary + delegate selector
├── SentimentDetector.cs            # Tone detection and empathetic prefix selection
├── UserProfile.cs                  # Session data (name, start time, formatted name)
├── VoiceGreeting.cs                # Optional .wav playback (System.Media.SoundPlayer)
│
├── CybersecurityBot.csproj         # .NET 9 WPF project configuration
├── greeting.wav                    # (Optional) placed in build output folder
└── README.md
```

> **Note:** This is a pure WPF application – no console‑based `Program.cs` or `Chatbot.cs` exist. All interaction happens in the graphical window.

---

##  Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (or higher)
- Windows OS (required for WPF and `System.Media.SoundPlayer`)
- Visual Studio 2022+ **or** VS Code with the C# extension

### Run with Visual Studio

1. Open `CybersecurityBot.csproj` in Visual Studio.
2. Press **F5** or click **Start**.
3. The WPF window will launch.

### Run with the .NET CLI

```bash
cd CybersecurityBot
dotnet restore
dotnet run
```

The application will start and display the main chat window.

---

##  Optional Voice Greeting

Place a file named `greeting.wav` in the build output folder:

```
bin/Debug/net9.0-windows/greeting.wav
```

If the file is absent, the application continues silently without error.

---

##  Covered Topics

| Topic     |  Topic                |
|-----------|-----------------------|
| Passwords |      Phishing & Scams |
| Safe Browsing & Internet Safety |  | Malware |
| Privacy |    Social Engineering |
| 2FA / MFA |  VPN |
| Ransomware |  Wi‑Fi Security |
| Encryption |  Data Breach |
| Hacking |  Software Updates |
| Firewalls |  Identity Theft |
| Spam | | |

Each topic has multiple randomised responses for variety.

---

##  Architecture Notes

### Design patterns used

| Pattern | Where |
|---------|-------|
| **Delegate + Event** | `ConversationContext.OnActivity` — decoupled activity logging |
| **Strategy (delegate)** | `ResponseEngine.SelectResponse` — swappable response‑selection logic |
| **Auto‑properties** | `UserProfile`, `ConversationContext` — clean session state |
| **Sealed classes** | `UserProfile`, `ConversationContext` — prevents unintended subclassing |
| **Static helpers** | `CommandHelper`, `SentimentDetector`, `ResponseEngine` — stateless utilities |

### Extending the response engine

Add a new entry to the `Responses` dictionary in `ResponseEngine.cs`:

```csharp
["your keyword"] = new[]
{
    "First alternative response.",
    "Second alternative response.",
},
```

Then add the display label to `TopicList` – the UI chip will appear automatically.

---

##  Planned Improvements

- [ ] Integrate OpenAI / Claude API for true NLP responses
- [ ] Persist chat history and user preferences to disk
- [ ] Dark‑mode theming toggle (already dark, but a light mode could be added)
- [ ] Multi‑language support
- [ ] Export conversation transcript to PDF
- [ ] Web‑based version (Blazor or React)

---

##  Technologies

| | |
|---|---|
| **Language** | C# 13 |
| **Framework** | .NET 9.0 (Windows) |
| **UI** | WPF (Windows Presentation Foundation) |
| **Audio** | `System.Media.SoundPlayer` |
| **CI** | GitHub Actions (`.github/workflows/dotnet.yml`) |

---

##  Author

**Nemukongwe Oripfa Clinton**

Developed as a Cybersecurity Awareness Project to promote safe digital practices through accessible, interactive learning.