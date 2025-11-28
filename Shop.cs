using System;
using static GameState;

static class Shop
{
public static void VisitArmorShop()
{
string[] armors = { "leather jacket", "bulletproof vest", "riot helmet", "military armor", "bone armor" };
int[] prices = { 30, 60, 90, 200, 500 };
int[] reductions = { 2, 5, 8, 18, 0 };
reductions[4] = killCount;
Console.WriteLine("\nYou duck into a shady armor shop.");
Console.WriteLine("Steve the shopkeeper says: \"Heya Frank! Maybe you need some good pieces to defend yourself against some punks, please don't call the dogs on me\"");
Console.WriteLine("Steve says: \"Here's what I have on sale, if you are a cheapskate or not, ya find something for yarself\"");
for (int i = 0; i < armors.Length; i++)
{
if (armors[i] != "bone armor") Console.WriteLine($"{i + 1}. {armors[i]} - ${prices[i]} (Reduces damage by {reductions[i]})");
else Console.WriteLine($"{i + 1}. Bone armor - ${prices[i]} (Reduces damage by {reductions[i]}, increases with kills)");
}
Console.WriteLine($"Your cash: ${cash} | Current armor: {equippedArmor}");
Console.Write("Which armor do you want to buy? (1-5, or 'exit'): ");
string? input = Console.ReadLine();
if (input == "exit")
{
Console.WriteLine("You say: \"Bye Steve\"\nSteve replies: \"Bye, come again, or you'll die from these dogs!\"");
return;
}
if (int.TryParse(input, out int choice) && choice >= 1 && choice <= 5)
{
int idx = choice - 1;
if (cash >= prices[idx])
{
if (inventory.Contains(armors[idx]))
{
Console.WriteLine($"Steve says: \"Maybe you just want to give me your money? You already have {armors[idx]}.\"");
return;
}
cash -= prices[idx];
inventory.Add(armors[idx]);
equippedArmor = armors[idx];
if (armors[idx] == "bone armor")
{
boneArmorDurability = killCount;
}
ModLoader.OnArmorEquipped(armors[idx]);
Console.WriteLine($"Steve throws you the {armors[idx]} and you put it on! He says:\n\"Good pick! The dogs won't know what hit em!\"\nCash left: ${cash} [ARMOR EQUIPPED]");
}
else
{
Console.WriteLine("Steve says: \"Hey, no stealing from your friends! Ya ain't got enough moolah to buy this shit!\"");
}
}
else
{
Console.WriteLine("Invalid choice! [ERROR]");
}
}

public static int GetArmorReduction(string armor)
{
int customReduction = ArmorRegistry.GetArmorReduction(armor);
if (customReduction >= 0)
{
return customReduction;
}
switch ((armor ?? "none").ToLower())
{
case "leather jacket": return 2;
case "bulletproof vest": return 5;
case "riot helmet": return 8;
case "military armor": return 18;
case "bone armor": return boneArmorDurability;
case "none": return 0;
default: return 0;
}
}

public static int ApplyArmorDamage(int incomingDamage)
{
if (equippedArmor.ToLower() == "bone armor" && boneArmorDurability > 0)
{
int reduction = Math.Min(boneArmorDurability, incomingDamage);
boneArmorDurability -= incomingDamage;
if (boneArmorDurability < 0) boneArmorDurability = 0;
if (boneArmorDurability == 0)
{
Console.WriteLine("Your bone armor shatters completely! [ARMOR BROKEN]");
equippedArmor = "None";
}
return Math.Max(0, incomingDamage - reduction);
}
else
{
int reduction = GetArmorReduction(equippedArmor);
return Math.Max(0, incomingDamage - reduction);
}
}

public static void Heal()
{
int cost = 20;
Console.WriteLine("\nYou slink to an evil doctor's back-alley clinic.");
if (cash >= cost)
{
if (playerHealth >= playerMaxHealth)
{
Console.WriteLine("The doctor sneers: \"You're fine, punk. Get lost.\"");
return;
}
cash -= cost;
playerHealth = playerMaxHealth;
Console.WriteLine($"You pay ${cost} for a shady patch-up. Health restored to full.\nDoctor says: \"You got healed, now get fucking lost, punk!\"");
}
else
{
Console.WriteLine("The doctor growls: \"No cash, no fix. Beat it!\"");
}
}
}