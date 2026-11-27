# Seasons: Football Career & League Simulator

Multi-language football career game (**C#, C, C++, Java, PHP, JavaScript, HTML, CSS**) featuring:
- **Full Player Customizer**: Choose your player's name, nationality (32 countries), preferred foot, height, kit number, skin tone, hairstyle, hair color, facial hair, and boot color with a live SVG avatar preview.
- **Real European Leagues & 60 Real Clubs**: Start at any club across the **Premier League, LaLiga, Serie A, Bundesliga, or Ligue 1**, and transfer between clubs at the start of any season.
- **Full League Simulator & Playable Live Matches**: Simulate matchday-by-matchday with a live 12-team home-and-away league table, or jump onto the **interactive 2D pitch** to dribble, tackle, assist, and score goals yourself!

## Project Structure

| Language | Files | Role |
|---|---|---|
| C# | `Program.cs`, `Player.cs`, `LeagueData.cs`, `LeagueSimulator.cs`, `Progression.cs`, `MarketValue.cs`, `Native.cs` | Game rules, league simulator, matchday endpoints, web server |
| C++ / C | `native/market_value.cpp`, `native/curves.c`, `native/curves.h` | Native market value engine via P/Invoke |
| Java | `java/NewsServer.java` | Dynamic headlines mentioning your player, club, and nationality |
| PHP | `php/index.php` | JSON career save/load service |
| HTML/CSS/JS | `wwwroot/index.html`, `wwwroot/style.css`, `wwwroot/app.js`, `wwwroot/audio.js` | Player customizer, live SVG avatar, league table, interactive pitch, Web Audio synth |

## How to Run

1. Install **.NET 8 SDK** (required). Optionally install **JDK 17+**, **PHP 8+**, and **MinGW-w64 (`g++`/`gcc`)**.
2. Run **`start-all.bat`** (Windows) or **`./start-all.sh`** (Linux/macOS).
3. Open **http://localhost:5000** in your browser.
