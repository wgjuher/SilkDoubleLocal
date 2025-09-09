# SilkMod - Silksong Translation Display Mod

A BepInEx mod for Hollow Knight: Silksong that intercepts English strings and displays both English and Russian translations on screen in real-time.

## Features

- **Real-time Translation Display**: Shows both English and Russian text for dialogue and UI elements
- **Harmony Patching**: Intercepts `Language.Get()` calls to capture all localized strings
- **Smart Filtering**: Only processes relevant content (Dialogue and UI sheets)
- **On-Screen Overlay**: Displays translations using Unity's OnGUI system
- **File Logging**: Optionally saves translation pairs to a text file
- **Error Handling**: Robust error handling with automatic language restoration

## How It Works

1. **Interception**: The mod patches `TeamCherry.Localization.Language.Get(string key, string sheetTitle)` using Harmony
2. **Capture**: When the game requests an English string, the mod captures it
3. **Translation Fetch**: The mod temporarily switches to `ru-RU` locale to fetch the Russian translation
4. **Display**: Both strings are displayed on screen for 6 seconds
5. **Logging**: Translation pairs are logged to console and optionally to file

## Installation

1. Install BepInEx for Hollow Knight: Silksong
2. Build this mod or download the compiled DLL
3. Place `SilkMod.dll` in `BepInEx/plugins/` folder
4. Launch the game

## Building

```bash
dotnet build
```

Requirements:
- .NET Standard 2.1
- BepInEx 5.4.21+
- HarmonyX 2.10.2+
- Silksong Game Libraries

## Configuration

The mod includes several configurable options in the source code:

- `displayDuration`: How long translations appear on screen (default: 6 seconds)
- `enableFileLogging`: Whether to save translations to file (default: true)
- `logFileName`: Name of the log file (default: "translation_pairs.txt")

## Display Format

```
EN: My name Vlad.
RU: Меня зовут Влад.
```

## File Logging

When enabled, translation pairs are saved to `BepInEx/translation_pairs.txt` with timestamps:

```
2024-01-01 12:00:00 | EN: Settings | RU: Настройки
2024-01-01 12:00:05 | EN: My name Vlad. | RU: Меня зовут Влад.
```

## Filtered Content

The mod only processes strings from these sheets to reduce noise:
- `Dialogue` - NPC conversations and story text
- `UI` - Menu items, buttons, and interface elements

## Troubleshooting

### Nothing appears on screen
- Check BepInEx console for mod loading messages
- Ensure the game has Russian localization files
- Try interacting with NPCs or opening menus

### Too many translations flooding the screen
- The mod filters to Dialogue and UI sheets only
- Display duration is limited to 6 seconds per translation

### Missing Russian translations
- Some keys may not have Russian translations
- The mod will show `<перевод не найден>` for missing translations

### Performance issues
- The mod includes error handling to prevent crashes
- Language switching is optimized with proper restoration

## Technical Details

### Architecture
- **Main Class**: `SilkMod` - BepInEx plugin entry point
- **Patch Class**: `LanguageGetPatch` - Harmony postfix patch
- **Display System**: Unity OnGUI overlay
- **Language Switching**: Temporary locale changes with restoration

### API Usage
- `Language.Get(key, sheetTitle)` - Target method for interception
- `Language.CurrentLanguage()` - Get current language code
- `Language.SwitchLanguage(LanguageCode)` - Change active language

### Error Handling
- Try-catch blocks around all language operations
- Automatic language restoration in finally blocks
- Graceful handling of missing translations
- Logging of all errors and warnings

## Development

To modify the mod:

1. Clone the repository
2. Open in Visual Studio or VS Code
3. Modify `SilkMod.cs` as needed
4. Build with `dotnet build`
5. Test in game

### Key Methods
- `CaptureTranslation()` - Stores and displays translation pairs
- `GetRussianTranslation()` - Fetches Russian text for a given key
- `OnGUI()` - Renders the on-screen overlay

## License

This mod is provided as-is for educational and personal use.

## Credits

Based on the translation interception guide for Hollow Knight: Silksong modding.