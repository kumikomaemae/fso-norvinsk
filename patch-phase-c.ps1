# === FSO Phase C v2: Match Knight's pattern (lastName ABSENT) + inventory.items fix ===
#
# KEY INSIGHT (mae's catch): Knight's bot JSON has NO lastName field at all.
# It's absent, not empty. This is the working pattern for single-name bots.
#
# JSON absent != JSON empty array. SPT loader treats them differently:
#   - ABSENT: "skip name component" code path - SAFE
#   - EMPTY:  "iterate this array" code path - NRE on array[0] when len=0
#
# Fix Bug #1: REMOVE the lastName field entirely from each FSO bot
# Fix Bug #2: ADD inventory.items from BEAR as reference loot pool

$botDir = "C:\Dev\FSO-NorvinskSection1\FSO.NorvinskSection1.Server\db\bots\types"
$sptBots = "C:\Games\SPT\SPT\SPT_Data\database\bots\types"

# === STEP 1: Load BEAR's inventory.items as reference ===
Write-Host "=== Phase C v2: lastName removal + inventory.items fix ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Approach: DELETE lastName field (matches Knight's working pattern)"
Write-Host "          ADD inventory.items from BEAR"
Write-Host ""

if (-not (Test-Path "$sptBots\pmcbear.json")) {
    Write-Host "ERROR: BEAR JSON not found at $sptBots\pmcbear.json" -ForegroundColor Red
    exit 1
}

Write-Host "Loading BEAR reference..." -ForegroundColor Cyan
$bear = Get-Content "$sptBots\pmcbear.json" -Raw | ConvertFrom-Json
$bearInventoryItems = $bear.inventory.items
$itemsCategories = @($bearInventoryItems.PSObject.Properties | Measure-Object).Count
Write-Host "  Loaded BEAR inventory.items with $itemsCategories item categories" -ForegroundColor Green
Write-Host ""

# === STEP 2: Patch each FSO tier ===
$tiers = @("fsofixerrookie", "fsofixeroperative", "fsofixerspecialist", "fsofixerlead", "fsofixerinnercircle")
$patched = 0

foreach ($tier in $tiers) {
    $file = Join-Path $botDir "$tier.json"

    if (-not (Test-Path $file)) {
        Write-Host "  SKIP $tier - file not found" -ForegroundColor Yellow
        continue
    }

    Write-Host "Patching $tier" -ForegroundColor Cyan
    $bot = Get-Content $file -Raw | ConvertFrom-Json

    # Preserve current firstName
    $firstNameCount = @($bot.firstName).Count
    Write-Host "  firstName preserved: $firstNameCount entries"

    # Bug #1 fix: REMOVE lastName field entirely
    $hadLastName = $bot.PSObject.Properties | Where-Object { $_.Name -eq "lastName" }
    if ($hadLastName) {
        $bot.PSObject.Properties.Remove("lastName")
        Write-Host "  REMOVED lastName field (now matches Knight pattern)" -ForegroundColor Green
    } else {
        Write-Host "  lastName already absent" -ForegroundColor Gray
    }

    # Bug #2 fix: inventory.items
    $hasItems = $bot.inventory.PSObject.Properties | Where-Object { $_.Name -eq "items" }
    if (-not $hasItems) {
        Add-Member -InputObject $bot.inventory -MemberType NoteProperty -Name "items" -Value $bearInventoryItems
        Write-Host "  Added inventory.items from BEAR ($itemsCategories categories)" -ForegroundColor Green
    } else {
        Write-Host "  inventory.items already exists, skipping" -ForegroundColor Gray
    }

    # Save back
    $json = $bot | ConvertTo-Json -Depth 50
    Set-Content -Path $file -Value $json -Encoding UTF8 -NoNewline
    Write-Host "  Saved" -ForegroundColor Green
    Write-Host ""

    $patched++
}

# === STEP 3: Verify ===
Write-Host "=== Verification ===" -ForegroundColor Cyan
foreach ($tier in $tiers) {
    $file = Join-Path $botDir "$tier.json"
    if (-not (Test-Path $file)) { continue }

    try {
        $bot = Get-Content $file -Raw | ConvertFrom-Json
        $firstNameCount = @($bot.firstName).Count
        $hasLastName = ($bot.PSObject.Properties | Where-Object { $_.Name -eq "lastName" }) -ne $null
        $hasItems = ($bot.inventory.PSObject.Properties | Where-Object { $_.Name -eq "items" }) -ne $null
        $invFields = @($bot.inventory.PSObject.Properties | Measure-Object).Count
        $topLevelFields = @($bot.PSObject.Properties | Measure-Object).Count

        $tierShort = ($tier -replace "fsofixer", "").ToUpper()
        Write-Host ""
        Write-Host "  $tierShort" -ForegroundColor Yellow
        Write-Host "    firstName entries: $firstNameCount"
        Write-Host "    lastName field present: $hasLastName  (expected: False)"
        Write-Host "    inventory.items present: $hasItems  (expected: True)"
        Write-Host "    inventory total sub-fields: $invFields  (expected: 4)"
        Write-Host "    top-level fields: $topLevelFields  (expected: 9, was 10 before)"
    } catch {
        Write-Host "  ERROR reading ${tier}: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Phase C v2 complete: $patched files patched ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "  1. Rebuild solution (dotnet build or VS rebuild)"
Write-Host "  2. Boot SPT server - watch FSO boot lines + check for new errors"
Write-Host "  3. Launch SPT client - load Customs raid"
Write-Host "  4. Watch killfeed for FSO bot names (should display as single-name handles)"
Write-Host ""
Write-Host "EXPECTED OUTCOME if hypothesis correct:" -ForegroundColor Green
Write-Host "  - NO 'Failed to generate bot' errors in SPT server log"
Write-Host "  - NO BotsPresets.CreateProfile NRE in EFT traces.log"
Write-Host "  - FSO bots spawn with single-name handles (Bong-bong, Yum-yum, etc.)"
