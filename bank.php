            function bank_attack($state,$events)
		{
			if ($state['currentGang'] === 'None') {
                $events[] = "You need a gang to pull off a bank heist.";
            } else {
                $g = null;
                foreach ($gangs as $gg) if ($gg['Name'] === $state['currentGang']) { $g = $gg; return; }
                if ($g === null) { $events[] = "Your gang data is missing!"; return; }
                $events[] = "You and the {$state['currentGang']} storm a bank!";
                $security = 400;
                $crewDamage = $g['MemberCount'] * 5;
                while ($security > 0 && $state['playerHealth'] > 0) {
                    $playerHit = rand_range(15,25) + get_weapon_damage($state['equippedWeapon']);
                    $totalHit = $playerHit + $crewDamage;
                    $security -= $totalHit;
                    $events[] = "You hit $playerHit and crew adds $crewDamage. Security health: ".max(0,$security).".";
                    if ($security <= 0) {
                        $haul = rand_range(1000,2000) + $g['MemberCount'] * 100;
                        $share = intdiv($haul, ($g['MemberCount'] + 1));
                        $state['cash'] += $share;
                        $state['killCount'] += 4;
                        $state['wantedLevel'] += 3;
                        $events[] = "Heist success! Your share: \$$share! fuck yeah! Total cash: \${$state['cash']}. Kills +4. Wanted +3.";
                        call_cops($state,$events);
                        break;
                    }
                    $damage = rand_range(20,34);
                    $events[] = "Security hits you!";
                    apply_player_damage($damage, $state, $events, 'security');
                    if (rand_range(1,100) <= 20) {
                        $events[] =  "A crew member goes down!";
                        $crewDamage = max(5, $crewDamage - 5);
                    }
                    if ($state['playerHealth'] < 20 && rand_range(1,100) <= 50) {
                        $events[] ="You and crew bail empty-handed.";
                        return;
                    }
                }
            }
		}