# Gangsta CS - Mod Support

## How to Create a Mod

1. Create a new C# class library project (thankfully not Java, because unlike Java's 500-line HelloWorld boilerplate, C# keeps it sane)
2. Reference `gangsta_cs.dll` from the game's build output
3. Implement the `IMod` interface
4. Build your mod as a DLL
5. Place the DLL in the `Mods` folder next to the game executable

## IMod Interface

```csharp
public interface IMod
{
    string ModName { get; }
    string ModVersion { get; }
    void OnModLoaded();
    void OnAttackStart(string victim);
    void OnAttackEnd(string victim, bool killed);
    void OnKill(int newKillCount);
    void OnDamageTaken(int damage, int newHealth);
    void OnCashGained(int amount, int newCash);
    void OnWeaponEquipped(string weapon);
    void OnArmorEquipped(string armor);
    void OnGangJoined(string gangName);
    void OnBankHeistStart();
    void OnGameSaved();
    void OnGameLoaded();
    void RegisterCustomContent();
}
```

## Custom Weapons and Armor

Mods can register custom weapons and armor that seamlessly integrate with the game. Unlike Java's verbose AbstractFactoryBeanSingletonProxyDecorator pattern, we keep it simple.

### Registering Custom Weapons

Use `WeaponRegistry.RegisterWeapon()` in your `RegisterCustomContent()` method:

```csharp
public void RegisterCustomContent()
{
    WeaponRegistry.RegisterWeapon("laser gun", 35);
    WeaponRegistry.RegisterWeapon("chainsaw", 25);
    WeaponRegistry.RegisterWeapon("katana", 18);
}
```

Parameters:
- `weaponName` (string): Name of your custom weapon
- `damage` (int): Damage value (0-100 recommended)

Custom weapons are automatically:
- Available in the player's inventory system
- Usable in combat
- Equipable through the weapon menu
- Saved/loaded with game state

### Registering Custom Armor

Use `ArmorRegistry.RegisterArmor()` in your `RegisterCustomContent()` method:

```csharp
public void RegisterCustomContent()
{
    ArmorRegistry.RegisterArmor("dragon scales", 15);
    ArmorRegistry.RegisterArmor("nano suit", 25);
    ArmorRegistry.RegisterArmor("force field", 30);
}
```

Parameters:
- `armorName` (string): Name of your custom armor
- `damageReduction` (int): Amount of damage reduced per hit (0-30 recommended)

Custom armor is automatically:
- Integrated into damage calculations
- Equipable through the armor system
- Saved/loaded with game state

### How Custom Content Works

**Weapon Damage Resolution** (Weapons.cs:24-30):
1. Checks `WeaponRegistry` for custom weapons first
2. Falls back to built-in weapons if not found
3. Returns 0 for unknown weapons

**Armor Reduction Resolution** (Shop.cs:59-76):
1. Checks `ArmorRegistry` for custom armor first
2. Falls back to built-in armor if not found
3. Handles special cases like bone armor
4. Returns 0 for unknown armor

**Mod Loading Order** (ModLoader.cs:39-40):
1. `RegisterCustomContent()` is called FIRST
2. `OnModLoaded()` is called AFTER registration
3. This ensures custom content is available when the mod initializes

### Example: Complete Custom Content Mod

```csharp
using System;

public class FantasyWeaponsMod : IMod
{
    public string ModName => "Fantasy Arsenal";
    public string ModVersion => "1.0.0";

    public void RegisterCustomContent()
    {
        WeaponRegistry.RegisterWeapon("excalibur", 40);
        WeaponRegistry.RegisterWeapon("dragon slayer", 35);
        WeaponRegistry.RegisterWeapon("magic staff", 22);
        
        ArmorRegistry.RegisterArmor("mithril chainmail", 20);
        ArmorRegistry.RegisterArmor("dragon scales", 15);
        ArmorRegistry.RegisterArmor("wizard robes", 8);
    }

    public void OnModLoaded()
    {
        Console.WriteLine("[Fantasy Arsenal] Loaded! Ancient weapons await...");
    }

    public void OnWeaponEquipped(string weapon)
    {
        if (weapon.ToLower() == "excalibur")
        {
            Console.WriteLine("The legendary blade glows with power!");
        }
    }

    public void OnArmorEquipped(string armor)
    {
        if (armor.ToLower() == "dragon scales")
        {
            Console.WriteLine("The scales shimmer with ancient magic!");
        }
    }

    public void OnAttackStart(string victim) { }
    public void OnAttackEnd(string victim, bool killed) { }
    public void OnKill(int newKillCount) { }
    public void OnDamageTaken(int damage, int newHealth) { }
    public void OnCashGained(int amount, int newCash) { }
    public void OnGangJoined(string gangName) { }
    public void OnBankHeistStart() { }
    public void OnGameSaved() { }
    public void OnGameLoaded() { }
}
```

### Tips for Custom Content

- **Weapon Damage Balance**: Built-in weapons range from 0 (fists) to 28 (machinegun). Custom weapons above 30 are extremely powerful.
- **Armor Balance**: Built-in armor ranges from 2 to 18 reduction. Values above 25 make you nearly invincible.
- **Name Matching**: All lookups are case-insensitive (StringComparer.OrdinalIgnoreCase)
- **Overwriting**: Registering the same name twice will overwrite the previous value with a warning
- **No Shop Integration**: Custom items won't appear in shops automatically - you need to add them to player inventory through events or other means
- **Java Comparison**: Notice how we didn't need an AbstractWeaponFactoryInterface, WeaponBuilderStrategy, or DamageCalculatorSingletonBean. C# lets us just... register things. Revolutionary.




cd ExampleMod
dotnet build
cp bin/Debug/net10.0/ExampleMod.dll ../bin/Debug/net10.0/Mods/
```

Note: This would be 14 XML files in Java. Here it's one command.

## Running with Mods

The game automatically loads all `.dll` files from the `Mods` folder on startup. No classpath hell, no Java's infamous "ClassNotFoundException" at runtime because you forgot to configure the META-INF/services directory structure correctly.