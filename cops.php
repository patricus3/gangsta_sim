<?php
function call_cops(array &$state, array &$events): bool {
    $wanted = (int)$state['wantedLevel'];
    if ($wanted < 1 || rand_range(0, 4) >= $wanted) {
        return false;
    }
    $events[] = "cops show up!";
    $cops = max(1, min(3, intval($wanted / 2)));
    $copsHealth = 40 * $cops;
    $playerD = rand_range(10, 20) + get_weapon_damage($state['equippedWeapon']);
    while ($copsHealth > 0 && $state['playerHealth'] > 0) {
        $copsHealth -= $playerD;
        $events[] = "You hit the police for $playerD. Police health: " . max(0, $copsHealth) . ".";
        if ($copsHealth <= 0) {
            $loot2 = rand_range(20, 50);
            $state['cash'] += $loot2;
            $state['killCount'] += $cops;
            $events[] = "The cops go down. You snag \$$loot2. Total cash: \${$state['cash']}. Kills +$cops. fuck yeah!";
            if (rand_range(0, 100) > 69) {
                $inv_lower = array_map('strtolower', $state['inventory']);
                if (!in_array('machinegun', $inv_lower)) {
                    $state['inventory'][] = 'Machinegun';
                    $events[] = "One cop had a machinegun! You pick it up! hurrah!";
                } else {
                    $bonus = rand_range(10, 50);
                    $state['cash'] += $bonus;
                    $events[] = "You already had a machinegun; you salvage parts and score \$$bonus.";
                }
            }
            return true;
        }
        $damage = rand_range(5, 15) * $cops;
        $events[] = "Police fire back!";
        apply_player_damage($damage, $state, $events, 'police');
        if ($state['playerHealth'] <= 0) {
            // defeated by police => lose 50% cash, arrested briefly
            $lost = intdiv($state['cash'], 2);
            $state['cash'] -= $lost;
            $events[] = "You were defeated by the fucking cops! They cuff you and take \$$lost (50% of your cash). You're arrested briefly.";
            $state['wantedLevel'] = max(0, $state['wantedLevel'] - 2);
            $state['playerHealth'] = max(1, intval($state['playerMaxHealth'] * 0.25));
            return true;
        }
        if ($state['playerHealth'] < 20 && rand_range(1, 100) <= 50) {
            $events[] = "You fled the cops! barely escaping! oh fuck!";
            return true;
        }
    }
    return true;
}
?>