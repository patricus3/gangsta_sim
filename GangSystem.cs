using System;
using static GameState;

static class GangSystem
{
public static void JoinGang()
{
Console.WriteLine("\nYou look for a gang to join!");
if (currentGang != "None")
{
Console.WriteLine($"You're already in the {currentGang}! Type 'leave gang' first if you want to switch.");
return;
}
Console.WriteLine("Available gangs:");
for (int i = 0; i < gangs.Count; i++)
{
Console.WriteLine($"{i + 1}. {gangs[i].Name} - {gangs[i].MemberCount} members (Requires {gangs[i].RequiredKills} kills)");
}
Console.Write("Which gang do you want to join? (1-3, or 'exit'): ");
string? input = Console.ReadLine();
if (input == "exit") return;
if (int.TryParse(input, out int choice) && choice >= 1 && choice <= 3)
{
int idx = choice - 1;
Gang gang = gangs[idx];
if (killCount < gang.RequiredKills)
{
Console.WriteLine($"The {gang.Name} boss says: \"You need {gang.RequiredKills} kills to join us, punk! You only got {killCount}!\"");
}
else
{
currentGang = gang.Name;
ModLoader.OnGangJoined(gang.Name);
Console.WriteLine($"\nThe {gang.Name} boss nods: \"Welcome to the crew, killer! You're one of us now.\"");
cash += 50;
ModLoader.OnCashGained(50, cash);
Console.WriteLine("You get a $50 welcome bonus from the gang stash!");
}
}
else
{
Console.WriteLine("Invalid choice!");
}
}

public static void CheckStatus()
{
Console.WriteLine("\nYour status:");
Console.WriteLine($"Health: {playerHealth}/{playerMaxHealth}");
Console.WriteLine($"Cash: ${cash}");
Console.WriteLine($"Equipped Weapon: {equippedWeapon}");
Console.WriteLine($"Equipped Armor: {equippedArmor}");
if (equippedArmor == "bone armor") Console.WriteLine($"Bone Armor Durability: {boneArmorDurability}");
Console.WriteLine($"Kill Count: {killCount}");
Console.WriteLine($"Gang: {currentGang}");
Console.Write("Inventory: ");
foreach (var item in inventory) Console.Write(item + " ");
Console.WriteLine();
}
}
