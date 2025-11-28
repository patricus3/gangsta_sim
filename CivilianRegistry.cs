using System;
using System.Collections.Generic;

public class CivilianStats
{
public int MinHp { get; set; }
public int MaxHp { get; set; }
public int MinLoot { get; set; }
public int MaxLoot { get; set; }

public CivilianStats(int minHp, int maxHp, int minLoot, int maxLoot)
{
MinHp = minHp;
MaxHp = maxHp;
MinLoot = minLoot;
MaxLoot = maxLoot;
}
}

public static class CivilianRegistry
{
public static List<string> CustomCivilians = new List<string>();
public static Dictionary<string, CivilianStats> CivilianStatsMap = new Dictionary<string, CivilianStats>(StringComparer.OrdinalIgnoreCase);

public static void RegisterCivilian(string civilianType)
{
if (CustomCivilians.Contains(civilianType))
{
return;
}
CustomCivilians.Add(civilianType);
}

public static void RegisterCivilian(string civilianType, int minHp, int maxHp, int minLoot, int maxLoot)
{
RegisterCivilian(civilianType);
CivilianStatsMap[civilianType] = new CivilianStats(minHp, maxHp, minLoot, maxLoot);
}

public static string[] GetAllCivilians()
{
return CustomCivilians.ToArray();
}

public static CivilianStats? GetCivilianStats(string civilianType)
{
if (CivilianStatsMap.TryGetValue(civilianType, out CivilianStats? stats))
{
return stats;
}
return null;
}
}
