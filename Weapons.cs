using System;
using System.Collections.Generic;

static class Weapons
{
public static void EquipWeapon()
{
Console.WriteLine("\nYour weapons:");
foreach (var item in GameState.inventory) if (GetWeaponDamage(item) > 0) Console.WriteLine(item);
Console.Write("Which weapon do you want to equip? ");
string? pick = Console.ReadLine();
if (pick != null && GameState.inventory.Contains(pick) && GetWeaponDamage(pick) > 0)
{
GameState.equippedWeapon = pick;
ModLoader.OnWeaponEquipped(pick);
Console.WriteLine($"You equip the {GameState.equippedWeapon}.");
}
else
{
Console.WriteLine("You don't have that weapon!");
}
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