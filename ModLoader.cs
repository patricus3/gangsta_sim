using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
static class ModLoader
{
public static List<IMod> LoadedMods { get; private set; } = new List<IMod>();

public static void LoadMods()
{
string modPath = "Mods";
if (!Directory.Exists(modPath))
{
Directory.CreateDirectory(modPath);
Console.WriteLine("Created Mods folder.");
return;
}

string[] modFiles = Directory.GetFiles(modPath, "*.dll");
if (modFiles.Length == 0)
{
return;
}
foreach (string modFile in modFiles)
{
try
{
Assembly modAssembly = Assembly.LoadFrom(modFile);
Type[] types = modAssembly.GetTypes();
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
Console.WriteLine($"[MOD LOADED] {mod.ModName} v{mod.ModVersion}");
}
}
}
}
catch (Exception ex)
{
Console.WriteLine($"Failed to load mod {Path.GetFileName(modFile)}: {ex.Message}");
}
}

if (LoadedMods.Count > 0)
{
Console.WriteLine($"Total mods loaded: {LoadedMods.Count}");
}
}

public static void OnAttackStart(string victim)
{
foreach (var mod in LoadedMods)
{
try { mod.OnAttackStart(victim); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnAttackEnd(string victim, bool killed)
{
foreach (var mod in LoadedMods)
{
try { mod.OnAttackEnd(victim, killed); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnKill(int newKillCount)
{
foreach (var mod in LoadedMods)
{
try { mod.OnKill(newKillCount); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnDamageTaken(int damage, int newHealth)
{
foreach (var mod in LoadedMods)
{
try { mod.OnDamageTaken(damage, newHealth); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnCashGained(int amount, int newCash)
{
foreach (var mod in LoadedMods)
{
try { mod.OnCashGained(amount, newCash); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnWeaponEquipped(string weapon)
{
foreach (var mod in LoadedMods)
{
try { mod.OnWeaponEquipped(weapon); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnArmorEquipped(string armor)
{
foreach (var mod in LoadedMods)
{
try { mod.OnArmorEquipped(armor); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnGangJoined(string gangName)
{
foreach (var mod in LoadedMods)
{
try { mod.OnGangJoined(gangName); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnBankHeistStart()
{
foreach (var mod in LoadedMods)
{
try { mod.OnBankHeistStart(); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnGameSaved()
{
foreach (var mod in LoadedMods)
{
try { mod.OnGameSaved(); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}

public static void OnGameLoaded()
{
foreach (var mod in LoadedMods)
{
try { mod.OnGameLoaded(); }
catch (Exception ex) { Console.WriteLine($"[MOD ERROR] {mod.ModName}: {ex.Message}"); }
}
}
}