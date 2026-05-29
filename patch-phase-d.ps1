# === FSO Phase D: Purge HK USP .45 from FSO bots ===
#
# Bug #3: Slot mod_mount_000 does not exist for weapon weapon_hk_usp_45
#         When SPT tries to generate mods for this slot, lookup returns null,
#         LINQ throws ArgumentNullException "Value cannot be null. (Parameter 'source')"
#
# Fix: Remove HK USP .45 (ID 68ded60bbd53f44248ea324f) from FSO bot inventory entirely.
# Per mae's confirmation: design swapped from USP to Staccato XC. USP was a Phase B
# leftover from an in-game preset mod-tree copy.
#
# This script:
#   - Removes USP from inventory.equipment slots (likely Holster)
#   - Removes USP as a parent in inventory.mods (its entire mod tree)
#   - Removes USP from any other weapon's slot pool that may list it
#   - Reports findings per tier

$botDir = "C:\Dev\FSO-NorvinskSection1\FSO.NorvinskSection1.Server\db\bots\types"
$uspId = "68ded60bbd53f44248ea324f"
$tiers = @("fsofixerrookie", "fsofixeroperative", "fsofixerspecialist", "fsofixerlead", "fsofixerinnercircle")

Write-Host "=== Phase D: Purge HK USP .45 from FSO bots ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Target ID: $uspId (weapon_hk_usp_45)"
Write-Host ""

$totalRemoved = 0

foreach ($tier in $tiers) {
    $file = Join-Path $botDir "$tier.json"
    if (-not (Test-Path $file)) {
        Write-Host "  SKIP $tier - file not found" -ForegroundColor Yellow
        continue
    }

    $tierShort = ($tier -replace "fsofixer", "").ToUpper()
    Write-Host "=== $tierShort ===" -ForegroundColor Yellow

    $bot = Get-Content $file -Raw | ConvertFrom-Json
    $tierRemovals = 0

    # === 1) Remove USP from inventory.equipment slots ===
    if ($bot.inventory.equipment) {
        foreach ($slotProp in $bot.inventory.equipment.PSObject.Properties) {
            $slot = $slotProp.Value
            if ($slot.PSObject.Properties | Where-Object { $_.Name -eq $uspId }) {
                $slot.PSObject.Properties.Remove($uspId)
                Write-Host "  Removed USP from inventory.equipment.$($slotProp.Name)" -ForegroundColor Green
                $tierRemovals++
            }
        }
    }

    # === 2) Remove USP as parent in inventory.mods (its entire mod tree) ===
    if ($bot.inventory.mods) {
        $uspAsParent = $bot.inventory.mods.PSObject.Properties | Where-Object { $_.Name -eq $uspId }
        if ($uspAsParent) {
            $bot.inventory.mods.PSObject.Properties.Remove($uspId)
            Write-Host "  Removed USP as PARENT in inventory.mods (entire mod tree purged)" -ForegroundColor Green
            $tierRemovals++
        }
    }

    # === 3) Remove USP from any other weapon's mod slot pool ===
    if ($bot.inventory.mods) {
        foreach ($weaponProp in $bot.inventory.mods.PSObject.Properties) {
            $weaponId = $weaponProp.Name
            foreach ($slotProp in $weaponProp.Value.PSObject.Properties) {
                $slotPool = $slotProp.Value
                if ($slotPool -is [array] -and ($slotPool -contains $uspId)) {
                    $newPool = $slotPool | Where-Object { $_ -ne $uspId }
                    # Reassign through the parent's NoteProperty
                    $weaponProp.Value.($slotProp.Name) = $newPool
                    Write-Host "  Removed USP from inventory.mods.$weaponId.$($slotProp.Name)" -ForegroundColor Green
                    $tierRemovals++
                }
            }
        }
    }

    if ($tierRemovals -eq 0) {
        Write-Host "  No USP references found in this tier" -ForegroundColor Gray
    } else {
        Write-Host "  Total removals in this tier: $tierRemovals" -ForegroundColor Cyan
    }

    # Save back
    $json = $bot | ConvertTo-Json -Depth 50
    Set-Content -Path $file -Value $json -Encoding UTF8 -NoNewline

    $totalRemoved += $tierRemovals
    Write-Host ""
}

Write-Host "=== Phase D complete ===" -ForegroundColor Cyan
Write-Host "Total USP references removed across all tiers: $totalRemoved"
Write-Host ""

# === Verification: confirm USP is gone ===
Write-Host "=== Verification ===" -ForegroundColor Cyan
foreach ($tier in $tiers) {
    $file = Join-Path $botDir "$tier.json"
    if (-not (Test-Path $file)) { continue }

    $bot = Get-Content $file -Raw | ConvertFrom-Json

    # Scan for any remaining USP references
    $rawJson = Get-Content $file -Raw
    $uspMatches = ([regex]::Matches($rawJson, [regex]::Escape($uspId))).Count

    $tierShort = ($tier -replace "fsofixer", "").ToUpper()
    if ($uspMatches -eq 0) {
        Write-Host "  $tierShort - USP references: 0 (CLEAN)" -ForegroundColor Green
    } else {
        Write-Host "  $tierShort - USP references still present: $uspMatches" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "  1. git commit (optional safety checkpoint)"
Write-Host "  2. Rebuild solution (dotnet build or VS rebuild)"
Write-Host "  3. Boot SPT server, watch for FSO loading"
Write-Host "  4. Launch SPT client, load Customs raid"
Write-Host "  5. Watch for: NO 'Slot mod_mount_000' errors, NO bot gen failures"
Write-Host "  6. Watch killfeed for FSO bot names (Bong-bong, Yum-yum, etc.)"
