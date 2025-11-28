using System;
using System.Collections.Generic;

public static class ArmorRegistry
{
public static Dictionary<string, int> CustomArmors = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

public static void RegisterArmor(string armorName, int damageReduction)
{
if (CustomArmors.ContainsKey(armorName))
{
Console.WriteLine($"[ARMOR REGISTRY] Warning: Armor '{armorName}' already registered. Overwriting.");
}
CustomArmors[armorName] = damageReduction;
Console.WriteLine($"[ARMOR REGISTRY] Registered armor: {armorName} (Reduction: {damageReduction})");
}

public static int GetArmorReduction(string armorName)
{
if (CustomArmors.TryGetValue(armorName, out int reduction))
{
return reduction;
}
return -1;
}
}
