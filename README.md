# SilkMod - Silksong Translation Display Mod

A BepInEx mod for Hollow Knight: Silksong that intercepts English strings and displays both English and a configurable second language translations in-game by concatenating them directly into the game text.

## Features

- **In-Game Text Concatenation**: Modifies game text to show both English and second language translations inline
- **Configurable Second Language**: Build-time configuration for multiple language support (RU, DE, FR, ES, IT, PT, JA, KO, ZH)
- **Configurable Logging**: Build-time flag to enable/disable all console logging (disabled by default for performance)
- **Harmony Patching**: Intercepts `Language.Get()` calls to capture and modify localized strings
- **Smart Filtering**: Processes specific content types with configurable sheet filtering
- **Delayed Patching**: Waits for game initialization before applying patches for stability
- **Console Logging**: Detailed logging to BepInEx console (when logging enabled)
- **Page-Aware Processing**: Handles multi-page dialogue with proper formatting
- **Error Handling**: Robust error handling with automatic language restoration

## How It Works

1. **Delayed Initialization**: Waits 5 seconds and for scene loading before applying patches
2. **Interception**: Patches `TeamCherry.Localization.Language.Get(string key, string sheetTitle)` using Harmony
3. **Translation Fetch**: Temporarily switches to configured second language locale to fetch translations
4. **Text Concatenation**: Combines English and second language text with `<br>` separator
5. **In-Game Display**: Modified text appears directly in game dialogue and UI
6. **Logging**: Translation pairs are logged to BepInEx console (when enabled)

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
2. Build with default language (Russian):
```bash
dotnet restore
dotnet build --configuration Release
```

3. Build with specific second language:
```bash
dotnet build --configuration Release -p:SecondLanguage=DE
```

4. Build with logging enabled:
```bash
dotnet build --configuration Release -p:EnableLogging=true
```

5. Build with both custom language and logging:
```bash
dotnet build --configuration Release -p:SecondLanguage=DE -p:EnableLogging=true
```

**Supported Languages:**
- `RU` - Russian (default)
- `DE` - German
- `FR` - French
- `ES` - Spanish
- `IT` - Italian
- `PT` - Portuguese
- `JA` - Japanese
- `KO` - Korean
- `ZH` - Chinese

**Logging Options:**
- `EnableLogging=false` - No logging (default, best performance)
- `EnableLogging=true` - Console logging to BepInEx log


### Dependencies
- .NET Standard 2.1
- BepInEx 5.4.21
- HarmonyX 2.10.2
- UnityEngine.Modules 6000.0.50
- Silksong.GameLibs 1.0.1-silksong1.0.28324

### CI/CD
The project includes GitHub Actions workflow with the following features:
- **Automatic Builds**: Builds multiple language variants on push/PR (RU, DE, FR by default)
- **Manual Dispatch**: Trigger builds with custom language selection via GitHub Actions UI
- **Artifact Upload**: Separate artifacts for each language variant (`SilkMod-{LANG}-dll`)

## Configuration

### Build-Time Configuration
Configure the second language and logging when building:

```bash
# Second language only (no logging)
dotnet build -p:SecondLanguage=DE

# Enable logging with default language (Russian)
dotnet build -p:EnableLogging=true

# Both custom language and logging
dotnet build -p:SecondLanguage=DE -p:EnableLogging=true

# Using environment variables
export SecondLanguage=FR
export EnableLogging=true
dotnet build
```

### Runtime Configuration
The mod includes several configurable options in the source code:

- `dialogSheets`: Array of sheet names to process (currently: "Bonebottom")

**Note**: Console logging is disabled by default for optimal performance. Enable it only when needed for debugging or development.

## Display Format

Instead of overlay display, translations are concatenated directly into game text:

**In-Game Text (Russian example):**
```
My name Vlad.<br>Меня зовут Влад.
```

**In-Game Text (German example):**
```
My name Vlad.<br>Mein Name ist Vlad.
```

**Multi-page Dialogue:**
```
Page 1 English<br>Page 1 Second Language<page>Page 2 English<br>Page 2 Second Language
```

## Console Logging

When logging is enabled via `EnableLogging=true`, translation pairs are logged to the BepInEx console:

```
[Info   :   SilkMod] [TRANSLATION] Sheet: Bonebottom, Key: GREETING_KEY
[Info   :   SilkMod] [TRANSLATION] EN: Hello there!
[Info   :   SilkMod] [TRANSLATION] SECOND: Привет!
```

The "SECOND" field contains the translation in whatever language was configured at build time.

**Performance Note**: When logging is disabled (default), no console logging occurs, providing optimal game performance.

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
- If logging is enabled: Check BepInEx console for mod loading messages and "[TRANSLATION]" entries
- Ensure the game has the configured second language localization files installed
- Verify you're interacting with content from processed sheets (currently Bonebottom)
- If logging is enabled: Check that delayed patching completed successfully (look for "Delayed Harmony patches applied successfully!")

### Missing second language translations
- Some keys may not have translations available in the configured language
- The mod will show `<перевод не найден>` for missing translations
- If logging is enabled: Check the BepInEx console for detailed translation attempts

### Performance Issues
- If experiencing performance problems, ensure logging is disabled (default)
- Logging adds overhead for console output operations
- Only enable logging for debugging or development purposes

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

### Build Configuration
Set the second language at build time:
```bash
dotnet build -p:SecondLanguage=DE -p:SecondLanguageCode=LanguageCode.DE -p:SecondLanguageName=German
```

### Runtime Configuration Options
Modify these constants in `SilkMod.cs`:
- `dialogSheets` - Array of sheet names to process
- `enableFileLogging` - Controlled by `EnableLogging` build parameter (compile-time)
- `logFileName` - Name of the log file (only used when logging enabled)

**Build Parameters:**
- `SecondLanguage` - Target language code (RU, DE, FR, ES, IT, PT, JA, KO, ZH)
- `EnableLogging` - Enable all logging (true/false, default: false)

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