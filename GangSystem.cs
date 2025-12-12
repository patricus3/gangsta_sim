using System;
using static GameState;

static class GangSystem
{
public static string GetGangOptions()
{
string result = "\nYou look for a gang to join!\n";
if (currentGang != "None")
{
result += $"You're already in the {currentGang}! Type 'leave gang' first if you want to switch.\n";
return result;
}
result += "Available gangs:\n";
for (int i = 0; i < gangs.Count; i++)
{
result += $"{i + 1}. {gangs[i].Name} - {gangs[i].MemberCount} members (Requires {gangs[i].RequiredKills} kills)\n";
}
return result;
}

public static string JoinSpecificGang(int choice)
{
string result = "";
if (currentGang != "None")
{
result += $"You're already in the {currentGang}! Type 'leave gang' first if you want to switch.\n";
return result;
}

if (choice < 1 || choice > gangs.Count)
{
result += "Invalid gang choice.\n";
return result;
}

Gang selectedGang = gangs[choice - 1];
if (killCount >= selectedGang.RequiredKills)
{
currentGang = selectedGang.Name;
ModLoader.OnGangJoined(selectedGang.Name);
result += $"\nThe {selectedGang.Name} boss nods: \"Welcome to the crew, killer! You're one of us now.\"\n";
cash += 50;
ModLoader.OnCashGained(50, cash);
result += "You get a $50 welcome bonus from the gang stash!\n";
}
else
{
result += $"You don't have enough kills to join the {selectedGang.Name} yet!\n";
}
return result;
}

public static string JoinGang()
{
// For backward compatibility, we'll simulate joining a random eligible gang
bool joined = false;
string result = "";
for (int i = 0; i < gangs.Count; i++)
{
Gang gang = gangs[i];
if (killCount >= gang.RequiredKills)
{
currentGang = gang.Name;
ModLoader.OnGangJoined(gang.Name);
result += $"\nThe {gang.Name} boss nods: \"Welcome to the crew, killer! You're one of us now.\"\n";
cash += 50;
ModLoader.OnCashGained(50, cash);
result += "You get a $50 welcome bonus from the gang stash!\n";
joined = true;
break;
}
}
if (!joined)
{
result += "You don't have enough kills to join any gang yet!\n";
}
return result;
}

public static string CheckStatus()
{
string result = "\nYour status:\n";
result += $"Health: {playerHealth}/{playerMaxHealth}\n";
result += $"Cash: ${cash}\n";
result += $"Equipped Weapon: {equippedWeapon}\n";
result += $"Equipped Armor: {equippedArmor}\n";
if (equippedArmor == "bone armor") result += $"Bone Armor Durability: {boneArmorDurability}\n";
result += $"Kill Count: {killCount}\n";
result += $"Gang: {currentGang}\n";
result += "Inventory: ";
foreach (var item in inventory) result += item + " ";
result += "\n";
return result;
}
}