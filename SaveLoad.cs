using System;
using System.Collections.Generic;
using System.IO;
using static GameState;

static class SaveLoad
{
public static void SaveGame()
{
    try
    {
        // Get the AppData directory and create proper subfolder structure
        string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string saveDirectory = Path.Combine(appDataDirectory, "patricus productions", "gangsta");
        string saveFile = Path.Combine(saveDirectory, "gangster_save.txt");
        
        
        // Create the save directory if it doesn't exist
        if (!Directory.Exists(saveDirectory))
        {
            try
            {
                Directory.CreateDirectory(saveDirectory);
                Console.WriteLine($"Created save directory: {saveDirectory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create save directory: {ex.Message}");
                // Fallback to current directory if AppData fails
                saveDirectory = "savegames";
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }
                saveFile = Path.Combine(saveDirectory, "gangster_save.txt");
            }
        }
        
        
        string[] contents =
        {
            playerHealth.ToString(),
            cash.ToString(),
            equippedWeapon,
            killCount.ToString(),
            string.Join(",", inventory),
            equippedArmor,
            currentGang,
            boneArmorDurability.ToString(),
            hasBrokenBoneArmor.ToString()
        };
        
        File.WriteAllLines(saveFile, contents);
        ModLoader.OnGameSaved();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to save game: {ex.Message}");
    }
}
public static void LoadGame()
{
    // Get the AppData directory and create proper subfolder structure
    string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    string saveDirectory = Path.Combine(appDataDirectory, "patricus productions", "gangsta");
    string saveFile = Path.Combine(saveDirectory, "gangster_save.txt");
    
    // Debug output to see where we're loading from
    Console.WriteLine($"AppData Directory: {appDataDirectory}");
    Console.WriteLine($"Save Directory: {saveDirectory}");
    Console.WriteLine($"Save File: {saveFile}");
    
    try
    {
        if (File.Exists(saveFile))
        {
            string[] data = File.ReadAllLines(saveFile);
            if (data.Length >= 9)
            {
                playerHealth = int.Parse(data[0]);
                // Ensure player health is at least 1 to prevent immediate game over
                if (playerHealth <= 0) playerHealth = 1;
                cash = int.Parse(data[1]);
                equippedWeapon = data[2];
                killCount = int.Parse(data[3]);
                inventory = new List<string>((data[4] ?? "Fists").Split(',', StringSplitOptions.RemoveEmptyEntries));
                if (inventory.Count == 0) inventory.Add("Fists");
                equippedArmor = data[5];
                currentGang = data[6];
                boneArmorDurability = int.Parse(data[7]);
                hasBrokenBoneArmor = bool.Parse(data[8]);
                Console.WriteLine("Game loaded from '%appdata%\\patricus productions\\gangsta\\gangster_save.txt'. [LOAD COMPLETE]");
                playerMaxHealth = 100 + killCount;
                ModLoader.OnGameLoaded();
            }
            else if (data.Length == 8)
            {
                playerHealth = int.Parse(data[0]);
                // Ensure player health is at least 1 to prevent immediate game over
                if (playerHealth <= 0) playerHealth = 1;
                cash = int.Parse(data[1]);
                equippedWeapon = data[2];
                killCount = int.Parse(data[3]);
                inventory = new List<string>((data[4] ?? "Fists").Split(',', StringSplitOptions.RemoveEmptyEntries));
                if (inventory.Count == 0) inventory.Add("Fists");
                equippedArmor = data[5];
                currentGang = data[6];
                boneArmorDurability = int.Parse(data[7]);
                hasBrokenBoneArmor = false;
                Console.WriteLine("Game loaded from '%appdata%\\patricus productions\\gangsta\\gangster_save.txt'. [LOAD COMPLETE]");
                playerMaxHealth = 100 + killCount;
                ModLoader.OnGameLoaded();
            }
            else if (data.Length == 6)
            {
                Console.WriteLine("legacy convert system, v 0.1 activated, please wait as we convert & load your save data!");
                cash = int.Parse(data[1]);
                equippedWeapon = data[2];
                killCount = int.Parse(data[3]);
                inventory = new List<string>((data[4] ?? "Fists").Split(',', StringSplitOptions.RemoveEmptyEntries));
                if (inventory.Count == 0) inventory.Add("Fists");
                equippedArmor = data[5];
                playerMaxHealth = 100 + killCount;
                currentGang = "None";
                boneArmorDurability = 0;
                hasBrokenBoneArmor = false;
                // Set player health to a reasonable value
                playerHealth = 100;
            }
            else
            {
                Console.WriteLine("Save file corrupted! Starting new game.");
                InitializeNewGame();
            }
        }
        else
        {
            Console.WriteLine("No save file found. Starting new game.");
            InitializeNewGame();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load game: {ex.Message}");
        Console.WriteLine("Starting new game due to load error.");
        InitializeNewGame();
    }
}

private static void InitializeNewGame()
{
    // Set default values
    playerHealth = 100;
    cash = 0;
    equippedWeapon = "Fists";
    killCount = 0;
    inventory = new List<string> { "Fists" };
    equippedArmor = "None";
    currentGang = "None";
    boneArmorDurability = 0;
    hasBrokenBoneArmor = false;
    playerMaxHealth = 100;
}
}