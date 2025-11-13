<?php
declare(strict_types=1);
header('X-Content-Type-Options: nosniff');
include 'spkdf2.php';
include 'cops.php';
include 'bank.php';
include 'defeat.php';
$cookieName = 'gangsta_save';
$defaultState = [
    'playerHealth' => 100,
    'playerMaxHealth' => 100,
    'cash' => 0,
    'killCount' => 0,
    'inventory' => ['Fists'],
    'equippedWeapon' => 'Fists',
    'equippedArmor' => 'None',
    'boneArmorDurability' => 0,
    'currentGang' => 'None',
    'wantedLevel' => 0
];

$gangs = [
    ['Name'=>'Mini Squad','MemberCount'=>3,'HealthPerMember'=>30,'Required kills'=>3],
    ['Name'=>'Hooligans','MemberCount'=>6,'HealthPerMember'=>40,'Required kills'=>14],
    ['Name'=>'Master Crew','MemberCount'=>10,'HealthPerMember'=>50,'Required kills'=>30],
];

// load saved state from cookie if present
$state = $defaultState;
if (isset($_COOKIE[$cookieName])) {
    $decoded = json_decode((string)$_COOKIE[$cookieName], true);
    if (is_array($decoded)) {
        $state = array_merge($defaultState, array_intersect_key($decoded, $defaultState));
        if (!is_array($state['inventory'])) $state['inventory'] = $defaultState['inventory'];
        $state['playerMaxHealth'] = max(100, 100 + (int)$state['killCount']);
        $state['playerHealth'] = min($state['playerHealth'], $state['playerMaxHealth']);
    } else {
        $state = $defaultState;
    }
}

// helpers
function set_persistent_cookie(string $name, string $json): void {
    @setcookie($name, $json, [
        'expires' => 253402300799, // 9999-12-31 23:59:59 UTC
        'path' => '/',
        'secure' => true,
        'httponly' => true,
        'samesite' => 'Strict'
    ]);
    $cookieHeader = rawurlencode($name) . '=' . rawurlencode($json)
        . '; Expires=Fri, 31 Dec 9999 23:59:59 GMT; Path=/; HttpOnly; SameSite=Strict';
    header('Set-Cookie: ' . $cookieHeader, false);
}

function save_state_cookie(array $state): void {
    global $cookieName;
    $json = json_encode($state);
    set_persistent_cookie($cookieName, $json);
}

function rand_range(int $min, int $max): int { return random_int($min, $max); }

function get_weapon_damage(string $weapon): int {
    $w = strtolower($weapon);
    return match ($w) {
        'fists' => 0,
        'knife' => 5,
        'bat' => 10,
        'pistol' => 20,
        'machinegun' => 35, // machinegun stronger
        default => 0
    };
}

function get_armor_reduction(string $armor, array $state): int {
    $a = strtolower($armor);
    return match ($a) {
        'leather jacket' => 2,
        'bulletproof vest' => 5,
        'riot helmet' => 8,
        'military armor' => 18,
        'bone armor' => (int)$state['boneArmorDurability'],
        'none' => 0,
        default => 0
    };
}

function apply_player_damage(int $damage, array &$state, array &$events, string $attacker = ''): void {
    if ($damage <= 0) {
        return;
    }
    $reduction = get_armor_reduction($state['equippedArmor'], $state);
    $absorbed = min($damage, $reduction);
    $effective = max(0, $damage - $absorbed);
    $armor_lower = strtolower($state['equippedArmor']);
    $from_attacker = $attacker ? " from the $attacker" : '';
    if ($armor_lower === 'bone armor') {
        $state['boneArmorDurability'] -= $absorbed;
        if ($state['boneArmorDurability'] <= 0) {
            $state['boneArmorDurability'] = 0;
            $events[] = "Your Bone Armor shatters! You take $effective damage$from_attacker. Your health: " . ($state['playerHealth'] - $effective) . ".";
            $state['inventory'] = array_values(array_filter($state['inventory'], fn($it) => strtolower($it) !== 'bone armor'));
            $state['inventory'][] = 'Broken Bone Armor';
            $state['equippedArmor'] = 'None';
        } else {
            $events[] = "Bone Armor absorbed the damage$from_attacker! Durability is now {$state['boneArmorDurability']}. Your health: {$state['playerHealth']}.";
        }
    } else {
        $events[] = "You take $effective damage$from_attacker. Your health: " . ($state['playerHealth'] - $effective) . ".";
    }
    $state['playerHealth'] -= $effective;
    if ($state['playerHealth'] < 0) {
        $state['playerHealth'] = 0;
    }
}
// AJAX endpoint
$rawInput = file_get_contents('php://input');
if ($_SERVER['REQUEST_METHOD'] === 'POST' && !empty($rawInput)) {
    $req = json_decode($rawInput, true);
    if (!is_array($req)) {
        http_response_code(400);
        echo json_encode(['ok'=>false,'errors'=>['Invalid request']]);
        exit;
    }
    $action = $req['action'] ?? '';
    $events = [];
    $ok = true;

    // reload server state from cookie (ensures consistency)
    $state = $defaultState;
    if (isset($_COOKIE[$cookieName])) {
        $decoded = json_decode((string)$_COOKIE[$cookieName], true);
        if (is_array($decoded)) {
            $state = array_merge($defaultState, array_intersect_key($decoded, $defaultState));
            if (!is_array($state['inventory'])) $state['inventory'] = $defaultState['inventory'];
            $state['playerMaxHealth'] = max(100, 100 + (int)$state['killCount']);
            $state['playerHealth'] = min($state['playerHealth'], $state['playerMaxHealth']);
        }
    }

    switch ($action) {
        case 'noop':
            // return state for initial load / health check
            $events[] = "Idle.";
            break;

        case 'attack':
            $civilians = ['shopkeeper','old lady','teenager','businessman','jogger','old man'];
            $screams = ["Oh no, please don't hurt me!","Somebody help me!","What's wrong with you?!","No, no, no, stay away!","I've got a family, please!","I'm begging you!", "fuck! fuck! fuck!","please, go away!"];
            $target = $civilians[array_rand($civilians)];
            $targetHealth = rand_range(30,50);
            $playerDamage = rand_range(10,20) + get_weapon_damage($state['equippedWeapon']);
            $events[] = "You spot a $target. You attack with your {$state['equippedWeapon']}!";
            $fled = false;
            while ($targetHealth > 0 && $state['playerHealth'] > 0 && !$fled) {
                $targetHealth -= $playerDamage;
                $events[] = "You hit the $target for $playerDamage damage. Their health: " . max(0,$targetHealth) . ".";
                $events[] = "The $target screams: \"".$screams[array_rand($screams)]."\"";
                if ($targetHealth <= 0) {
                    $loot = rand_range(5,30);
                    $state['cash'] += $loot;
                    $state['killCount'] += 1;
                    $state['playerHealth'] = min($state['playerMaxHealth'], $state['playerHealth'] + 1);
                    $state['wantedLevel'] += 1;
                    $events[] = "The $target collapses. You loot \$$loot. Total cash: \${$state['cash']}. Kill count: {$state['killCount']}.";
                    if (rand_range(1,100) <= 50) $events[] = "The $target spits out blood.";
                    call_cops($state,$events);
                    break;
                }
                $damage = rand_range(1,9);
                $events[] = "The $target fights back!";
                apply_player_damage($damage, $state, $events, $target);
                if ($state['playerHealth'] < 20) {
                    if (rand_range(1,100) <= 50) {
                        $events[] = "You escape the fight!";
                        $fled = true;
                        break;
                    } else {
                        $events[] = "You try to run but you're hurt and fail to flee!";
                    }
                }
                if($state["playerHealth"]<=0)
                {
                    $lost = intdiv($state['cash'], 2);
                    $state['cash'] -= $lost;
                    $events[] = "You were defeated by someone, they called the fucking cops! They cuff you and take \$$lost (50% of your cash). You're arrested briefly.";
                    $state['wantedLevel'] = max(0, $state['wantedLevel'] - 2);
                    $state['playerHealth'] = max(1, intval($state['playerMaxHealth'] * 0.25));
                }
            }
            break;

        case 'steal':
            $pool = ['Knife','Pistol','Bat'];
            $pick = $pool[array_rand($pool)];
            $roll = rand_range(1,100);
            $events[] = "You attempt to steal a weapon...";
            if ($roll > 30) {
                if (!in_array($pick, $state['inventory'])) {
                    $state['inventory'][] = $pick;
                    $events[] = "You stole a $pick.";
                    $state['wantedLevel'] += 1;
                } else {
                    $events[] = "You already have that weapon. you leave it.";
                }
            } else {
                $events[] = "Busted! The seller fights back!";
                $damage = rand_range(10,24);
                apply_player_damage($damage, $state, $events, 'seller');
            }
            break;

        case 'heal':
            $price = 20;
            $events[] = "You visit the doctor, and he scowls at you!";
            if ($state['cash'] >= $price) {
                if ($state['playerHealth'] >= $state['playerMaxHealth']) {
                    $events[] = "Doc says: You're fine, get lost, punk! don't waste my time!";
                } else {
                    $state['cash'] -= $price;
                    $state['playerHealth'] = $state['playerMaxHealth'];
                    $events[] = "You paid \$$price, and the doctor heals you and says: you got healed, now get fucking lost: punk!";
                }
            } else {
                $events[] = "the doctor scowls and says: you have no money, punk! fuck off!";
            }
            break;
        case 'equip':
            $weapon = $req['weapon'] ?? '';
            if (is_string($weapon) && in_array($weapon, $state['inventory'])) {
                $state['equippedWeapon'] = $weapon;
                $events[] = "You equip the $weapon.";
            } else {
                $events[] = "Invalid weapon choice or not in inventory.";
            }
            break;
        case 'equip_armor':
            $armor = $req['armor'] ?? '';
            if (is_string($armor) && in_array($armor, $state['inventory'])) {
                $state['equippedArmor'] = $armor;
                $events[] = "You equip the $armor.";
            } else {
                $events[] = "Invalid armor choice or not in inventory.";
            }
            break;
        case 'armor_shop':
          $events[] ="hey stevo! that's me! rick! you want some stuff?";
          $armors = ['Leather Jacket','Bulletproof Vest','Riot Helmet','Military Armor','Bone Armor'];
            $prices = [30,60,90,200, 5000];
            $choiceIdx = isset($req['choice']) ? intval($req['choice']) - 1 : -1;
            if ($choiceIdx >= 0 && $choiceIdx < count($armors)) {
                $armor = $armors[$choiceIdx];
                $price = $prices[$choiceIdx];
                if ($state['cash'] >= $price) {
                    if (in_array($armor, $state['inventory'])) {
                        $events[] = " Steve: You already own the $armor, bruh!";
                    } else {
                        $state['cash'] -= $price;
                        $state['inventory'][] = $armor;
                        $state['equippedArmor'] = $armor;
                        if (strtolower($armor) === 'bone armor') {
                            $state['boneArmorDurability'] = max(1, (int)$state['killCount']);
                            $events[] = "You buy Bone Armor. Initial durability: {$state['boneArmorDurability']}, Rick says: good pick bro!";
                        }
                        $events[] = "You equip the $armor. Cash left: \${$state['cash']}. Rick says: it fits on you! you reply: fuck yeah!";
                    }
                } else {
                    $events[] = "Rick says: sorry bro, you can't afford $armor right now, and I ein't gonna give it out for free.";
                }
            } else {
                $events[] = "Rick says: bro, this armor doesn't exist!";
            }
            break;

        case 'repair_bone':
            $hasBroken = in_array('Broken Bone Armor', $state['inventory']);
            if ($hasBroken) {
                $cost = max(0, (int)$state['killCount']);
                if ($state['cash'] >= $cost) {
                    $state['cash'] -= $cost;
                    $state['inventory'] = array_values(array_filter($state['inventory'], fn($it)=> $it !== 'Broken Bone Armor'));
                    $state['inventory'][] = 'Bone Armor';
                    $state['equippedArmor'] = 'Bone Armor';
                    $state['boneArmorDurability'] = max(1, (int)$state['killCount']);
                    $events[] = "You repaired Bone Armor for \$$cost. Durability: {$state['boneArmorDurability']}. Rick says: as good as new! you say: thanks!";
                } else {  
                  $events[] = "Rick says:bro, you don't have \$$cost to fix it, sorry.";
                }
            } else {
              $events[] = "well, your armor is in good state, it didn't break yet, Rick says";
            }
            break;

        case 'join_gang':
            $choice = isset($req['whichgang']) ? intval($req['whichgang']) - 1 : -1;
            if ($choice >= 0 && $choice < count($gangs)) {
                $g = $gangs[$choice];
                if ($state['killCount'] < $g['Required kills']) {
                    $events[] = "Boss says: You need {$g['Required kills']} kills to join {$g['Name']}.";
                } else {
                    $state['currentGang'] = $g['Name'];
                    $events[] = "You join {$g['Name']}.";
                }
            } else {
                $events[] = "Invalid gang choice.";
            }
            break;

        case 'leave_gang':
            $state['currentGang'] = 'None';
            $events[] = "You leave the gang. Boss says: You'll regret it!";
            break;

        case 'attack_bank':
          bank_attack($state,$events);
            break;

        case 'save':
            $events[] = "Game saved..";
            break;

        case 'reset':
            set_persistent_cookie($cookieName, json_encode($defaultState));
            $state = $defaultState;
            $events[] = "Save cleared; fresh start.";
            break;

        case 'download_export':
            $payload = json_encode($state);
$blob=pkdf2_encrypt($payload,"Patricusmeow");
            header('Content-Type: application/octet-stream');
header('Content-Disposition: attachment; filename="gangsta_save.gsave"');
            echo $blob;
            exit;
        case 'upload_import':
            $blob = $req['import_blob'] ?? '';
            if (!is_string($blob) || $blob === '') { $ok=false; $events[] = "Event: Import requires a file blob."; break; }
            $plain = pkdf2_decrypt($blob,"Patricusmeow");
            if ($plain === null) { $ok=false; $events[] = "Event: Decryption failed — corrupted file or mismatched build key."; break; }
            $data = json_decode($plain, true);
            if (!is_array($data)) { $ok=false; $events[] = "Event: Decrypted content invalid."; break; }
            $state = array_merge($defaultState, array_intersect_key($data, $defaultState));
            if (!is_array($state['inventory'])) $state['inventory'] = $defaultState['inventory'];
            $state['playerMaxHealth'] = max(100, 100 + (int)$state['killCount']);
            $state['playerHealth'] = min($state['playerHealth'], $state['playerMaxHealth']);
            $events[] = "Event: Save imported successfully (using build key).";
            break;

        case 'restore_from_local':
            $localjson = $req['local_json'] ?? '';
            if (!is_string($localjson) || $localjson === '') { $ok=false; $events[] = "Event: No local JSON provided."; break; }
            $data = json_decode($localjson, true);
            if (!is_array($data)) { $ok=false; $events[] = "Event: Local JSON invalid."; break; }
            $state = array_merge($defaultState, array_intersect_key($data, $defaultState));
            if (!is_array($state['inventory'])) $state['inventory'] = $defaultState['inventory'];
            $state['playerMaxHealth'] = max(100, 100 + (int)$state['killCount']);
            $state['playerHealth'] = min($state['playerHealth'], $state['playerMaxHealth']);
            $events[] = "game loaded successfully!";
            break;

        default:
            $events[] = "Event: Unknown action.";
            $ok = false;
            break;
    }

    // clamp and save
    $state['playerMaxHealth'] = max(100, 100 + (int)$state['killCount']);
    if ($state['playerHealth'] > $state['playerMaxHealth']) $state['playerHealth'] = $state['playerMaxHealth'];

    save_state_cookie($state);

    header('Content-Type: application/json; charset=utf-8');
    echo json_encode(['ok'=>$ok,'state'=>$state,'events'=>$events]);
    exit;
}

// Render UI
function h($s){ return htmlspecialchars((string)$s, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8'); }
?>
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<title>Gangsta sim(2.2.0)</title>
<link rel="stylesheet" href="gangsta_style.css">
<meta name="viewport" content="width=device-width,initial-scale=1">
</head>
<body>
<section class="status">
  <h2 id="status-title">Player Status</h2>
  <span class="stat" id="health">Health: <?=h($state['playerHealth'])?>/<?=h($state['playerMaxHealth'])?></span><br>
  <span class="stat" id="cash">Cash: $<?=h($state['cash'])?></span><br>
  <span class="stat" id="weapon">Equipped Weapon: <?=h($state['equippedWeapon'])?></span><br>
  <span class="stat" id="armor">Equipped Armor: <?=h($state['equippedArmor'])?>
    <?php if (strtolower($state['equippedArmor']) === 'bone armor'): ?>
      (durability: <?=h($state['boneArmorDurability'])?>)
    <?php endif; ?>
  </span><br>
  <span class="stat" id="kills">Kill Count: <?=h($state['killCount'])?></span><br>
  <span class="stat" id="wanted">Wanted Level: <?=h($state['wantedLevel'])?></span><br>
  <span class="stat" id="gang">Gang: <?=h($state['currentGang'])?></span><br>
  <span class="stat" id="inv">Inventory: <?= h(implode(', ', $state['inventory'])) ?></span>
</section>
<section class="messages" aria-live="assertive" role="status" id="events">
<h2>event log</h2>
<div id="eventList"><p>Nothing yet. Hit Attack or Steal to start comiting crimes!</p></div>
  </section>
  <nav class="actions" aria-label="Game actions" id="actionRow">
    <button class="primary" onclick="doAction('attack')">Attack</button>
    <button onclick="doAction('steal')">Steal</button>
    <button onclick="doAction('heal')">Heal ($20)</button>
    <button onclick="doAction('attack_bank')" class="warn">Attack Bank</button>
    <button onclick="doAction('save')">Save</button>
    <button onclick="doAction('reset')">Reset Save</button>
  </nav>

  <div style="display:flex;gap:1rem;flex-wrap:wrap;margin-bottom:1rem">
    <div style="min-width:260px">
      <fieldset>
        <legend class="small">Equip Weapon</legend>
        <label for="weaponSelect">Choose weapon</label>
        <select id="weaponSelect" aria-label="Weapon select">
          <?php foreach ($state['inventory'] as $it): ?>
            <?php if (get_weapon_damage($it) > 0 || $it === 'Fists'): ?>
              <option value="<?=h($it)?>"><?=h($it)?> (Damage: <?=h(get_weapon_damage($it))?>)</option>
            <?php endif; ?>
          <?php endforeach; ?>
        </select>
        <div style="margin-top:.4rem"><button onclick="doAction('equip', {weapon: document.getElementById('weaponSelect').value })">Equip</button></div>
      </fieldset>
    </div>

    <div style="min-width:260px">
      <fieldset>
        <legend class="small">Equip Armor</legend>
        <label for="armorSelectEquip">Choose armor</label>
        <select id="armorSelectEquip" aria-label="Armor equip select">
          <?php foreach ($state['inventory'] as $it): ?>
            <?php if (get_armor_reduction($it, $state) > 0 || strtolower($it) === 'bone armor'): ?>
              <option value="<?=h($it)?>"><?=h($it)?> (Reduction: <?=h(get_armor_reduction($it, $state))?>)</option>
            <?php endif; ?>
          <?php endforeach; ?>
          <option value="None">None (Reduction: 0)</option>
        </select>
        <div style="margin-top:.4rem"><button onclick="doAction('equip_armor', {armor: document.getElementById('armorSelectEquip').value })">Equip</button></div>
      </fieldset>
    </div>

    <div style="min-width:260px">
      <fieldset>
        <legend class="small">Armor Shop</legend>
        <label for="armorSelect">Pick armor</label>
        <select id="armorSelect" aria-label="Armor select">
          <option value="1">Leather Jacket - $30 (reduces 2)</option>
          <option value="2">Bulletproof Vest - $60 (reduces 5)</option>
          <option value="3">Riot Helmet - $90 (reduces 8)</option>
          <option value="4">Military Armor - $200 (reduces 18)</option>
          <option value="5">Bone Armor - $5000 (durability = kills)</option>
        </select>
        <div style="margin-top:.4rem"><button onclick="doAction('armor_shop', {choice: document.getElementById('armorSelect').value })">Buy</button></div>
      </fieldset>
    </div>

    <div style="min-width:260px">
      <fieldset>
        <legend class="small">Bone Armor Repair</legend>
        <p class="small">If you have <strong>Broken Bone Armor</strong>, repair cost = current Kill Count dollars (<span id="repairCost"><?=h($state['killCount'])?></span>).</p>
        <div style="margin-top:.4rem"><button onclick="doAction('repair_bone')">Repair Broken Bone Armor</button></div>
      </fieldset>
    </div>
  </div>

  <section style="margin-bottom:1rem">
    <fieldset>
      <legend class="small">Gangs</legend>
      <label for="gangSelect">Choose gang</label>
      <select id="gangSelect">
        <?php foreach ($gangs as $i => $g): ?>
          <option value="<?=($i+1)?>"><?=h($g['Name'])?> - <?=h($g['MemberCount'])?> members (Requires <?=h($g['Required kills'])?> kills)</option>
        <?php endforeach; ?>
      </select>
      <div style="margin-top:.4rem">
        <button onclick="doAction('join_gang', {whichgang: document.getElementById('gangSelect').value})">Join</button>
        <button onclick="doAction('leave_gang')">Leave Gang</button>
      </div>
    </fieldset>
  </section>

  <section>
    <fieldset>
      <legend class="small">Export / Import</legend>
      <div style="display:flex;gap:.6rem;flex-wrap:wrap;margin-bottom:.6rem">
        <div>
          <div style="margin-top:.4rem"><button onclick="exportSave()">export your save)</button></div>
        </div>

        <div>
          <label for="impFile">Import save</label>
          <input type="file" id="impFile" aria-label="Import save">
          <div style="margin-top:.4rem"><button onclick="importSave()">Upload & Import)</button></div>
      </div>

<script>
const endpoint = location.href;

// AJAX actions
async function doAction(action, extra={}) {
  const payload = Object.assign({action}, extra);
  const res = await fetch(endpoint, {
    method: 'POST',
    credentials: 'same-origin',
    headers: {'Content-Type':'application/json'},
    body: JSON.stringify(payload)
  });
  if (!res.ok) {
    renderEvents(['Event: Server error! oh fuck! ' + res.status]);
    return;
  }
  const data = await res.json();
  if (!data.ok) {
    renderEvents(data.events || ['Event: Crash happened, bork!']);
    return;
  }
  if (data.state) {
    updateUI(data.state);
    try {
      localStorage.setItem('gangsta_save_local', JSON.stringify(data.state));
      localStorage.setItem('gangsta_save_local_ts', Date.now().toString());
    } catch(e) { console.warn('localStorage save failed', e); }
  }
  renderEvents(data.events || []);
}

function updateUI(s) {
  document.getElementById('health').textContent = `Health: ${s.playerHealth}/${s.playerMaxHealth}`;
  document.getElementById('cash').textContent = `Cash: $${s.cash}`;
  document.getElementById('weapon').textContent = `Equipped Weapon: ${s.equippedWeapon}`;
  const armorEl = document.getElementById('armor');
  if (armorEl) {
    armorEl.textContent = `Equipped Armor: ${s.equippedArmor}`;
    if (s.equippedArmor.toLowerCase() === 'bone armor') {
      armorEl.textContent += ` (durability: ${s.boneArmorDurability ?? 0})`;
    }
  }
  document.getElementById('kills').textContent = `Kill Count: ${s.killCount}`;
  document.getElementById('wanted').textContent = `Wanted Level: ${s.wantedLevel}`;
  document.getElementById('gang').textContent = `Gang: ${s.currentGang}`;
  document.getElementById('inv').textContent = `Inventory: ${(s.inventory || []).join(', ')}`;
  document.getElementById('repairCost').textContent = s.killCount;

  // update weapon select
  const ws = document.getElementById('weaponSelect');
  ws.innerHTML = '';
  for (const it of (s.inventory || [])) {
    if (getWeaponDamage(it) > 0 || it === 'Fists') {
      const o = document.createElement('option');
      o.value = it;
      o.textContent = `${it} (Damage: ${getWeaponDamage(it)})`;
      ws.appendChild(o);
    }
  }

  // update armor select


  // update armor select
  const as = document.getElementById('armorSelectEquip');
  as.innerHTML = '<option value="None">None (Reduction: 0)</option>';
  for (const it of (s.inventory || [])) {
    const reduction = getArmorReduction(it, s);
    if (reduction > 0 || it.toLowerCase() === 'bone armor') {
      const o = document.createElement('option');
      o.value = it; o.textContent = `${it} (Reduction: ${reduction})`; as.appendChild(o);
    }
  }
}

function getWeaponDamage(w) {
  switch ((w||'').toLowerCase()) {
    case 'fists': return 0;
    case 'knife': return 5;
    case 'bat': return 10;
    case 'pistol': return 20;
    case 'machinegun': return 35;
    default: return 0;
  }
}

function getArmorReduction(a, s) {
  switch ((a||'').toLowerCase()) {
    case 'leather jacket': return 2;
    case 'bulletproof vest': return 5;
    case 'riot helmet': return 8;
    case 'military armor': return 18;
    case 'bone armor': return s.boneArmorDurability ?? 0;
    default: return 0;
  }
}

function renderEvents(events) {
  const container = document.getElementById('eventList');
  container.innerHTML = '';
  if (!events || events.length === 0) {
    const p = document.createElement('p'); p.textContent = 'Nothing happened.'; container.appendChild(p); return;
  }
  for (const e of events) {
    const p = document.createElement('p'); p.textContent = e; container.appendChild(p);
  }
  container.parentElement.scrollIntoView({behavior:'smooth', block:'center'});
}

// Export (no password, uses build key on server)
async function exportSave() {
  const res = await fetch(endpoint, {
    method: 'POST',
    credentials: 'same-origin',
    headers: {'Content-Type':'application/json'},
    body: JSON.stringify({action:'download_export'})
  });
  if (!res.ok) { renderEvents(['Event: Export failed: ' + res.status]); return; }
  const blobText = await res.text();
  // download
  const raw = atob(blobText);
  const arr = new Uint8Array(raw.length);
  for (let i=0;i<raw.length;i++) arr[i]=raw.charCodeAt(i);
  const blob = new Blob([arr], {type:'application/octet-stream'});
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url; a.download = 'gangsta_save.gsave'; document.body.appendChild(a);
  a.click(); a.remove(); URL.revokeObjectURL(url);
  renderEvents(['Event: Export complete — save downloaded (encrypted with build key). Keep it safe.']);
}

async function importSave() {
  const fileInput = document.getElementById('impFile');
  if (!fileInput.files.length) { alert('Choose a file to import'); return; }
  const f = fileInput.files[0];
  const arr = await f.arrayBuffer();
  const bytes = new Uint8Array(arr);
  let binary = '';
  for (let i=0;i<bytes.length;i++) binary += String.fromCharCode(bytes[i]);
  const b64 = btoa(binary);
  const res = await fetch(endpoint, {
    method: 'POST',
    credentials: 'same-origin',
    headers: {'Content-Type':'application/json'},
    body: JSON.stringify({action:'upload_import', import_blob: b64})
  });
  if (!res.ok) { renderEvents(['Event: Import failed: ' + res.status]); return; }
  const data = await res.json();
  if (!data.ok) { renderEvents(data.events || ['Event: Import failed']); return; }
  if (data.state) updateUI(data.state);
  renderEvents(data.events || ['Event: Imported successfully (using build key).']);
  try {
    localStorage.setItem('gangsta_save_local', JSON.stringify(data.state));
    localStorage.setItem('gangsta_save_local_ts', Date.now().toString());
  } catch(e){}
}

// Try restore local backup if server looks default
async function tryRestoreFromLocalIfCookieMissing() {
  try {
    const resp = await fetch(endpoint, {
      method:'POST', credentials:'same-origin',
      headers:{'Content-Type':'application/json'},
      body: JSON.stringify({action:'noop'})
    });
    if (!resp.ok) return;
    const data = await resp.json();
    if (!data.ok) return;
    const local = localStorage.getItem('gangsta_save_local');
    if (!local) return;
    const serverState = data.state || {};
    const serverIsDefault = (serverState.playerHealth === 100 && serverState.killCount === 0 && serverState.cash === 0);
    if (serverIsDefault) {
      if (confirm('A local backup exists in this browser. Restore it to the server cookie?')) {
        const res = await fetch(endpoint, {
          method:'POST', credentials:'same-origin',
          headers:{'Content-Type':'application/json'},
          body: JSON.stringify({action:'restore_from_local', local_json: local})
        });
        const j = await res.json();
        if (j.ok && j.state) updateUI(j.state);
        renderEvents(j.events || ['Event: Local backup restored.']);
      }
    }
  } catch(e){ console.warn('restore check failed', e); }
}

// Init: get server state
(async function init(){
  try {
    const res = await fetch(endpoint, {
      method:'POST', credentials:'same-origin',
      headers:{'Content-Type':'application/json'},
      body: JSON.stringify({action:'noop'})
    });
    const data = await res.json();
    if (data.ok && data.state) updateUI(data.state);
  } catch(e){}
  tryRestoreFromLocalIfCookieMissing();
})();

// Expose for console
window.doAction = doAction;
window.exportSave = exportSave;
window.importSave = importSave;
</script>
</body>
</html>