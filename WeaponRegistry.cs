using System;
using System.Collections.Generic;

public static class WeaponRegistry
{
public static Dictionary<string, int> CustomWeapons = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

public static void RegisterWeapon(string weaponName, int damage)
{
if (CustomWeapons.ContainsKey(weaponName))
{
Console.WriteLine($"[WEAPON REGISTRY] Warning: Weapon '{weaponName}' already registered. Overwriting.");
}
CustomWeapons[weaponName] = damage;
Console.WriteLine($"[WEAPON REGISTRY] Registered weapon: {weaponName} (Damage: {damage})");
}

public static int GetWeaponDamage(string weaponName)
{
if (CustomWeapons.TryGetValue(weaponName, out int damage))
{
return damage;
}
return -1;
}
}
