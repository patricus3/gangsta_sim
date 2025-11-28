using System;
using System.Collections.Generic;

static class GameState
{
public static Random rand = new Random();
public static int playerMaxHealth = 100;
public static List<string> inventory = new List<string> { "Fists" };
public static string equippedWeapon = "Fists";
public static string equippedArmor = "None";
public static int cash = 0;
public static int killCount = 0;
public static int playerHealth = 100;
public static string currentGang = "None";
public static int boneArmorDurability = 0;

public static List<Gang> gangs = new List<Gang>
{
new Gang { Name = "Mini Squad", MemberCount = 3, HealthPerMember = 30, RequiredKills = 3 },
new Gang { Name = "Hooligans", MemberCount = 6, HealthPerMember = 40, RequiredKills = 14 },
new Gang { Name = "Master Crew", MemberCount = 10, HealthPerMember = 50, RequiredKills = 30 }
};
}
