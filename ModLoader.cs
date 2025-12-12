using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

static class ModLoader
{
    public static List<IMod> LoadedMods { get; private set; } = new List<IMod>();
    public static List<string> ModLoadErrors { get; private set; } = new List<string>();
    public static Action<string> OnMessageLogged { get; set; }
    private static StringWriter consoleWriter;
    private static TextWriter originalConsoleOut;
    private static List<string> capturedMessages = new List<string>();

    public static void LoadMods()
    {
        // Clear previous errors and loaded mods
        ModLoadErrors.Clear();
        LoadedMods.Clear();
        
        // Redirect console output to capture mod messages
        originalConsoleOut = Console.Out;
        consoleWriter = new StringWriter();
        Console.SetOut(consoleWriter);
        
        string modPath = "Mods";
        if (!Directory.Exists(modPath))
        {
            Directory.CreateDirectory(modPath);
            LogMessage("Created Mods folder.");
            RestoreConsoleOutput();
            return;
        }

        string[] modFiles = Directory.GetFiles(modPath, "*.dll");
        if (modFiles.Length == 0)
        {
            LogMessage("No mod files found in Mods folder.");
            RestoreConsoleOutput();
            return;
        }
        
        LogMessage($"Found {modFiles.Length} mod file(s) in Mods folder.");
        
        foreach (string modFile in modFiles)
        {
            try
            {
                Assembly modAssembly = Assembly.LoadFrom(modFile);
                Type[] types = modAssembly.GetTypes();
                bool modFound = false;
                
                foreach (Type type in types)
                {
                    if (typeof(IMod).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        IMod? mod = Activator.CreateInstance(type) as IMod;
                        if (mod != null)
                        {
                            LoadedMods.Add(mod);
                            mod.RegisterCustomContent();
                            mod.OnModLoaded();
                            LogMessage($"[MOD LOADED] {mod.ModName} v{mod.ModVersion}");
                            modFound = true;
                        }
                    }
                }
                
                if (!modFound)
                {
                    string errorMessage = $"[MOD SKIPPED] {Path.GetFileName(modFile)} - No valid IMod implementation found";
                    LogMessage(errorMessage);
                    ModLoadErrors.Add(errorMessage);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"[MOD LOAD FAILED] {Path.GetFileName(modFile)}: {ex.Message}";
                LogMessage(errorMessage);
                ModLoadErrors.Add(errorMessage);
            }
        }

        if (LoadedMods.Count > 0)
        {
            LogMessage($"Total mods loaded: {LoadedMods.Count}");
        }
        else
        {
            LogMessage("No mods were successfully loaded.");
        }
        
        RestoreConsoleOutput();
    }

    private static void RestoreConsoleOutput()
    {
        // Flush any pending console output
        if (consoleWriter != null)
        {
            string consoleOutput = consoleWriter.ToString();
            if (!string.IsNullOrEmpty(consoleOutput))
            {
                // Split by newlines and log each line
                string[] lines = consoleOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        LogMessage(line);
                    }
                }
            }
            
            // Restore original console output
            if (originalConsoleOut != null)
            {
                Console.SetOut(originalConsoleOut);
            }
        }
    }

    private static void LogMessage(string message)
    {
        // Send message to UI if callback is set
        OnMessageLogged?.Invoke(message);
        
        // Also log to console for debugging
        Console.WriteLine(message);
    }

    public static string OnAttackStart(string victim)
    {
        capturedMessages.Clear();
        
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnAttackStart(victim); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    capturedMessages.Add(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
        
        // Return captured messages as a single string
        return string.Join("\n", capturedMessages);
    }

    public static string OnAttackEnd(string victim, bool killed)
    {
        capturedMessages.Clear();
        
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnAttackEnd(victim, killed); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    capturedMessages.Add(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
        
        // Return captured messages as a single string
        return string.Join("\n", capturedMessages);
    }

    public static void OnKill(int newKillCount)
    {
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnKill(newKillCount); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    LogMessage(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
    }

    public static string OnDamageTaken(int damage, int newHealth)
    {
        capturedMessages.Clear();
        
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnDamageTaken(damage, newHealth); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    capturedMessages.Add(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
        
        // Return captured messages as a single string
        return string.Join("\n", capturedMessages);
    }

    public static void OnCashGained(int amount, int newCash)
    {
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnCashGained(amount, newCash); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    LogMessage(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
    }

    public static void OnWeaponEquipped(string weapon)
    {
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnWeaponEquipped(weapon); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    LogMessage(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
    }

    public static void OnArmorEquipped(string armor)
    {
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnArmorEquipped(armor); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    LogMessage(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
    }

    public static void OnGangJoined(string gangName)
    {
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnGangJoined(gangName); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    LogMessage(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
    }

    public static void OnBankHeistStart()
    {
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnBankHeistStart(); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    LogMessage(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
    }

    public static void OnGameSaved()
    {
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnGameSaved(); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    LogMessage(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
    }

    public static void OnGameLoaded()
    {
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try { mod.OnGameLoaded(); }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    LogMessage(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
        
        // Announce mods loaded
        if (LoadedMods.Count > 0)
        {
            LogMessage($"Game loaded with {LoadedMods.Count} mod(s) active.");
        }
        else
        {
            LogMessage("Game loaded with no mods.");
        }
    }
    
    public static string OnCivilianDeath(string civilianType)
    {
        capturedMessages.Clear();
        
        // Capture console output during mod execution
        var writer = new StringWriter();
        Console.SetOut(writer);
        
        foreach (var mod in LoadedMods)
        {
            try 
            { 
                // Check if the mod implements OnCivilianDeath before calling it
                var method = mod.GetType().GetMethod("OnCivilianDeath");
                if (method != null && method.DeclaringType != typeof(IMod))
                {
                    mod.OnCivilianDeath(civilianType);
                }
            }
            catch (Exception ex) { LogMessage($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
        }
        
        // Flush captured output
        string output = writer.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    capturedMessages.Add(line);
                }
            }
        }
        
        // Restore console output
        Console.SetOut(originalConsoleOut ?? new StreamWriter(Console.OpenStandardOutput()));
        
        // Return captured messages as a single string
        return string.Join("\n", capturedMessages);
    }
}