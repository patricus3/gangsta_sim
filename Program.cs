using System;
using static GameState;

class Program
{
static void Main(string[] args)
{
Console.Title = "Gangsta Simulator";
ModLoader.LoadMods();
SaveLoad.LoadGame();
Console.WriteLine("Welcome to Street Gangster Sim!");
Console.WriteLine("You're a menace on the streets. Type commands to survive and thrive.");
Console.WriteLine("Commands: 'attack', 'steal', 'heal', 'equip', 'armor', 'check', 'save', 'exit', 'join gang', 'leave gang', 'attack bank'");
while (playerHealth > 0)
{
Console.Write("\nWhat do you want to do? ");
string? cmd = Console.ReadLine();
switch (cmd)
{
case "attack": Combat.Attack(); break;
case "steal": Combat.Steal(); break;
case "heal": Shop.Heal(); break;
case "equip": Weapons.EquipWeapon(); break;
case "armor": Shop.VisitArmorShop(); break;
case "check": GangSystem.CheckStatus(); break;
case "save": SaveLoad.SaveGame(); break;
case "exit": Console.WriteLine("Leaving the streets. Game over."); return;
case "join gang": GangSystem.JoinGang(); break;
case "leave gang": currentGang = "None"; Console.WriteLine("you leave the gang, the gang boss says: \"you'll regret it!\""); break;
case "attack bank": Combat.BankHeist(); break;
default: Console.WriteLine("Invalid command. Try 'attack', 'steal', 'heal', 'equip', 'armor', 'check', 'save', 'exit', 'join gang', 'leave gang', or 'attack bank'."); break;
}
playerMaxHealth = 100 + killCount;
}
Console.WriteLine("\nYou're down! Game over. Press enter to continue");
Console.ReadKey();
}
}
