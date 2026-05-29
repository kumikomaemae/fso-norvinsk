$fsoBotsDir = "C:\Dev\FSO-NorvinskSection1\FSO.NorvinskSection1.Server\db\bots\types"

# Per-tier chance overrides — based on what pools we actually populated
$tierOverrides = @{
    "fsofixerrookie" = @{
        Headwear = 0          # No headwear pool; head is in appearance.head
        ArmorVest = 100       # Has Slick
        FaceCover = 100       # The mask defines FSO
        Earpiece = 100        # Proflex earbuds on every fixer
        Backpack = 100        # Each tier has assigned backpack
    }
    "fsofixeroperative" = @{
        Headwear = 0
        ArmorVest = 100       # Has Slick
        FaceCover = 100
        Earpiece = 100
        Backpack = 100
    }
    "fsofixerspecialist" = @{
        Headwear = 0
        ArmorVest = 100       # Has THOR
        FaceCover = 100
        Earpiece = 100
        Backpack = 100
    }
    "fsofixerlead" = @{
        Headwear = 0          # No headwear (intentional)
        ArmorVest = 0         # Empty pool — uses Siege-R rig in TacticalVest slot
        FaceCover = 100
        Earpiece = 100
        Backpack = 100
    }
    "fsofixerinnercircle" = @{
        Headwear = 100        # Bastion helmet
        ArmorVest = 0         # Empty pool — uses Siege-R rig in TacticalVest slot
        FaceCover = 100
        Earpiece = 100
        Backpack = 100
    }
}

foreach ($botName in $tierOverrides.Keys) {
    $botPath = Join-Path $fsoBotsDir "$botName.json"
    if (-not (Test-Path $botPath)) {
        Write-Host "  NOT FOUND: $botPath" -ForegroundColor Red
        continue
    }
    $bot = Get-Content $botPath -Raw | ConvertFrom-Json
    
    $overrides = $tierOverrides[$botName]
    foreach ($slot in $overrides.Keys) {
        $bot.chances.equipment.$slot = $overrides[$slot]
    }
    
    $bot | ConvertTo-Json -Depth 10 | Set-Content $botPath -Encoding UTF8
    Write-Host "  Patched $botName : Headwear=$($overrides.Headwear), ArmorVest=$($overrides.ArmorVest), FaceCover=$($overrides.FaceCover), Earpiece=$($overrides.Earpiece), Backpack=$($overrides.Backpack)" -ForegroundColor Green
}

Write-Host "`n=== Chance override complete ===" -ForegroundColor Cyan

# Verify
Write-Host "`n=== Verification ===" -ForegroundColor Yellow
foreach ($botName in $tierOverrides.Keys) {
    $bot = Get-Content "$fsoBotsDir\$botName.json" -Raw | ConvertFrom-Json
    Write-Host ("  {0,-25} HW={1} AV={2} FC={3} EP={4} BP={5}" -f $botName, $bot.chances.equipment.Headwear, $bot.chances.equipment.ArmorVest, $bot.chances.equipment.FaceCover, $bot.chances.equipment.Earpiece, $bot.chances.equipment.Backpack) -ForegroundColor Cyan
}
