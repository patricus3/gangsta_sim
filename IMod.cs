using System;

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