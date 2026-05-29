$fsoBotsDir = "C:\Dev\FSO-NorvinskSection1\FSO.NorvinskSection1.Server\db\bots\types"
$fsoBots = @("fsofixerrookie", "fsofixeroperative", "fsofixerspecialist", "fsofixerlead", "fsofixerinnercircle")

$pmcChances = @{
    equipment = @{
        Headwear = 86; Earpiece = 54; FaceCover = 67; ArmorVest = 18; Eyewear = 86
        ArmBand = 0; TacticalVest = 100; Backpack = 47; FirstPrimaryWeapon = 100
        SecondPrimaryWeapon = 0; Holster = 100; Scabbard = 0; Pockets = 100; SecuredContainer = 100
    }
    weaponMods = @{
        mod_magazine = 100; mod_sight_front = 74; mod_sight_rear = 77; mod_scope = 82
        mod_tactical_000 = 11; mod_stock = 44; mod_mount = 20; mod_mount_001 = 36
        mod_mount_002 = 27; mod_muzzle = 47; mod_foregrip = 70; mod_pistol_grip = 0
        mod_tactical = 20; mod_mount_000 = 36; mod_stock_000 = 99; mod_bipod = 0
        mod_charge_001 = 8; mod_launcher = 0; mod_tactical_2 = 33; mod_tactical_003 = 28
        mod_handguard = 100; mod_tactical_001 = 39; mod_flashlight = 100; mod_reciever = 100
        mod_charge = 19; mod_mount_003 = 2; mod_stock_002 = 100; mod_mount_004 = 0
        mod_tactical_002 = 0; mod_muzzle_000 = 81; mod_muzzle_001 = 32
        mod_scope_000 = 100; mod_scope_001 = 0; mod_scope_002 = 0
    }
    equipmentMods = @{
        front_plate = 100; back_plate = 100; left_side_plate = 9; right_side_plate = 9
        mod_equipment_000 = 2; mod_nvg = 4; mod_equipment_001 = 4; mod_mount = 0; mod_equipment_002 = 0
    }
}

$pmcGeneration = @{
    items = @{
        specialItems = @{ weights = @{ "0" = 1; "1" = 0 }; whitelist = @() }
        healing = @{ weights = @{ "0" = 1; "1" = 2; "2" = 1 }; whitelist = @() }
        drugs = @{ weights = @{ "0" = 1; "1" = 2; "2" = 0 }; whitelist = @() }
        stims = @{ weights = @{ "0" = 2; "1" = 1; "2" = 0 }; whitelist = @() }
        food = @{ weights = @{ "0" = 10; "1" = 5; "2" = 2 }; whitelist = @() }
        drink = @{ weights = @{ "0" = 10; "1" = 5; "2" = 2 }; whitelist = @() }
        currency = @{ weights = @{ "0" = 20; "1" = 5; "2" = 1 }; whitelist = @() }
        backpackLoot = @{ weights = @{ "0" = 1; "1" = 1; "2" = 2; "3" = 1; "4" = 1; "5" = 1; "6" = 1; "7" = 0 }; whitelist = @() }
        pocketLoot = @{ weights = @{ "0" = 1; "1" = 6; "2" = 3; "3" = 1; "4" = 1 }; whitelist = @() }
        vestLoot = @{ weights = @{ "0" = 1; "1" = 1; "2" = 2; "3" = 1; "4" = 0; "5" = 0; "6" = 0 }; whitelist = @() }
        magazines = @{ weights = @{ "0" = 0; "1" = 0; "2" = 1; "3" = 3; "4" = 1 }; whitelist = @() }
        grenades = @{ weights = @{ "0" = 1; "1" = 2; "2" = 1; "3" = 1; "4" = 0; "5" = 0 }; whitelist = @() }
    }
}

foreach ($botName in $fsoBots) {
    $botPath = Join-Path $fsoBotsDir "$botName.json"
    if (-not (Test-Path $botPath)) {
        Write-Host "  NOT FOUND: $botPath" -ForegroundColor Red
        continue
    }
    $bot = Get-Content $botPath -Raw | ConvertFrom-Json
    $bot.chances = $pmcChances
    $bot.generation = $pmcGeneration
    $bot | ConvertTo-Json -Depth 10 | Set-Content $botPath -Encoding UTF8
    Write-Host "  Patched: $botName" -ForegroundColor Green
}

Write-Host "`n=== Patching complete ===" -ForegroundColor Cyan

foreach ($botName in $fsoBots) {
    $bot = Get-Content "$fsoBotsDir\$botName.json" -Raw | ConvertFrom-Json
    $chancesCount = ($bot.chances.weaponMods.PSObject.Properties).Count
    $genCount = ($bot.generation.items.PSObject.Properties).Count
    Write-Host "  $botName : weaponMods chances = $chancesCount, generation items = $genCount" -ForegroundColor Cyan
}
