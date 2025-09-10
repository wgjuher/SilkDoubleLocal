using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TeamCherry.Localization;
using UnityEngine;
using System;
using System.IO;
using System.Collections;

namespace SilkMod;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class SilkMod : BaseUnityPlugin
{
    public static ManualLogSource logger = null!;
    private static Harmony harmony = null!;
    public static bool patchesApplied = false;
    
    // File logging settings
    private static readonly bool enableFileLogging = true;
    private static readonly string logFileName = "translation_pairs.txt";
    private static string logFilePath = "";
    
    // Translation settings - Focus on Bonebottom dialog only
    private static readonly string[] dialogSheets = {
        "Bonebottom" // Only translate Bonebottom dialog
    };
    
    private void Awake()
    {
        logger = Logger;
        
        // Initialize file logging path
        if (enableFileLogging)
        {
            logFilePath = Path.Combine(Paths.BepInExRootPath, logFileName);
            Logger.LogInfo($"Translation logging enabled. File: {logFilePath}");
        }
        
        // Initialize Harmony but don't patch yet
        harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} has loaded!");
        Logger.LogInfo("Starting delayed patching...");
        
        // Start delayed patching coroutine
        StartCoroutine(DelayedPatchingCoroutine());
    }
    
    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
    }
    
    private IEnumerator DelayedPatchingCoroutine()
    {
        Logger.LogInfo("Waiting for game to initialize...");
        
        // Wait for a few seconds to let the game fully load
        yield return new WaitForSeconds(5f);
        
        // Additional check - wait for a scene to be loaded
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "")
        {
            yield return new WaitForSeconds(1f);
        }
        
        Logger.LogInfo($"Game scene loaded: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // Now apply the patches
        try
        {
            harmony.PatchAll();
            patchesApplied = true;
            Logger.LogInfo("Delayed Harmony patches applied successfully!");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to apply delayed patches: {ex.Message}");
        }
    }
    
    
    public static void CaptureTranslation(string english, string secondLanguage, string sheetTitle, string key)
    {
        // Validate inputs
        if (string.IsNullOrEmpty(english) || string.IsNullOrEmpty(secondLanguage))
            return;

        // Skip if translations are identical (no actual translation occurred)
        if (english.Equals(secondLanguage, StringComparison.OrdinalIgnoreCase))
        {
            logger?.LogDebug($"[TRANSLATION_SKIP] Identical text: {english}");
            return;
        }

        // Log the translation pair with context (for file logging only)
        logger?.LogInfo($"[TRANSLATION] Sheet: {sheetTitle}, Key: {key}");
        logger?.LogInfo($"[TRANSLATION] EN: {english}");
        logger?.LogInfo($"[TRANSLATION] SECOND: {secondLanguage}");

        // Save to file if enabled (keep original format for logging)
        if (enableFileLogging && !string.IsNullOrEmpty(logFilePath))
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {sheetTitle}.{key} | EN: {english} | SECOND: {secondLanguage}\n";
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                logger?.LogWarning($"Failed to write to log file: {ex.Message}");
            }
        }
    }


    // Helper method to check if a sheet should be translated
    public static bool ShouldTranslateSheet(string sheetTitle, string key = null)
    {
        logger?.LogInfo($"[TRANSLATION] Sheet: {sheetTitle}, Key: {key}");
        if (string.IsNullOrEmpty(sheetTitle))
            return false;
    
        // Exclude specific sheets entirely
        string[] excludedSheets = { "MainMenu", "Prompts", "Titles", "Map Zones" };
        
        foreach (string excludedSheet in excludedSheets)
        {
            if (sheetTitle.Equals(excludedSheet, StringComparison.OrdinalIgnoreCase))
                return false;
        }
        
        string[] descSheets = { "UI", "Quests", "Tools" };
        // Special handling for descSheets
        foreach (string sheet in descSheets)
        {
            if (sheetTitle.Equals(sheet, StringComparison.OrdinalIgnoreCase))
             {
                // For Quests sheet, only translate if key contains "DESC" (case-insensitive)
                if (string.IsNullOrEmpty(key))
                    return false;
                    
                return key.IndexOf("DESC", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }
        // For all other sheets (not excluded and not Quests), translate everything
        return true;
    }
}

[HarmonyPatch(typeof(Language), "Get", new[] { typeof(string), typeof(string) })]
public static class LanguageGetPatch
{
    private static ManualLogSource Logger => SilkMod.logger;
    
    private static bool isGettingTranslation = false;
    
    static void Postfix(string key, string sheetTitle, ref string __result)
    {
        try
        {
            // Only process if patches were applied after game startup and we're not in a recursive call
            if (!SilkMod.patchesApplied || isGettingTranslation)
                return;

            // Skip if result is empty or null
            if (string.IsNullOrEmpty(__result))
                return;

            // Check if we should translate this sheet
            if (!SilkMod.ShouldTranslateSheet(sheetTitle, key))
            {
                // Still log excluded strings but less verbosely
                Logger?.LogDebug($"[STRING_INTERCEPT] Excluded Sheet: {sheetTitle}, Key: {key}");
                return;
            }

            // Log string interception for all processed sheets
            Logger?.LogInfo($"[STRING_INTERCEPT] Sheet: {sheetTitle}, Key: {key}, Result: {__result}");

            // Get second language translation
            string secondLanguageTranslation = GetSecondLanguageTranslation(key, sheetTitle);
            
            // If we got a valid second language translation, concatenate EN+SECOND and modify the result
            if (!string.IsNullOrEmpty(secondLanguageTranslation) &&
                secondLanguageTranslation != "<перевод не найден>" &&
                secondLanguageTranslation != "<уже на втором языке>")
            {
                // Create concatenated version with EN and second language text
                string concatenatedResult = ConcatenateEnglishAndSecondLanguage(__result, secondLanguageTranslation);
                
                // Log the original translations for file logging
                SilkMod.CaptureTranslation(__result, secondLanguageTranslation, sheetTitle, key);
                
                
                // Modify the actual result to return concatenated text
                __result = concatenatedResult;
                
                Logger?.LogInfo($"[CONCATENATED] Modified result with EN+SECOND concatenation");
            }
            else
            {
                // Log when translation fails or is skipped
                Logger?.LogDebug($"[TRANSLATION_SKIP] Sheet: {sheetTitle}, Key: {key}, Reason: {secondLanguageTranslation}");
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError($"Error in Language.Get patch: {ex.Message}");
        }
    }

    private static string ConcatenateEnglishAndSecondLanguage(string englishText, string secondLanguageText)
    {
        if (string.IsNullOrEmpty(englishText) || string.IsNullOrEmpty(secondLanguageText))
            return englishText;

        // Split both texts by page tags
        string[] englishPages = SplitByPageTags(englishText);
        string[] secondLanguagePages = SplitByPageTags(secondLanguageText);
        
        // If no pages found, treat as single page
        if (englishPages.Length == 0) englishPages = new string[] { englishText };
        if (secondLanguagePages.Length == 0) secondLanguagePages = new string[] { secondLanguageText };
        
        // Build concatenated result
        var result = new System.Text.StringBuilder();
        int maxPages = Math.Max(englishPages.Length, secondLanguagePages.Length);
        
        for (int i = 0; i < maxPages; i++)
        {
            // Add page separator if not first page
            if (i > 0)
            {
                result.Append("<page>");
            }
            
            // Get English and second language text for this page
            string enPage = i < englishPages.Length ? englishPages[i].Trim() : "";
            string secondPage = i < secondLanguagePages.Length ? secondLanguagePages[i].Trim() : "";
            
            // Concatenate with <br> separator
            if (!string.IsNullOrEmpty(enPage) && !string.IsNullOrEmpty(secondPage))
            {
                result.Append(enPage).Append("<br>").Append(secondPage);
            }
            else if (!string.IsNullOrEmpty(enPage))
            {
                result.Append(enPage);
            }
            else if (!string.IsNullOrEmpty(secondPage))
            {
                result.Append(secondPage);
            }
        }
        
        return result.ToString();
    }
    
    // Split text by <page> and <hpage> tags (same logic as in SilkMod class)
    private static string[] SplitByPageTags(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new string[0];

        // Split by both <page> and <hpage> tags
        string[] parts = text.Split(new string[] { "<page>", "<hpage>" }, StringSplitOptions.RemoveEmptyEntries);
        
        // Clean up each part (trim whitespace)
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = parts[i].Trim();
        }
        
        return parts;
    }

    private static string GetSecondLanguageTranslation(string key, string sheetTitle)
    {
        LanguageCode originalLanguage = LanguageCode.EN;
        
        // Define the second language code at compile time based on build properties
#if SECOND_LANGUAGE_RU
        LanguageCode secondLanguageCode = LanguageCode.RU;
        string alreadyInSecondLanguageMessage = "<уже на русском>";
#elif SECOND_LANGUAGE_DE
        LanguageCode secondLanguageCode = LanguageCode.DE;
        string alreadyInSecondLanguageMessage = "<already in German>";
#elif SECOND_LANGUAGE_FR
        LanguageCode secondLanguageCode = LanguageCode.FR;
        string alreadyInSecondLanguageMessage = "<already in French>";
#elif SECOND_LANGUAGE_ES
        LanguageCode secondLanguageCode = LanguageCode.ES;
        string alreadyInSecondLanguageMessage = "<already in Spanish>";
#elif SECOND_LANGUAGE_IT
        LanguageCode secondLanguageCode = LanguageCode.IT;
        string alreadyInSecondLanguageMessage = "<already in Italian>";
#elif SECOND_LANGUAGE_PT
        LanguageCode secondLanguageCode = LanguageCode.PT;
        string alreadyInSecondLanguageMessage = "<already in Portuguese>";
#elif SECOND_LANGUAGE_JA
        LanguageCode secondLanguageCode = LanguageCode.JA;
        string alreadyInSecondLanguageMessage = "<already in Japanese>";
#elif SECOND_LANGUAGE_KO
        LanguageCode secondLanguageCode = LanguageCode.KO;
        string alreadyInSecondLanguageMessage = "<already in Korean>";
#elif SECOND_LANGUAGE_ZH
        LanguageCode secondLanguageCode = LanguageCode.ZH;
        string alreadyInSecondLanguageMessage = "<already in Chinese>";
#else
        // Default to Russian if no specific language is defined
        LanguageCode secondLanguageCode = LanguageCode.RU;
        string alreadyInSecondLanguageMessage = "<уже на русском>";
#endif

        try
        {
            // Set flag to prevent recursion
            isGettingTranslation = true;

            // Save current language
            originalLanguage = Language.CurrentLanguage();

            // Skip if already in second language to avoid unnecessary work
            if (originalLanguage == secondLanguageCode)
            {
                return alreadyInSecondLanguageMessage;
            }

            // Switch to second language
            Language.SwitchLanguage(secondLanguageCode);

            // Get second language translation
            string secondLanguageText = Language.Get(key, sheetTitle);

            return secondLanguageText ?? "<перевод не найден>";
        }
        catch (Exception ex)
        {
            Logger?.LogWarning($"Failed to get second language translation for {sheetTitle}.{key}: {ex.Message}");
            return "<перевод не найден>";
        }
        finally
        {
            // Always restore original language and clear flag
            try
            {
                if (originalLanguage != secondLanguageCode)
                {
                    Language.SwitchLanguage(originalLanguage);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Failed to restore language to {originalLanguage}: {ex.Message}");
            }
            finally
            {
                isGettingTranslation = false;
            }
        }
    }

}








