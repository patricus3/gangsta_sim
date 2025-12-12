using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using static GameState;

namespace GangstaWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpeechCore? speechCore;
        private bool isScreenReaderEnabled = true;
        private string? pendingCrowdCivilianType;
        private bool pendingCrowdCivilianAlive;

        public MainWindow()
        {
            InitializeComponent();
            
            // Set up mod message callback
            ModLoader.OnMessageLogged = OnModMessageLogged;
            
            // Initialize game state
            ModLoader.LoadMods();
            SaveLoad.LoadGame();
            
            // Set up initial UI
            UpdateStatus();
            
            // Initialize SpeechCore after UI is initialized
            Loaded += MainWindow_Loaded;
            
            // Handle ESC key for mod info panel
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize SpeechCore
            try
            {
                speechCore = new SpeechCore();
                // Don't speak welcome message to avoid conflicts
            }
            catch (Exception ex)
            {
                // If SpeechCore fails, continue without it
                System.Diagnostics.Debug.WriteLine($"SpeechCore initialization failed: {ex.Message}");
                speechCore = null;
            }
        }

        private void Speak(string text)
        {
            try
            {
                if (speechCore != null && speechCore.IsLoaded())
                {
                    speechCore.Speak(text, false);
                }
            }
            catch (Exception ex)
            {
                // Silently handle any speech errors
                System.Diagnostics.Debug.WriteLine($"Speech error: {ex.Message}");
            }
        }
        
        private void OnModMessageLogged(string message)
        {
            // Speak the mod message
            Speak(message);
            
            // Update the result text block with the message
            string currentText = ResultTextBlock.Text;
            if (!string.IsNullOrEmpty(currentText))
            {
                ResultTextBlock.Text = currentText + "\n" + message;
            }
            else
            {
                ResultTextBlock.Text = message;
            }
        }

        private void UpdateStatus()
        {
            string statusText = $"Health: {playerHealth}/{playerMaxHealth} | Money: ${cash} | Kills: {killCount} | Gang: {currentGang}";
            StatusTextBlock.Text = statusText;
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            string result = Combat.Attack();
            ShowResult(result);
        }

        private void StealButton_Click(object sender, RoutedEventArgs e)
        {
            string result = Combat.Steal();
            ShowResult(result);
        }

        private void HealButton_Click(object sender, RoutedEventArgs e)
        {
            string result = Shop.Heal();
            ShowResult(result);
        }

        private void EquipButton_Click(object sender, RoutedEventArgs e)
        {
            string result = Weapons.GetAvailableWeapons();
            ShowResult(result);
            
            // Show weapon selection panel if player has weapons
            if (GameState.inventory.Exists(item => Weapons.GetWeaponDamage(item) > 0))
            {
                // Update weapon button labels with actual weapon names
                UpdateWeaponButtonLabels();
                
                WeaponSelectionPanel.Visibility = Visibility.Visible;
                DisableAllButtons();
            }
        }

        private void ArmorButton_Click(object sender, RoutedEventArgs e)
        {
            string result = Shop.GetArmorShopOptions();
            ShowResult(result);
            
            // Show armor selection panel
            ArmorSelectionPanel.Visibility = Visibility.Visible;
            
            // Disable other buttons while selecting armor
            DisableAllButtons();
        }

        private void CheckStatusButton_Click(object sender, RoutedEventArgs e)
        {
            string result = GangSystem.CheckStatus();
            ShowResult(result);
        }

        private void JoinGangButton_Click(object sender, RoutedEventArgs e)
        {
            string result = GangSystem.GetGangOptions();
            ShowResult(result);
            
            // Update gang button labels with actual gang names
            UpdateGangButtonLabels();
            
            // Show gang selection panel
            GangSelectionPanel.Visibility = Visibility.Visible;
            
            // Disable other buttons while selecting gang
            DisableAllButtons();
        }
        
        private void UpdateGangButtonLabels()
        {
            // This would ideally update the button labels with actual gang names
            // For now, we'll just leave them as Gang 1, Gang 2, etc.
            // In a more sophisticated implementation, we'd update these dynamically
        }
        
        private void GangSelection_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && int.TryParse(button.Tag.ToString(), out int choice))
            {
                string result = GangSystem.JoinSpecificGang(choice);
                ShowResult(result);
                
                // Hide gang selection panel
                GangSelectionPanel.Visibility = Visibility.Collapsed;
                
                // Re-enable other buttons
                EnableAllButtons();
            }
        }
        
        private void CancelGangButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide gang selection panel
            GangSelectionPanel.Visibility = Visibility.Collapsed;
            
            // Re-enable other buttons
            EnableAllButtons();
            
            // Show cancellation message
            ShowResult("You decide not to join any gang right now.");
        }

        private void WeaponSelection_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && int.TryParse(button.Tag.ToString(), out int choice))
            {
                // Get available weapons
                List<string> availableWeapons = new List<string>();
                foreach (var item in GameState.inventory)
                {
                    if (Weapons.GetWeaponDamage(item) > 0)
                    {
                        availableWeapons.Add(item);
                    }
                }
                
                // Equip the selected weapon if it exists
                if (choice > 0 && choice <= availableWeapons.Count)
                {
                    string selectedWeapon = availableWeapons[choice - 1];
                    string result = Weapons.EquipWeapon(selectedWeapon);
                    ShowResult(result);
                }
                
                // Hide weapon selection panel
                WeaponSelectionPanel.Visibility = Visibility.Collapsed;
                
                // Re-enable other buttons
                EnableAllButtons();
            }
        }
        
        private void CancelWeaponButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide weapon selection panel
            WeaponSelectionPanel.Visibility = Visibility.Collapsed;
            
            // Re-enable other buttons
            EnableAllButtons();
            
            // Show cancellation message
            ShowResult("You decide not to equip any weapon right now.");
        }

        private void LeaveGangButton_Click(object sender, RoutedEventArgs e)
        {
            currentGang = "None";
            string result = "You leave the gang. The gang boss says: \"You'll regret it!\"";
            ShowResult(result);
        }

        private void BankHeistButton_Click(object sender, RoutedEventArgs e)
        {
            string result = Combat.BankHeist();
            ShowResult(result);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLoad.SaveGame();
            string result = "Game saved successfully!";
            ShowResult(result);
        }

        private void ArmorSelection_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && int.TryParse(button.Tag.ToString(), out int choice))
            {
                string result = Shop.BuyArmor(choice);
                ShowResult(result);
                
                // Hide armor selection panel
                ArmorSelectionPanel.Visibility = Visibility.Collapsed;
                
                // Re-enable other buttons
                EnableAllButtons();
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide armor selection panel
            ArmorSelectionPanel.Visibility = Visibility.Collapsed;
            
            // Re-enable other buttons
            EnableAllButtons();
            
            // Show cancellation message
            ShowResult("You decide not to buy anything and leave the shop.");
        }
        
        private void AttackCrowdButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide crowd choice panel
            CrowdChoicePanel.Visibility = Visibility.Collapsed;
            
            // Engage the crowd
            string result = Combat.FightCrowd(pendingCrowdCivilianType ?? "civilian", pendingCrowdCivilianAlive);
            
            // Show result and re-enable buttons
            ShowResult(result);
            EnableAllButtons();
        }
        
        private void AvoidCrowdButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide crowd choice panel
            CrowdChoicePanel.Visibility = Visibility.Collapsed;
            
            // Show avoidance message
            string result = "You decide to avoid the crowd and move on.\n";
            
            // Show result and re-enable buttons
            ShowResult(result);
            EnableAllButtons();
        }
        
        private void ShowModInfoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowModInfo();
        }
        
        private void CloseModInfoButton_Click(object sender, RoutedEventArgs e)
        {
            HideModInfo();
        }
        
        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Handle ESC key to close mod info panel
            if (e.Key == System.Windows.Input.Key.Escape && ModInfoPanel.Visibility == Visibility.Visible)
            {
                HideModInfo();
                e.Handled = true;
            }
        }
        
        private void ShowModInfo()
        {
            // Build mod information text
            string modInfo = BuildModInfoText();
            
            // Update the mod info text box
            ModInfoTextBox.Text = modInfo;
            
            // Show mod info panel
            ModInfoPanel.Visibility = Visibility.Visible;
            
            // Disable other buttons while viewing mod info
            DisableAllButtons();
            
            // Focus the text box so screen readers can read it
            ModInfoTextBox.Focus();
        }
        
        private void HideModInfo()
        {
            // Hide mod info panel
            ModInfoPanel.Visibility = Visibility.Collapsed;
            
            // Re-enable other buttons
            EnableAllButtons();
        }
        
        private string BuildModInfoText()
        {
            StringBuilder sb = new StringBuilder();
            
            if (ModLoader.LoadedMods.Count > 0)
            {
                sb.AppendLine($"Successfully Loaded Mods ({ModLoader.LoadedMods.Count}):");
                sb.AppendLine(new string('=', 40));
                
                foreach (var mod in ModLoader.LoadedMods)
                {
                    sb.AppendLine($"Name: {mod.ModName}");
                    sb.AppendLine($"Version: {mod.ModVersion}");
                    
                    // Check which methods are overridden by the mod
                    var modType = mod.GetType();
                    var methods = new List<string>();
                    
                    var attackStartMethod = modType.GetMethod("OnAttackStart");
                    if (attackStartMethod != null && attackStartMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnAttackStart");
                    
                    var attackEndMethod = modType.GetMethod("OnAttackEnd");
                    if (attackEndMethod != null && attackEndMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnAttackEnd");
                    
                    var killMethod = modType.GetMethod("OnKill");
                    if (killMethod != null && killMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnKill");
                    
                    var damageTakenMethod = modType.GetMethod("OnDamageTaken");
                    if (damageTakenMethod != null && damageTakenMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnDamageTaken");
                    
                    var cashGainedMethod = modType.GetMethod("OnCashGained");
                    if (cashGainedMethod != null && cashGainedMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnCashGained");
                    
                    var weaponEquippedMethod = modType.GetMethod("OnWeaponEquipped");
                    if (weaponEquippedMethod != null && weaponEquippedMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnWeaponEquipped");
                    
                    var armorEquippedMethod = modType.GetMethod("OnArmorEquipped");
                    if (armorEquippedMethod != null && armorEquippedMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnArmorEquipped");
                    
                    var gangJoinedMethod = modType.GetMethod("OnGangJoined");
                    if (gangJoinedMethod != null && gangJoinedMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnGangJoined");
                    
                    var bankHeistStartMethod = modType.GetMethod("OnBankHeistStart");
                    if (bankHeistStartMethod != null && bankHeistStartMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnBankHeistStart");
                    
                    var gameSavedMethod = modType.GetMethod("OnGameSaved");
                    if (gameSavedMethod != null && gameSavedMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnGameSaved");
                    
                    var gameLoadedMethod = modType.GetMethod("OnGameLoaded");
                    if (gameLoadedMethod != null && gameLoadedMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnGameLoaded");
                    
                    var registerCustomContentMethod = modType.GetMethod("RegisterCustomContent");
                    if (registerCustomContentMethod != null && registerCustomContentMethod.DeclaringType != typeof(IMod))
                        methods.Add("RegisterCustomContent");
                    
                    // Check if OnCivilianDeath is overridden (it has a default implementation)
                    var civilianDeathMethod = modType.GetMethod("OnCivilianDeath");
                    if (civilianDeathMethod != null && civilianDeathMethod.DeclaringType != typeof(IMod))
                        methods.Add("OnCivilianDeath");
                    
                    if (methods.Count > 0)
                    {
                        sb.AppendLine($"Events Handled: {string.Join(", ", methods)}");
                    }
                    
                    sb.AppendLine(new string('-', 30));
                }
            }
            
            if (ModLoader.ModLoadErrors.Count > 0)
            {
                if (ModLoader.LoadedMods.Count > 0)
                {
                    sb.AppendLine();
                }
                
                sb.AppendLine($"Failed to Load Mods ({ModLoader.ModLoadErrors.Count}):");
                sb.AppendLine(new string('=', 40));
                
                foreach (string error in ModLoader.ModLoadErrors)
                {
                    sb.AppendLine(error);
                    sb.AppendLine(new string('-', 30));
                }
            }
            
            if (ModLoader.LoadedMods.Count == 0 && ModLoader.ModLoadErrors.Count == 0)
            {
                sb.AppendLine("No mods are currently loaded.");
                sb.AppendLine();
                sb.AppendLine("To load mods:");
                sb.AppendLine("1. Place .dll mod files in the 'Mods' folder");
                sb.AppendLine("2. Restart the game");
                sb.AppendLine();
                sb.AppendLine("Note: Mods must be compiled against the correct version of the game.");
                sb.AppendLine("If you're seeing this message but expected mods to load,");
                sb.AppendLine("the mods may need to be recompiled or are incompatible.");
            }
            
            sb.AppendLine();
            sb.AppendLine("To reload mods, save your game and restart the application.");
            
            return sb.ToString();
        }
        
        private void DisableAllButtons()
        {
            AttackButton.IsEnabled = false;
            StealButton.IsEnabled = false;
            HealButton.IsEnabled = false;
            EquipButton.IsEnabled = false;
            ArmorButton.IsEnabled = false;
            CheckStatusButton.IsEnabled = false;
            JoinGangButton.IsEnabled = false;
            LeaveGangButton.IsEnabled = false;
            BankHeistButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
            ExitButton.IsEnabled = false;
        }
        
        private void EnableAllButtons()
        {
            AttackButton.IsEnabled = true;
            StealButton.IsEnabled = true;
            HealButton.IsEnabled = true;
            EquipButton.IsEnabled = true;
            ArmorButton.IsEnabled = true;
            CheckStatusButton.IsEnabled = true;
            JoinGangButton.IsEnabled = true;
            LeaveGangButton.IsEnabled = true;
            BankHeistButton.IsEnabled = true;
            SaveButton.IsEnabled = true;
            ExitButton.IsEnabled = true;
        }

        private void ShowResult(string result)
        {
            // Check if crowd choice is needed
            if (result.Contains("[CHOICE_NEEDED:ATTACK_CROWD:"))
            {
                // Extract the civilian type and alive status from the result
                int startIndex = result.IndexOf("[CHOICE_NEEDED:ATTACK_CROWD:") + "[CHOICE_NEEDED:ATTACK_CROWD:".Length;
                int endIndex = result.IndexOf("]", startIndex);
                string civilianInfo = result.Substring(startIndex, endIndex - startIndex);
                string[] parts = civilianInfo.Split(':');
                
                if (parts.Length >= 2)
                {
                    pendingCrowdCivilianType = parts[0];
                    pendingCrowdCivilianAlive = bool.Parse(parts[1]);
                }
                else
                {
                    pendingCrowdCivilianType = "civilian";
                    pendingCrowdCivilianAlive = false;
                }
                
                // Remove the choice indicator from the result
                string cleanResult = result.Substring(0, result.IndexOf("[CHOICE_NEEDED:ATTACK_CROWD:")) + 
                                   result.Substring(endIndex + 1);
                
                // Update the result text block
                ResultTextBlock.Text = cleanResult;
                
                // Show crowd choice panel
                CrowdChoicePanel.Visibility = Visibility.Visible;
                
                // Disable other buttons while making choice
                DisableAllButtons();
                
                // Update status
                UpdateStatus();
                
                // Speak the result (without the choice indicator)
                Speak(cleanResult);
            }
            else
            {
                // Update the result text block
                ResultTextBlock.Text = result;
                
                // Update status
                UpdateStatus();
                
                // Speak the result
                Speak(result);
            }
        }
        
        private void UpdateWeaponButtonLabels()
        {
            // Get available weapons
            List<string> availableWeapons = new List<string>();
            foreach (var item in GameState.inventory)
            {
                if (Weapons.GetWeaponDamage(item) > 0)
                {
                    availableWeapons.Add(item);
                }
            }
            
            // Update weapon button labels with actual weapon names
            if (availableWeapons.Count > 0)
                Weapon1Button.Content = availableWeapons.Count > 0 ? availableWeapons[0] : "Weapon 1";
            if (availableWeapons.Count > 1)
                Weapon2Button.Content = availableWeapons.Count > 1 ? availableWeapons[1] : "Weapon 2";
            if (availableWeapons.Count > 2)
                Weapon3Button.Content = availableWeapons.Count > 2 ? availableWeapons[2] : "Weapon 3";
            if (availableWeapons.Count > 3)
                Weapon4Button.Content = availableWeapons.Count > 3 ? availableWeapons[3] : "Weapon 4";
            if (availableWeapons.Count > 4)
                Weapon5Button.Content = availableWeapons.Count > 4 ? availableWeapons[4] : "Weapon 5";
        }
        
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            string result = "Leaving the streets. Game over.";
            ShowResult(result);
            
            // Clean up SpeechCore
            speechCore?.Dispose();
            
            Close();
        }
    }
}