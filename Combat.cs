using System;
using static GameState;

static class Combat
{
public static string Attack()
{
string[] defaultVictims = { "shopkeeper", "old lady", "teenager", "businessman", "jogger", "old man" };
string[] customVictims = CivilianRegistry.GetAllCivilians();
List<string> allVictims = new List<string>(defaultVictims);
allVictims.AddRange(customVictims);
string[] victims = allVictims.ToArray();
string[] cries = { "Oh no, please don't hurt me!", "Somebody help me!", "What's wrong with you?!", "No, no, no, stay away!", "I've got a family, please!", "I'm begging you!" };
string victim = victims[rand.Next(victims.Length)];
CivilianStats? customStats = CivilianRegistry.GetCivilianStats(victim);
int targetHp = customStats != null ? rand.Next(customStats.MinHp, customStats.MaxHp + 1) : rand.Next(20, 50);
int hit = rand.Next(10, 20) + Weapons.GetWeaponDamage(equippedWeapon);
string result = $"\nYou spot a {victim}. You attack with your {equippedWeapon}! [fight stats]\n";
string modMessages = ModLoader.OnAttackStart(victim);
if (!string.IsNullOrEmpty(modMessages))
{
    result += modMessages + "\n";
}
bool fled = false;
bool killed = false;
while (targetHp > 0 && playerHealth > 0 && !fled)
{
targetHp -= hit;
result += $"You hit the {victim} for {hit} damage. Their health: {Math.Max(0, targetHp)} [HIT]\n";
string s = cries[rand.Next(cries.Length)];
result += $"The {victim} screams: \"{s}\"\n";
if (targetHp <= 0)
{
result += $"The {victim} collapses, whimpering!\n";
    // Call mod OnCivilianDeath and capture messages
    string deathMessages = ModLoader.OnCivilianDeath(victim);
    if (!string.IsNullOrEmpty(deathMessages))
    {
        result += deathMessages + "\n";
    }
int loot = customStats != null ? rand.Next(customStats.MinLoot, customStats.MaxLoot + 1) : rand.Next(5, 30);
cash += loot;
ModLoader.OnCashGained(loot, cash);
killCount++;
ModLoader.OnKill(killCount);
playerHealth = Math.Min(playerMaxHealth, playerHealth + 1);
result += $"You rummage their pockets and find ${loot}. Kill count: {killCount}\n";
if (victim == "old man" && rand.Next(1, 101) <= 50) result += "The old man spits out blood.\n";
killed = true;
result += PanicOnVictimDeath(victim, true);
result += CheckCrowdGathering(victim, false);
result += CheckPolice(victim, false);
break;
}
int enemyHit = rand.Next(1, 10);
int actualDamage = Shop.ApplyArmorDamage(enemyHit);
playerHealth -= actualDamage;
string damageMessages = "";
if (actualDamage > 0) damageMessages = ModLoader.OnDamageTaken(actualDamage, playerHealth);
result += $"The {victim} flails at you for {actualDamage} damage. Your health: {playerHealth}\n";
if (!string.IsNullOrEmpty(damageMessages))
{
    result += damageMessages + "\n";
}

// Check if player is dead
if (playerHealth <= 0)
{
    result += $"\nThe {victim} beats the shit out of you and calls the cops!\n";
    int moneyLost = cash / 2;
    cash -= moneyLost;
    playerHealth = playerMaxHealth / 2;
    result += $"You lose ${moneyLost} and are revived with {playerHealth} health.\n";
    SaveLoad.SaveGame();
    
    // Player is defeated, don't process additional crowd gathering or police checks
    // These will be handled when the player recovers and continues playing
    return result;
}

if (playerHealth < 20)
{
fled = TryFlee("civilian");
if (fled) 
    {
        result += "You leave the panicked civilian, you're too wounded to finish them off.\n";
        break;
    }
}
if (!fled && killCount >= 3 && rand.Next(0, 5) < killCount / 3)
{
result += FightPolice(victim, true);
    
    // Check if the player fled from police
    if (result.Contains("The gangster fled!"))
    {
        // Player fled from police, so we should break out of the civilian fight loop
        break;
    }
// The fight result will indicate if the player fled
// For now, we'll continue the loop normally
}
}
ModLoader.OnAttackEnd(victim, killed);
return result;
}

public static string FightPolice(string civilianType, bool civilianAlive)
{
int squads = Math.Min(3, killCount / 3);
if (squads <= 0) squads = 1;
int copsHp = 40 * squads;
int hit = rand.Next(10, 20) + Weapons.GetWeaponDamage(equippedWeapon);
string[] crowdShouts = { "Oh no, he killed police!", "Run, he's crazy!", "The streets aren't safe anymore!", "Someone stop this lunatic!", "We're all gonna die!" };
string[] victimShouts = { "They're dead! Oh god!", "You're a monster!", "I'm next, aren't I?!", "No one can save me now!", "What have you done?!" };
string result = $"\n{ squads } cop{(squads > 1 ? "s" : "")} show up! \"Freeze, scum!\"\n";
if (civilianAlive) result += $"The {civilianType} screams: \"Save me, please!\" [police fight stats:]\n"; else result += "[police fight stats]\n";
bool fled = false;
while (copsHp > 0 && playerHealth > 0 && !fled)
{
copsHp -= hit;
result += $"You hit the police for {hit} damage. Their health: {Math.Max(0, copsHp)} [HIT]\n";
if (copsHp <= 0)
{
result += "The cops are down!\n";
int loot = rand.Next(20, 50);
cash += loot;
result += $"You grab ${loot} from their gear.\n";
if (rand.Next(0, 100) > 89 && !inventory.Contains("machinegun"))
{
inventory.Add("machinegun");
result += "The cops had a machinegun! You scoop it up hastily\n";
}
string c = crowdShouts[rand.Next(crowdShouts.Length)];
result += $"Nearby civilians scream: \"{c}\"\n";
if (civilianAlive)
{
string v = victimShouts[rand.Next(victimShouts.Length)];
result += $"The {civilianType} screams: \"{v}\"\n";
}
result += CheckCrowdGathering(civilianType, civilianAlive);
return result;
}
int dmg = rand.Next(5, 15) * squads;
int actualDamage = Shop.ApplyArmorDamage(dmg);
playerHealth -= actualDamage;
string damageMessages = "";
if (actualDamage > 0) damageMessages = ModLoader.OnDamageTaken(actualDamage, playerHealth);
result += $"The cops shoot back for {actualDamage} damage. Your health: {playerHealth}\n";
if (!string.IsNullOrEmpty(damageMessages))
{
    result += damageMessages + "\n";
}

// Check if player is dead
if (playerHealth <= 0)
{
    result += "\nThe cops beat the shit out of you and take you to the hospital!\n";
    int moneyLost = cash / 2;
    cash -= moneyLost;
    playerHealth = playerMaxHealth / 2;
    result += $"You lose ${moneyLost} and are revived with {playerHealth} health.\n";
    SaveLoad.SaveGame();
    
    // Player is defeated, don't process additional crowd gathering
    // This will be handled when the player recovers and continues playing
    return result;
}

if (playerHealth < 20)
{
fled = TryFlee("police");
if (fled)
{
result += "The cops radio in: \"The gangster fled! Send the patrols!\"\n";
return result;
}
}
}
return result;
}

public static string FightCrowd(string originalCivilianType, bool originalCivilianAlive)
{
string[] crowdTypes = { "bystander", "gawker", "passerby", "rubberneck", "witness", "old dramaseeker" };
int crowd = rand.Next(3, 6);
int crowdHp = crowd * 30;
int hit = rand.Next(15, 25) + Weapons.GetWeaponDamage(equippedWeapon);
string[] screams = { "He's killing us all!", "Run for your lives!", "This guy's insane!", "Help, he's a maniac!", "We're doomed!" };
string result = $"\nYou lash out at a {crowdTypes[rand.Next(crowdTypes.Length)]} in the crowd!\n";
result += $"The crowd of {crowd} fights back as one! Total health: {crowdHp}\n";
while (crowdHp > 0 && playerHealth > 0)
{
crowdHp -= hit;
result += $"You smash into the crowd for {hit} damage. Crowd health: {Math.Max(0, crowdHp)}!\n";
if (crowdHp <= 0)
{
result += "The crowd scatters, bodies everywhere!\n";
int loot = rand.Next(10, 20) * crowd;
cash += loot;
ModLoader.OnCashGained(loot, cash);
killCount += crowd;
ModLoader.OnKill(killCount);
result += $"You loot ${loot} from the chaos. Kill count: {killCount}\n";
string s = screams[rand.Next(screams.Length)];
result += $"Surviving civilians scream: \"{s}\"\n";
if (originalCivilianAlive) result += $"The {originalCivilianType} screams: \"You're wiping us all out!\" [VICTIM PANIC]\n";
result += CheckPolice(originalCivilianType, originalCivilianAlive);
break;
}
int dmg = rand.Next(5, 10) * crowd;
int actualDamage = Shop.ApplyArmorDamage(dmg);
playerHealth -= actualDamage;
string damageMessages = "";
if (actualDamage > 0) damageMessages = ModLoader.OnDamageTaken(actualDamage, playerHealth);
result += $"The crowd swarms you for {actualDamage} damage. Your health: {playerHealth}\n";
if (!string.IsNullOrEmpty(damageMessages))
{
    result += damageMessages + "\n";
}

// Check if player is dead
if (playerHealth <= 0)
{
    result += "\nThe crowd beats the shit out of you and calls the cops!\n";
    int moneyLost = cash / 2;
    cash -= moneyLost;
    playerHealth = playerMaxHealth / 2;
    result += $"You lose ${moneyLost} and are revived with {playerHealth} health.\n";
    SaveLoad.SaveGame();
    
    // Player is defeated, don't process additional police checks
    // These will be handled when the player recovers and continues playing
    return result;
}
if (playerHealth < 20 && TryFlee("crowd")) 
{
result += "You manage to escape from the crowd!\n";
break;
}
result += PanicOnVictimDeath(originalCivilianType, originalCivilianAlive);
}
return result;
}

public static bool TryFlee(string enemyType)
{
// In GUI version, we'll auto-flee when health is low
if (playerHealth > 0 && playerHealth < 20)
{
    // 50% chance to flee when health is low
    if (rand.Next(1, 101) <= 50)
    {
        return true;
    }
}
return false;
}

public static string CheckPolice(string civilianType, bool civilianAlive)
{
string result = "";
if (killCount >= 3 && rand.Next(0, 5) < killCount / 3) 
{
result += FightPolice(civilianType, civilianAlive);
}
return result;
}

public static string CheckCrowdGathering(string civilianType, bool civilianAlive)
{
    string result = "";
    if (rand.Next(1, 101) <= 50)
    {
        result += "\nA crowd of onlookers gathers nearby, whispering and staring!\n";
        // For GUI version, we'll indicate that a choice is needed and store civilian info
        result += "[CHOICE_NEEDED:ATTACK_CROWD:" + civilianType + ":" + civilianAlive + "]";
    }
    else
    {
        result += $"\nThe {civilianType} flees, panicked!\n";
    }
    return result;
}

public static string PanicOnVictimDeath(string civilianType, bool civilianAlive)
{
string[] shouts = { "He's dead! Oh no!", "Murderer! Run!", "We're next!", "This can't be happening!", "Blood everywhere!" };
string shout = shouts[rand.Next(shouts.Length)];
string result = $"Nearby civilians scream: \"{shout}\"\n";
if (civilianAlive) result += $"The {civilianType} screams: \"I don't wanna die too!\"\n";
return result;
}

public static string Steal()
{
string[] weapons = { "knife", "pistol", "bat" };
string w = weapons[rand.Next(weapons.Length)];
int chance = rand.Next(1, 101);
string result = "\nYou try to steal a weapon from a shady guy.\n";
if (chance > 30)
{
if (!inventory.Contains(w))
{
inventory.Add(w);
result += $"Success! You stole a {w}. [ITEM ADDED]\n";
}
else
{
result += "You already have that weapon. You let it go.\n";
}
return result;
}
result += "Busted! The guy fights back!\n";
int dmg = rand.Next(10, 25);
int actualDamage = Shop.ApplyArmorDamage(dmg);
playerHealth -= actualDamage;
string damageMessages = "";
if (actualDamage > 0) damageMessages = ModLoader.OnDamageTaken(actualDamage, playerHealth);
result += $"You take {actualDamage} damage. Your health: {playerHealth} [DAMAGE TAKEN]\n";
if (!string.IsNullOrEmpty(damageMessages))
{
    result += damageMessages + "\n";
}
return result;
}

public static string BankHeist()
{
if (currentGang == "None")
{
return "\nYou need to join a gang first to pull off a bank heist!\n";
}
ModLoader.OnBankHeistStart();
Gang? gang = gangs.Find(g => g.Name == currentGang);
if (gang == null)
{
return "Your gang seems to have vanished. Try joining one again.\n";
}
string result = $"\nYou and the {currentGang} crew storm a bank! Alarms blare!\n";
int securityHp = 150;
int crewDps = gang.MemberCount * 5;
while (securityHp > 0 && playerHealth > 0)
{
int selfHit = rand.Next(15, 25) + Weapons.GetWeaponDamage(equippedWeapon);
int total = selfHit + crewDps;
securityHp -= total;
result += $"You hit for {selfHit} and your {currentGang} crew adds {crewDps} damage! Security health: {Math.Max(0, securityHp)}\n";
if (securityHp <= 0)
{
result += "The security is down! You and the crew raid the vault!\n";
int haul = rand.Next(1000, 2000) + gang.MemberCount * 100;
int share = haul / (gang.MemberCount + 1);
cash += share;
ModLoader.OnCashGained(share, cash);
killCount += 3;
ModLoader.OnKill(killCount);
result += $"The {currentGang} splits the ${haul} haul. Your share: ${share}!\n";
result += $"Added 3 kills. Total kills: {killCount}\n";
result += CheckPolice("bank teller", false);
break;
}
int dmg = rand.Next(20, 35);
int actualDamage = Shop.ApplyArmorDamage(dmg);
playerHealth -= actualDamage;
string damageMessages = "";
if (actualDamage > 0) damageMessages = ModLoader.OnDamageTaken(actualDamage, playerHealth);
result += $"Security shoots back at you for {actualDamage} damage! Your health: {playerHealth}\n";
if (!string.IsNullOrEmpty(damageMessages))
{
    result += damageMessages + "\n";
}

// Check if player is dead
if (playerHealth <= 0)
{
    result += "\nThe bank security beats the shit out of you and calls the cops!\n";
    int moneyLost = cash / 2;
    cash -= moneyLost;
    playerHealth = playerMaxHealth / 2;
    result += $"You lose ${moneyLost} and are revived with {playerHealth} health.\n";
    SaveLoad.SaveGame();
    
    // Player is defeated, don't process additional police checks
    // These will be handled when the player recovers and continues playing
    break;
}

if (rand.Next(1, 101) <= 20)
{
result += $"A {currentGang} member takes a hit and goes down!\n";
gang.MemberCount = Math.Max(1, gang.MemberCount - 1);
crewDps = gang.MemberCount * 5;
}
if (playerHealth < 20 && TryFlee("bank security"))
{
result += $"You and the remaining {currentGang} crew bolt from the bank, empty-handed!\n";
break;
}
}
return result;
}
}