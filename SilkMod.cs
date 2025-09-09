using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TeamCherry.Localization;
using UnityEngine;
using System;
using System.IO;

namespace SilkMod;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class SilkMod : BaseUnityPlugin
{
    public static ManualLogSource logger = null!;
    private static Harmony harmony = null!;
    
    // Translation display variables
    private static string lastEnglish = "";
    private static string lastRussian = "";
    private static float lastCaptureTime = 0f;
    private static readonly float displayDuration = 6f;
    
    // File logging settings
    private static readonly bool enableFileLogging = true;
    private static readonly string logFileName = "translation_pairs.txt";
    private static string logFilePath = "";
    
    private void Awake()
    {
        logger = Logger;
        
        // Initialize file logging path
        if (enableFileLogging)
        {
            logFilePath = Path.Combine(Paths.BepInExRootPath, logFileName);
            Logger.LogInfo($"Translation logging enabled. File: {logFilePath}");
        }
        
        // Initialize Harmony
        harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} has loaded!");
        Logger.LogInfo("Harmony patches applied successfully!");
    }
    
    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
    }
    
    private void OnGUI()
    {
        // Only display if we have recent translations and within display duration
        if (Time.time - lastCaptureTime > displayDuration) return;
        if (string.IsNullOrEmpty(lastEnglish) || string.IsNullOrEmpty(lastRussian)) return;
        
        // Create GUI style
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            normal = { textColor = Color.white }
        };
        
        // Display both translations
        GUI.Label(new Rect(20, 20, 1400, 40), "EN: " + lastEnglish, style);
        GUI.Label(new Rect(20, 50, 1400, 40), "RU: " + lastRussian, style);
    }
    
    public static void CaptureTranslation(string english, string russian)
    {
        lastEnglish = english;
        lastRussian = russian;
        lastCaptureTime = Time.time;
        
        // Log the translation pair
        logger.LogInfo($"[TRANSLATION] EN: {english}");
        logger.LogInfo($"[TRANSLATION] RU: {russian}");
        
        // Save to file if enabled
        if (enableFileLogging && !string.IsNullOrEmpty(logFilePath))
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | EN: {english} | RU: {russian}\n";
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Failed to write to log file: {ex.Message}");
            }
        }
    }
}

[HarmonyPatch(typeof(Language), "Get", new[] { typeof(string), typeof(string) })]
public static class LanguageGetPatch
{
    private static ManualLogSource Logger => SilkMod.logger;
    
    static void Postfix(string key, string sheetTitle, ref string __result)
    {
        try
        {
            // Filter to only relevant sheets to reduce noise
            if (sheetTitle != "Dialogue" && sheetTitle != "UI") return;
            
            // Skip empty or null results
            if (string.IsNullOrEmpty(__result)) return;
            
            var englishText = __result;
            
            // Fetch Russian translation
            string russianText = GetRussianTranslation(key, sheetTitle);
            
            // Only capture if we got a valid Russian translation
            if (!string.IsNullOrEmpty(russianText) && russianText != englishText)
            {
                SilkMod.CaptureTranslation(englishText, russianText);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError($"Error in Language.Get patch: {ex.Message}");
        }
    }
    
    private static string GetRussianTranslation(string key, string sheetTitle)
    {
        LanguageCode originalLanguage = LanguageCode.EN;
        try
        {
            // Save current language
            originalLanguage = Language.CurrentLanguage();
            
            // Switch to Russian
            Language.SwitchLanguage(LanguageCode.RU);
            
            // Get Russian translation
            string russianText = Language.Get(key, sheetTitle);
            
            return russianText ?? "<перевод не найден>";
        }
        catch (Exception ex)
        {
            Logger?.LogWarning($"Failed to get Russian translation for {sheetTitle}.{key}: {ex.Message}");
            return "<перевод не найден>";
        }
        finally
        {
            // Always restore original language
            try
            {
                Language.SwitchLanguage(originalLanguage);
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Failed to restore language to {originalLanguage}: {ex.Message}");
            }
        }
    }
}