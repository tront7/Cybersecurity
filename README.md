# 🛡 Cybersecurity Awareness Bot — Liam

A C# / WPF desktop application that educates users about cybersecurity best practices through
an interactive, keyword-driven chat interface. Designed to be extensible, professionally
structured, and easy to run on any Windows machine with .NET 9 installed.

---

## ✨ Features

| Feature | Detail |
|---|---|
| **Interactive chat** | Continuous conversation loop with keyword matching |
| **Sentiment detection** | Detects tone (positive, worried, confused, angry) and adapts replies |
| **Contextual memory** | Remembers mentioned device, browser, and security concerns |
| **Follow-up detection** | Recognises phrases like "tell me more" and expands on the last topic |
| **Session tracking** | Live session timer, message counter, and last-topic display |
| **Activity log** | Timestamped internal log of every significant event (delegate / event pattern) |
| **Topic chips** | Clickable buttons for all 18 major cybersecurity topics |
| **Voice greeting** | Optional `.wav` playback at startup |
| **ASCII branding** | Full terminal logo with colour and typing animation (console mode) |

---

## 📁 Project Structure

```
CybersecurityBot/
│
├── Program.cs              # Entry point — boots audio, logo, and chatbot
├── Chatbot.cs              # Console session orchestrator
├── UserProfile.cs          # Session data via auto-properties
├── ConversationContext.cs  # Memory, follow-up detection, activity log, delegate/event
├── ResponseEngine.cs       # Keyword → response dictionary with delegate selector
├── SentimentDetector.cs    # Tone detection and empathetic prefix selection
├── InputValidator.cs       # Input validation and name capture
├── UI.cs                   # Console colours, typing animation, ASCII logo
├── VoiceGreeting.cs        # Optional .wav playback
│
├── MainWindow.xaml         # WPF UI layout (sidebar + chat panel)
├── MainWindow.xaml.cs      # WPF code-behind — bridges UI to domain classes
├── App.xaml / App.xaml.cs  # WPF application host
│
├── CybersecurityBot.csproj # Project configuration (.NET 9, WPF)
├── greeting.wav            # (Optional) placed in bin/Debug/net9.0-windows/
└── README.md
```

---

## 🚀 Getting Started

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

---

## 🔉 Optional Voice Greeting

Place a file named `greeting.wav` in the build output folder:

```
bin/Debug/net9.0-windows/greeting.wav
```

If the file is absent, the application continues silently without error.

---

## 🗂 Covered Topics

| Emoji | Topic |
|-------|-------|
| 🔐 | Passwords |
| 🎣 | Phishing & Scams |
| 🌐 | Safe Browsing & Internet Safety |
| 🦠 | Malware |
| 🔏 | Privacy |
| 🎭 | Social Engineering |
| 🔑 | 2FA / MFA |
| 🔒 | VPN |
| 💰 | Ransomware |
| 📡 | Wi-Fi Security |
| 🔓 | Encryption |
| 🚨 | Data Breach |
| 🔴 | Hacking |
| 🔄 | Software Updates |
| 🛡 | Firewalls |
| 🪪 | Identity Theft |
| 📧 | Spam |

Each topic has multiple randomised responses for variety.

---

## 🧱 Architecture Notes

### Design patterns used

| Pattern | Where |
|---|---|
| **Delegate + Event** | `ConversationContext.OnActivity` — decoupled activity logging |
| **Strategy (delegate)** | `ResponseEngine.SelectResponse` — swappable response-selection logic |
| **Auto-properties** | `UserProfile`, `ConversationContext` — clean session state |
| **Sealed classes** | `Chatbot`, `UserProfile` — prevents unintended subclassing |
| **Static helpers** | `UI`, `InputValidator`, `SentimentDetector`, `ResponseEngine` — stateless utilities |

### Extending the response engine

Add a new entry to the `Responses` dictionary in `ResponseEngine.cs`:

```csharp
["your keyword"] = new[]
{
    "First alternative response.",
    "Second alternative response.",
},
```

Add the display label to `TopicList` and the UI chip will appear automatically.

---

## 🔭 Planned Improvements

- [ ] Integrate OpenAI / Claude API for true NLP responses
- [ ] Persist chat history and user preferences to disk
- [ ] Dark-mode theming toggle
- [ ] Multi-language support
- [ ] Export conversation transcript to PDF
- [ ] Web-based version (Blazor or React)

---

## 🛠 Technologies

| | |
|---|---|
| **Language** | C# 13 |
| **Framework** | .NET 9.0 (Windows) |
| **UI** | WPF (Windows Presentation Foundation) |
| **Audio** | `System.Media.SoundPlayer` |
| **CI** | GitHub Actions (`.github/workflows/dotnet.yml`) |

---

## 👤 Author

**Nemukongwe Oripfa Clinton**

Developed as a Cybersecurity Awareness Project to promote safe digital practices
through accessible, interactive learning.