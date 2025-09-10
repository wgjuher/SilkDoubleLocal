# SilkMod - Silksong Translation Display Mod

A BepInEx mod for Hollow Knight: Silksong that intercepts English strings and displays both English and Russian translations in-game by concatenating them directly into the game text.

## Features

- **In-Game Text Concatenation**: Modifies game text to show both English and Russian translations inline
- **Harmony Patching**: Intercepts `Language.Get()` calls to capture and modify localized strings
- **Smart Filtering**: Processes specific content types with configurable sheet filtering
- **Delayed Patching**: Waits for game initialization before applying patches for stability
- **File Logging**: Saves translation pairs to a timestamped log file
- **Page-Aware Processing**: Handles multi-page dialogue with proper formatting
- **Error Handling**: Robust error handling with automatic language restoration

## How It Works

1. **Delayed Initialization**: Waits 5 seconds and for scene loading before applying patches
2. **Interception**: Patches `TeamCherry.Localization.Language.Get(string key, string sheetTitle)` using Harmony
3. **Translation Fetch**: Temporarily switches to Russian locale to fetch translations
4. **Text Concatenation**: Combines English and Russian text with `<br>` separator
5. **In-Game Display**: Modified text appears directly in game dialogue and UI
6. **Logging**: Translation pairs are logged to console and file with timestamps

## Installation

1. Install BepInEx for Hollow Knight: Silksong
2. Build this mod or download the compiled DLL from GitHub Actions
3. Place `SilkMod.dll` in `BepInEx/plugins/SilkMod/` folder
4. Launch the game

## Building

### Prerequisites
- .NET 6.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension

### Local Development
1. Clone the repository
2. Build with:
```bash
dotnet restore
dotnet build --configuration Release
```


### Dependencies
- .NET Standard 2.1
- BepInEx 5.4.21
- HarmonyX 2.10.2
- UnityEngine.Modules 6000.0.50
- Silksong.GameLibs 1.0.1-silksong1.0.28324

### CI/CD
The project includes GitHub Actions workflow that automatically builds the mod on push/PR to main branch.

## Configuration

The mod includes several configurable options in the source code:

- `enableFileLogging`: Whether to save translations to file (default: true)
- `logFileName`: Name of the log file (default: "translation_pairs.txt")
- `dialogSheets`: Array of sheet names to process (currently: "Bonebottom")

## Display Format

Instead of overlay display, translations are concatenated directly into game text:

**In-Game Text:**
```
My name Vlad.<br>Меня зовут Влад.
```

**Multi-page Dialogue:**
```
Page 1 English<br>Page 1 Russian<page>Page 2 English<br>Page 2 Russian
```

## File Logging

When enabled, translation pairs are saved to `BepInEx/translation_pairs.txt` with detailed context:

```
2024-01-01 12:00:00 | Bonebottom.GREETING_KEY | EN: Hello there! | RU: Привет!
2024-01-01 12:00:05 | UI.SETTINGS_DESC | EN: Game settings | RU: Настройки игры
```

## Filtered Content

The mod uses smart filtering to process only relevant content:

### Currently Processed Sheets:
- `Bonebottom` - Specific NPC dialogue (primary focus)

### Conditional Processing:
- `UI` sheets - Only keys containing "DESC" (case-insensitive)
- `Quests` sheets - Only keys containing "DESC" (case-insensitive)

### Excluded Sheets:
- `MainMenu` - Menu navigation items
- `Prompts` - System prompts and confirmations
- `Titles` - Title screens and headers

## Troubleshooting

### No translations appearing in-game
- Check BepInEx console for mod loading messages and "[TRANSLATION]" entries
- Ensure the game has Russian localization files installed
- Verify you're interacting with content from processed sheets (currently Bonebottom)
- Check that delayed patching completed successfully (look for "Delayed Harmony patches applied successfully!")

### Missing Russian translations
- Some keys may not have Russian translations available
- The mod will show `<перевод не найден>` for missing translations
- Check the log file for detailed translation attempts

### Performance issues
- The mod includes comprehensive error handling to prevent crashes
- Language switching uses proper restoration in finally blocks
- Recursive call protection prevents infinite loops

### Debugging
- Enable detailed logging by checking BepInEx console output
- Review `BepInEx/translation_pairs.txt` for successful translations
- Look for "[STRING_INTERCEPT]" messages to see what content is being processed

## Technical Details

### Architecture
- **Main Class**: `SilkMod` - BepInEx plugin entry point with delayed patching
- **Patch Class**: `LanguageGetPatch` - Harmony postfix patch with text concatenation
- **Display System**: Direct text modification (no overlay)
- **Language Switching**: Temporary locale changes with restoration and recursion protection

### Key Features
- **Delayed Patching**: 5-second delay + scene loading wait for stability
- **Page-Aware Processing**: Handles `<page>` and `<hpage>` tags in multi-page dialogue
- **Smart Concatenation**: Combines EN/RU text with `<br>` separator per page
- **Recursion Protection**: `isGettingTranslation` flag prevents infinite loops

### API Usage
- `Language.Get(key, sheetTitle)` - Target method for interception
- `Language.CurrentLanguage()` - Get current language code
- `Language.SwitchLanguage(LanguageCode)` - Change active language
- `UnityEngine.SceneManagement.SceneManager` - Scene loading detection

### Error Handling
- Comprehensive try-catch blocks around all language operations
- Automatic language restoration in finally blocks with nested error handling
- Graceful handling of missing translations with fallback messages
- Detailed logging of all operations, errors, and warnings

## Development

### Setup
1. Clone the repository
2. Open in Visual Studio 2022 or VS Code with C# extension
3. Modify `SilkMod.cs` as needed
4. Build with `dotnet build --configuration Release`
5. Manually copy `output/SilkMod.dll` to `BepInEx/plugins/SilkMod/`

### Key Methods
- `DelayedPatchingCoroutine()` - Handles delayed initialization and patching
- `CaptureTranslation()` - Logs translation pairs to console and file
- `ShouldTranslateSheet()` - Determines which content to process based on sheet/key filters
- `GetRussianTranslation()` - Fetches Russian text with recursion protection
- `ConcatenateEnglishAndRussian()` - Combines EN/RU text with page-aware formatting
- `SplitByPageTags()` - Handles multi-page dialogue parsing

### Configuration Options
Modify these constants in `SilkMod.cs`:
- `dialogSheets` - Array of sheet names to process
- `enableFileLogging` - Enable/disable file logging
- `logFileName` - Name of the log file

## Project Structure

```
SilkMod/
├── SilkMod.cs              # Main mod implementation
├── SilkMod.csproj          # Project configuration with dependencies
├── SilkMod.sln             # Visual Studio solution file
├── .github/workflows/      # CI/CD automation
│   └── build.yml          # GitHub Actions build workflow
├── output/                 # Build output directory (gitignored)
└── README.md              # This file
```

## License

This mod is provided as-is for educational and personal use.

## Credits

Developed for Hollow Knight: Silksong modding community.