using System;
using System.Collections.Generic;
using System.IO;
using static GameState;

static class SaveLoad
{
public static void SaveGame()
{
string[] contents =
{
playerHealth.ToString(),
cash.ToString(),
equippedWeapon,
killCount.ToString(),
string.Join(",", inventory),
equippedArmor,
currentGang,
boneArmorDurability.ToString()
};
File.WriteAllLines("gangster_save.txt", contents);
ModLoader.OnGameSaved();
Console.WriteLine("Game saved to 'gangster_save.txt'.");
}

public static void LoadGame()
{
if (File.Exists("gangster_save.txt"))
{
string[] data = File.ReadAllLines("gangster_save.txt");
if (data.Length >= 8)
{
playerHealth = int.Parse(data[0]);
cash = int.Parse(data[1]);
equippedWeapon = data[2];
killCount = int.Parse(data[3]);
inventory = new List<string>((data[4] ?? "Fists").Split(',', StringSplitOptions.RemoveEmptyEntries));
if (inventory.Count == 0) inventory.Add("Fists");
equippedArmor = data[5];
currentGang = data[6];
boneArmorDurability = int.Parse(data[7]);
Console.WriteLine("Game loaded from 'gangster_save.txt'. [LOAD COMPLETE]");
playerMaxHealth = 100 + killCount;
ModLoader.OnGameLoaded();
}
else if (data.Length == 7)
{
Console.WriteLine("legacy convert system, v 0.2 activated, converting save data!");
playerHealth = int.Parse(data[0]);
cash = int.Parse(data[1]);
equippedWeapon = data[2];
killCount = int.Parse(data[3]);
inventory = new List<string>((data[4] ?? "Fists").Split(',', StringSplitOptions.RemoveEmptyEntries));
if (inventory.Count == 0) inventory.Add("Fists");
equippedArmor = data[5];
currentGang = data[6];
boneArmorDurability = 0;
playerMaxHealth = 100 + killCount;
}
else if (data.Length == 6)
{
Console.WriteLine("legacy convert system, v 0.1 activatedd, please wait as we convert & load your save data!");
cash = int.Parse(data[1]);
equippedWeapon = data[2];
killCount = int.Parse(data[3]);
inventory = new List<string>((data[4] ?? "Fists").Split(',', StringSplitOptions.RemoveEmptyEntries));
if (inventory.Count == 0) inventory.Add("Fists");
equippedArmor = data[5];
playerMaxHealth = 100 + killCount;
currentGang = "None";
boneArmorDurability = 0;
}
else
{
Console.WriteLine("Save file corrupted! Starting new game.");
}
}
}
}
