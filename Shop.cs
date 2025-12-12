using System;
using static GameState;

static class Shop
{
public static string GetArmorShopOptions()
{
string[] armors = { "leather jacket", "bulletproof vest", "riot helmet", "military armor", "bone armor" };
int[] prices = { 30, 60, 90, 200, 500 };
int[] reductions = { 2, 5, 8, 18, 0 };
reductions[4] = killCount;
string result = "\nYou duck into a shady armor shop.\n";
result += "Steve the shopkeeper says: \"Heya Frank! Maybe you need some good pieces to defend yourself against some punks, please don't call the dogs on me\"\n";
result += "Steve says: \"Here's what I have on sale, if you are a cheapskate or not, ya find something for yarself\"\n";
for (int i = 0; i < armors.Length; i++)
{
if (armors[i] != "bone armor") result += $"{i + 1}. {armors[i]} - ${prices[i]} (Reduces damage by {reductions[i]})\n";
else result += $"{i + 1}. Bone armor - ${prices[i]} (Reduces damage by {reductions[i]}, increases with kills)\n";
}
result += $"Your cash: ${cash} | Current armor: {equippedArmor}\n";
if (hasBrokenBoneArmor) result += "You have broken bone armor that can be repaired.\n";
result += "You browse the selection and make your choice.\n";
return result;
}

public static string BuyArmor(int choice)
{
string[] armors = { "leather jacket", "bulletproof vest", "riot helmet", "military armor", "bone armor" };
int[] prices = { 30, 60, 90, 200, 500 };
string result = "";

if (choice < 1 || choice > armors.Length)
{
result += "Invalid choice.\n";
return result;
}

int idx = choice - 1;
if (cash >= prices[idx])
{
if (inventory.Contains(armors[idx]))
{
result += $"Steve says: \"Maybe you just want to give me your money? You already have {armors[idx]}.\"\n";
return result;
}
cash -= prices[idx];
inventory.Add(armors[idx]);
equippedArmor = armors[idx];
if (armors[idx] == "bone armor")
{
boneArmorDurability = killCount;
}
ModLoader.OnArmorEquipped(armors[idx]);
result += $"Steve throws you the {armors[idx]} and you put it on! He says:\n\"Good pick! The dogs won't know what hit em!\"\nCash left: ${cash} [ARMOR EQUIPPED]\n";
}
else
{
result += "Steve says: \"Hey, no stealing from your friends! Ya ain't got enough moolah to buy this shit!\"\n";
}
return result;
}

public static string VisitArmorShop()
{
// For backward compatibility, we'll simulate a random purchase
Random rand = new Random();
int choice = rand.Next(1, 6);
return GetArmorShopOptions() + BuyArmor(choice);
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
equippedArmor = "None";
hasBrokenBoneArmor = true;
}
return Math.Max(0, incomingDamage - reduction);
}
else
{
int reduction = GetArmorReduction(equippedArmor);
return Math.Max(0, incomingDamage - reduction);
}
}

public static string Heal()
{
int cost = 20;
string result = "\nYou slink to an evil doctor's back-alley clinic.\n";
if (cash >= cost)
{
if (playerHealth >= playerMaxHealth)
{
result += "The doctor sneers: \"You're fine, punk. Get lost.\"\n";
return result;
}
cash -= cost;
playerHealth = playerMaxHealth;
result += $"You pay ${cost} for a shady patch-up. Health restored to full.\nDoctor says: \"You got healed, now get fucking lost, punk!\"\n";
}
else
{
result += "The doctor growls: \"No cash, no fix. Beat it!\"\n";
}
return result;
}

public static string RepairBoneArmor()
{
string result = "\nYou visit Steve's armor repair shop.\n";

// Check if player has broken bone armor
if (!hasBrokenBoneArmor)
{
result += "Steve says: \"You don't have any broken bone armor to repair, pal!\"\n";
return result;
}

// Calculate repair cost based on kill count
int repairCost = killCount;

result += $"Steve examines your broken bone armor and says: \"This'll cost you {repairCost} bucks to fix, based on your kill count.\"\n";

if (cash >= repairCost)
{
cash -= repairCost;
hasBrokenBoneArmor = false;
inventory.Add("bone armor");
equippedArmor = "bone armor";
boneArmorDurability = killCount;
result += $"You pay ${repairCost} and Steve fixes your bone armor. It's as good as new!\n";
result += "Steve says: \"Good as new, buddy! The dogs won't know what hit 'em!\"\n";
}
else
{
result += "Steve says: \"Come back when you've got the cash, tough guy!\"\n";
}

return result;
}
}