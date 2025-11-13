<?php
function call_cops(array &$state, array &$events): bool {
    $cop_taunts=[
"hands up!",
"we're going to take you down!",
"get down!",
"it's police!"
    ];
    $chance=rand_range(1,2);
    if($chance==1&&$state['wantedLevel']<5)
    {
        return false;
    }
$cops=(int)($state['wantedLevel']/4);
    $events[] = "$cops cops show up!";
        if($cops>30)
        {
            $cops=30;
        }
    $copsHealth = 40 * $cops;
    $playerD = rand_range(10, 20) + get_weapon_damage($state['equippedWeapon']);
    while ($copsHealth > 0 && $state['playerHealth'] > 0) {
        $copsHealth -= $playerD;
        $events[] = "You hit the police for $playerD. Police health: " . max(0, $copsHealth) . ".";
        $taunt=array_rand($cop_taunts);
        $events[]="a cop shouts: $cop_taunts[$taunt]";
        if($copsHealth <= 0) {
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
   $taunt=array_rand($cop_taunts);
        $events[]="a cop shouts: $cop_taunts[$taunt]";
        if ($state['playerHealth'] <= 0) {
defeat("cops",$state,$events);
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