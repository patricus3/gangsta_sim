using System;
using System.Collections.Generic;

static class Weapons
{
public static string GetAvailableWeapons()
{
string result = "\nYour weapons:\n";
List<string> availableWeapons = new List<string>();
foreach (var item in GameState.inventory) 
{
    int damage = GetWeaponDamage(item);
    if (damage > 0) 
    {
        availableWeapons.Add(item);
        result += item + "\n";
    }
}

if (availableWeapons.Count == 0)
{
result += "You don't have any weapons to equip!\n";
}
return result;
}

public static string EquipWeapon(string selectedWeapon)
{
GameState.equippedWeapon = selectedWeapon;
ModLoader.OnWeaponEquipped(selectedWeapon);
return $"You equip the {GameState.equippedWeapon}.\n";
}

public static string EquipWeapon()
{
string result = "\nYour weapons:\n";
foreach (var item in GameState.inventory) if (GetWeaponDamage(item) > 0) result += item + "\n";

// For GUI version, we'll simulate equipping a random weapon
Random rand = new Random();
List<string> availableWeapons = new List<string>();
foreach (var item in GameState.inventory) if (GetWeaponDamage(item) > 0) availableWeapons.Add(item);

if (availableWeapons.Count > 0)
{
string pick = availableWeapons[rand.Next(availableWeapons.Count)];
GameState.equippedWeapon = pick;
ModLoader.OnWeaponEquipped(pick);
result += $"You equip the {GameState.equippedWeapon}.\n";
}
else
{
result += "You don't have any weapons to equip!\n";
}
return result;
}

public static int GetWeaponDamage(string weapon)
{
int customDamage = WeaponRegistry.GetWeaponDamage(weapon);
if (customDamage >= 0)
{
return customDamage;
}
switch ((weapon ?? "").ToLower())
{
case "fists": return 0;
case "knife": return 5;
case "bat": return 10;
case "pistol": return 20;
case "machinegun": return 28;
default: return 0;
}
}
}