# Gangsta CS - Mod Support

## How to Create a Mod

1. Create a new C# class library project
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
}
```

## Example Mod

See `ExampleMod/KillTrackerMod.cs` for a complete example that tracks kills and damage.

## Building the Example Mod

```bash
cd ExampleMod
dotnet build
cp bin/Debug/net10.0/ExampleMod.dll ../bin/Debug/net10.0/Mods/
```

## Running with Mods

The game automatically loads all `.dll` files from the `Mods` folder on startup.
