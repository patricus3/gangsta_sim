using System;
using static GameState;

static class Combat
{
public static void Attack()
{
string[] defaultVictims = { "shopkeeper", "old lady", "teenager", "businessman", "jogger", "old man" };
string[] customVictims = CivilianRegistry.GetAllCivilians();
List<string> allVictims = new List<string>(defaultVictims);
allVictims.AddRange(customVictims);
string[] victims = allVictims.ToArray();
string[] cries = { "Oh no, please don't hurt me!", "Somebody help me!", "What's wrong with you?!", "No, no, no, stay away!", "I've got a family, please!", "I'm begging you!" };
string victim = victims[rand.Next(victims.Length)];
ModLoader.OnAttackStart(victim);
CivilianStats? customStats = CivilianRegistry.GetCivilianStats(victim);
int targetHp = customStats != null ? rand.Next(customStats.MinHp, customStats.MaxHp + 1) : rand.Next(20, 50);
int hit = rand.Next(10, 20) + Weapons.GetWeaponDamage(equippedWeapon);
Console.WriteLine($"\nYou spot a {victim}. You attack with your {equippedWeapon}! [fight stats]");
bool fled = false;
bool killed = false;
while (targetHp > 0 && playerHealth > 0 && !fled)
{
targetHp -= hit;
Console.WriteLine($"You hit the {victim} for {hit} damage. Their health: {Math.Max(0, targetHp)} [HIT]");
string s = cries[rand.Next(cries.Length)];
Console.WriteLine($"The {victim} screams: \"{s}\"");
if (targetHp <= 0)
{
Console.WriteLine($"The {victim} collapses, whimpering!");
int loot = customStats != null ? rand.Next(customStats.MinLoot, customStats.MaxLoot + 1) : rand.Next(5, 30);
cash += loot;
ModLoader.OnCashGained(loot, cash);
killCount++;
ModLoader.OnKill(killCount);
playerHealth = Math.Min(playerMaxHealth, playerHealth + 1);
Console.WriteLine($"You rummage their pockets and find ${loot}. Kill count: {killCount}");
if (victim == "old man" && rand.Next(1, 101) <= 50) Console.WriteLine("The old man spits out blood.");
killed = true;
PanicOnVictimDeath(victim, true);
CheckCrowdGathering(victim, false);
CheckPolice(victim, false);
break;
}
int enemyHit = rand.Next(1, 10);
int actualDamage = Shop.ApplyArmorDamage(enemyHit);
playerHealth -= actualDamage;
if (actualDamage > 0) ModLoader.OnDamageTaken(actualDamage, playerHealth);
Console.WriteLine($"The {victim} flails at you for {actualDamage} damage. Your health: {playerHealth}");
if (playerHealth < 20)
{
fled = TryFlee("civilian");
if (fled) break;
}
if (!fled && killCount >= 3 && rand.Next(0, 5) < killCount / 3)
{
fled = FightPolice(victim, true);
if (fled) break;
}
}
ModLoader.OnAttackEnd(victim, killed);
}

public static bool FightPolice(string civilianType, bool civilianAlive)
{
int squads = Math.Min(3, killCount / 3);
if (squads <= 0) squads = 1;
int copsHp = 40 * squads;
int hit = rand.Next(10, 20) + Weapons.GetWeaponDamage(equippedWeapon);
string[] crowdShouts = { "Oh no, he killed police!", "Run, he's crazy!", "The streets aren't safe anymore!", "Someone stop this lunatic!", "We're all gonna die!" };
string[] victimShouts = { "They're dead! Oh god!", "You're a monster!", "I'm next, aren't I?!", "No one can save me now!", "What have you done?!" };
Console.WriteLine($"\n{squads} cop{(squads > 1 ? "s" : "")} show up! \"Freeze, scum!\"");
if (civilianAlive) Console.WriteLine($"The {civilianType} screams: \"Save me, please!\" [police fight stats:]"); else Console.WriteLine("[police fight stats]");
bool fled = false;
while (copsHp > 0 && playerHealth > 0 && !fled)
{
copsHp -= hit;
Console.WriteLine($"You hit the police for {hit} damage. Their health: {Math.Max(0, copsHp)} [HIT]");
if (copsHp <= 0)
{
Console.WriteLine("The cops are down!");
int loot = rand.Next(20, 50);
cash += loot;
Console.WriteLine($"You grab ${loot} from their gear.");
if (rand.Next(0, 100) > 89 && !inventory.Contains("machinegun"))
{
inventory.Add("machinegun");
Console.WriteLine("The cops had a machinegun! You scoop it up hastily");
}
string c = crowdShouts[rand.Next(crowdShouts.Length)];
Console.WriteLine($"Nearby civilians scream: \"{c}\"");
if (civilianAlive)
{
string v = victimShouts[rand.Next(victimShouts.Length)];
Console.WriteLine($"The {civilianType} screams: \"{v}\"");
}
CheckCrowdGathering(civilianType, civilianAlive);
return false;
}
int dmg = rand.Next(5, 15) * squads;
int actualDamage = Shop.ApplyArmorDamage(dmg);
playerHealth -= actualDamage;
if (actualDamage > 0) ModLoader.OnDamageTaken(actualDamage, playerHealth);
Console.WriteLine($"The cops shoot back for {actualDamage} damage. Your health: {playerHealth}");
if (playerHealth < 20)
{
fled = TryFlee("police");
if (fled)
{
Console.WriteLine("The cops radio in: \"The gangster fled! Send the patrols!\"");
return true;
}
}
}
return fled;
}

public static void FightCrowd(string originalCivilianType, bool originalCivilianAlive)
{
string[] crowdTypes = { "bystander", "gawker", "passerby", "rubberneck", "witness", "old dramaseeker" };
int crowd = rand.Next(3, 6);
int crowdHp = crowd * 30;
int hit = rand.Next(15, 25) + Weapons.GetWeaponDamage(equippedWeapon);
string[] screams = { "He's killing us all!", "Run for your lives!", "This guy's insane!", "Help, he's a maniac!", "We're doomed!" };
Console.WriteLine($"\nYou lash out at a {crowdTypes[rand.Next(crowdTypes.Length)]} in the crowd!");
Console.WriteLine($"The crowd of {crowd} fights back as one! Total health: {crowdHp}");
while (crowdHp > 0 && playerHealth > 0)
{
crowdHp -= hit;
Console.WriteLine($"You smash into the crowd for {hit} damage. Crowd health: {Math.Max(0, crowdHp)}!");
if (crowdHp <= 0)
{
Console.WriteLine("The crowd scatters, bodies everywhere!");
int loot = rand.Next(10, 20) * crowd;
cash += loot;
ModLoader.OnCashGained(loot, cash);
killCount += crowd;
ModLoader.OnKill(killCount);
Console.WriteLine($"You loot ${loot} from the chaos. Kill count: {killCount}");
string s = screams[rand.Next(screams.Length)];
Console.WriteLine($"Surviving civilians scream: \"{s}\"");
if (originalCivilianAlive) Console.WriteLine($"The {originalCivilianType} screams: \"You're wiping us all out!\" [VICTIM PANIC]");
CheckPolice(originalCivilianType, originalCivilianAlive);
break;
}
int dmg = rand.Next(5, 10) * crowd;
int actualDamage = Shop.ApplyArmorDamage(dmg);
playerHealth -= actualDamage;
if (actualDamage > 0) ModLoader.OnDamageTaken(actualDamage, playerHealth);
Console.WriteLine($"The crowd swarms you for {actualDamage} damage. Your health: {playerHealth}");
if (playerHealth < 20 && TryFlee("crowd")) break;
PanicOnVictimDeath(originalCivilianType, originalCivilianAlive);
}
}

public static bool TryFlee(string enemyType)
{
if (playerHealth > 0)
{
Console.Write($"Your health is low ({playerHealth} HP)! Try to flee from the {enemyType}? (yes/no): ");
string? choice = Console.ReadLine()?.ToLower();
if (choice == "yes" || choice == "y")
{
if (rand.Next(1, 101) <= 50)
{
Console.WriteLine($"You run away from the {enemyType} and are on the run!");
return true;
}
Console.WriteLine($"You try to run but the {enemyType} catches up!");
return false;
}
}
return false;
}

public static void CheckPolice(string civilianType, bool civilianAlive)
{
if (killCount >= 3 && rand.Next(0, 5) < killCount / 3) FightPolice(civilianType, civilianAlive);
}

public static void CheckCrowdGathering(string civilianType, bool civilianAlive)
{
if (rand.Next(1, 101) <= 50)
{
Console.WriteLine("\nA crowd of onlookers gathers nearby, whispering and staring!");
Console.Write("Do you want to attack one of them? (yes/no): ");
if ((Console.ReadLine()?.ToLower() ?? "") == "yes")
{
FightCrowd(civilianType, civilianAlive);
return;
}
Console.WriteLine("You let the crowd disperse. [CROWD LEAVES]");
CheckPolice(civilianType, civilianAlive);
}
else
{
Console.WriteLine($"\nThe {civilianType} flees, panicked!");
}
}

public static void PanicOnVictimDeath(string civilianType, bool civilianAlive)
{
string[] shouts = { "He's dead! Oh no!", "Murderer! Run!", "We're next!", "This can't be happening!", "Blood everywhere!" };
string shout = shouts[rand.Next(shouts.Length)];
Console.WriteLine($"Nearby civilians scream: \"{shout}\"");
if (civilianAlive) Console.WriteLine($"The {civilianType} screams: \"I don't wanna die too!\"");
}

public static void Steal()
{
string[] weapons = { "knife", "pistol", "bat" };
string w = weapons[rand.Next(weapons.Length)];
int chance = rand.Next(1, 101);
Console.WriteLine("\nYou try to steal a weapon from a shady guy.");
if (chance > 30)
{
if (!inventory.Contains(w))
{
inventory.Add(w);
Console.WriteLine($"Success! You stole a {w}. [ITEM ADDED]");
}
else
{
Console.WriteLine("You already have that weapon. You let it go.");
}
return;
}
Console.WriteLine("Busted! The guy fights back!");
int dmg = rand.Next(10, 25);
int actualDamage = Shop.ApplyArmorDamage(dmg);
playerHealth -= actualDamage;
if (actualDamage > 0) ModLoader.OnDamageTaken(actualDamage, playerHealth);
Console.WriteLine($"You take {actualDamage} damage. Your health: {playerHealth} [DAMAGE TAKEN]");
}

public static void BankHeist()
{
if (currentGang == "None")
{
Console.WriteLine("\nYou need to join a gang first to pull off a bank heist!");
return;
}
ModLoader.OnBankHeistStart();
Gang? gang = gangs.Find(g => g.Name == currentGang);
if (gang == null)
{
Console.WriteLine("Your gang seems to have vanished. Try joining one again.");
return;
}
Console.WriteLine($"\nYou and the {currentGang} crew storm a bank! Alarms blare!");
int securityHp = 150;
int crewDps = gang.MemberCount * 5;
while (securityHp > 0 && playerHealth > 0)
{
int selfHit = rand.Next(15, 25) + Weapons.GetWeaponDamage(equippedWeapon);
int total = selfHit + crewDps;
securityHp -= total;
Console.WriteLine($"You hit for {selfHit} and your {currentGang} crew adds {crewDps} damage! Security health: {Math.Max(0, securityHp)}");
if (securityHp <= 0)
{
Console.WriteLine("The security is down! You and the crew raid the vault!");
int haul = rand.Next(1000, 2000) + gang.MemberCount * 100;
int share = haul / (gang.MemberCount + 1);
cash += share;
ModLoader.OnCashGained(share, cash);
killCount += 3;
ModLoader.OnKill(killCount);
Console.WriteLine($"The {currentGang} splits the ${haul} haul. Your share: ${share}!");
Console.WriteLine($"Added 3 kills. Total kills: {killCount}");
FightPolice("bank teller", false);
break;
}
int dmg = rand.Next(20, 35);
int actualDamage = Shop.ApplyArmorDamage(dmg);
playerHealth -= actualDamage;
if (actualDamage > 0) ModLoader.OnDamageTaken(actualDamage, playerHealth);
Console.WriteLine($"Security shoots back at you for {actualDamage} damage! Your health: {playerHealth}");
if (rand.Next(1, 101) <= 20)
{
Console.WriteLine($"A {currentGang} member takes a hit and goes down!");
gang.MemberCount = Math.Max(1, gang.MemberCount - 1);
crewDps = gang.MemberCount * 5;
}
if (playerHealth < 20 && TryFlee("bank security"))
{
Console.WriteLine($"You and the remaining {currentGang} crew bolt from the bank, empty-handed!");
break;
}
}
}
}