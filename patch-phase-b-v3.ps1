# === FSO Phase B (PS 5.1): Apply mae-designed loadouts ===
# Strategy: pure string operations, no JSON round-trip needed
# Preserves all original formatting in bot JSONs

$botDir = "C:\Dev\FSO-NorvinskSection1\FSO.NorvinskSection1.Server\db\bots\types"

# === ID corrections ===
# AS VAL vanilla replaced with AS VAL Mod.4 from WTT-ContentBackport
# AA-12 base replaced with AA-12 Gen.2 vanilla variant
$idSwaps = @{
    "57838ad32459774a17445cd2" = "6871284e9a353bb50606f3ed"
    "5e848cc2988a8701445df1e8" = "67124dcfa3541f2a1f0e788b"
}

# === inventory.mods JSON per tier ===
# Extracted from mae profile weaponBuilds with fso- prefix
$tierModsJson = @{
    'fsofixerrookie' = @'
{
            "5926bb2186f7744b1c6c6e60": {
                "mod_magazine": [
                    "5926c3b286f774640d189b6b"
                ],
                "mod_reciever": [
                    "5926f2e086f7745aae644231"
                ],
                "mod_charge": [
                    "5926c32286f774616e42de99"
                ]
            },
            "5926f2e086f7745aae644231": {
                "mod_handguard": [
                    "5926f34786f77469195bfe92"
                ],
                "mod_stock": [
                    "5926d40686f7740f152b6b7e"
                ],
                "mod_muzzle": [
                    "5926d33d86f77410de68ebc0"
                ],
                "mod_mount": [
                    "5926dad986f7741f82604363"
                ]
            },
            "5926dad986f7741f82604363": {
                "mod_scope": [
                    "609bab8b455afd752b2e6138"
                ]
            },
            "5bd70322209c4d00d7167b8f": {
                "mod_magazine": [
                    "5ba26586d4351e44f824b340"
                ],
                "mod_muzzle": [
                    "5ba26acdd4351e003562908e"
                ],
                "mod_sight_front": [
                    "04cf4cab76ab7f4c00000000"
                ],
                "mod_sight_rear": [
                    "04cb4cab76ab7b4c00000000"
                ],
                "mod_tactical_002": [
                    "a681a1c93529b1fb2118488a"
                ],
                "mod_stock": [
                    "5bd704e7209c4d00d7167c31"
                ],
                "mod_foregrip": [
                    "5c791e872e2216001219c40a"
                ]
            },
            "5ba26acdd4351e003562908e": {
                "mod_muzzle": [
                    "5ba26ae8d4351e00367f9bdb"
                ]
            },
            "68452c3da87156b67d9ec538": {
                "mod_barrel": [
                    "684606c63dc00b27e95bb5fa"
                ],
                "mod_reciever": [
                    "68460744bf2da4e1fb5b4195"
                ],
                "mod_magazine": [
                    "68452ea1e41a7fbeb0b2ff4e"
                ],
                "mod_trigger": [
                    "68460625a5a59f05b08d55ba"
                ],
                "mod_hammer": [
                    "684606542ae9fafc455c4360"
                ]
            }
        }
'@
    'fsofixeroperative' = @'
{
            "6871284e9a353bb50606f3ed": {
                "mod_muzzle": [
                    "68712ce2251b8d4c6c04ec1f"
                ],
                "mod_reciever": [
                    "57c44f4f2459772d2c627113"
                ],
                "mod_magazine": [
                    "65118f531b90b4fc77015083"
                ],
                "mod_pistol_grip": [
                    "6878cc5bd0c26d57bf0aa37a"
                ],
                "mod_stock": [
                    "6878ccf4181ac8a5b5077236"
                ],
                "mod_handguard": [
                    "687128c4505fed5f370b1625"
                ]
            },
            "68712ce2251b8d4c6c04ec1f": {
                "mod_muzzle": [
                    "68712cafa1be89347f0d817c"
                ]
            },
            "6878ccf4181ac8a5b5077236": {
                "mod_stock_000": [
                    "5a9eb32da2750c00171b3f9c"
                ]
            },
            "687128c4505fed5f370b1625": {
                "mod_foregrip": [
                    "5a7dbfc1159bd40016548fde"
                ],
                "mod_mount_000": [
                    "68712b57a1be89347f0d8179"
                ],
                "mod_mount_001": [
                    "68712b57a1be89347f0d8179"
                ],
                "mod_mount_002": [
                    "68712bd4251b8d4c6c04ec19"
                ],
                "mod_sight_front": [
                    "04cf4cab76ab7f4c00000000"
                ]
            },
            "68712b57a1be89347f0d8179": {
                "mod_tactical": [
                    "b1c1a92264b2e9c5dc64506e"
                ]
            },
            "68712bd4251b8d4c6c04ec19": {
                "mod_scope": [
                    "544a3a774bdc2d3a388b4567"
                ],
                "mod_sight_rear": [
                    "04cb4cab76ab7b4c00000000"
                ]
            },
            "68452c3da87156b67d9ec538": {
                "mod_barrel": [
                    "684606c63dc00b27e95bb5fa"
                ],
                "mod_reciever": [
                    "68460744bf2da4e1fb5b4195"
                ],
                "mod_magazine": [
                    "68452ea1e41a7fbeb0b2ff4e"
                ],
                "mod_trigger": [
                    "68460625a5a59f05b08d55ba"
                ],
                "mod_hammer": [
                    "684606542ae9fafc455c4360"
                ]
            }
        }
'@
    'fsofixerspecialist' = @'
{
            "5df8ce05b11454561e39243b": {
                "mod_pistol_grip": [
                    "59db3a1d86f77429e05b4e92"
                ],
                "mod_magazine": [
                    "683997f125039545c12878e9"
                ],
                "mod_stock": [
                    "69326eea626d9d4943721c7a"
                ],
                "mod_reciever": [
                    "5df8e4080b92095fd441e594"
                ],
                "mod_charge": [
                    "69e2578f9e2f3658f4ff1863"
                ]
            },
            "69326eea626d9d4943721c7a": {
                "mod_stock_000": [
                    "6516e91f609aaf354b34b3e2"
                ]
            },
            "5df8e4080b92095fd441e594": {
                "mod_scope": [
                    "57ac965c24597706be5f975c"
                ],
                "mod_barrel": [
                    "5df917564a9f347bc92edca3"
                ],
                "mod_handguard": [
                    "69e2392804c7ff61f2ff1854"
                ],
                "mod_sight_rear": [
                    "04cb4cab76ab7b4c00000000"
                ]
            },
            "6516e91f609aaf354b34b3e2": {
                "mod_stock_000": [
                    "69326b8e88a95c5bcfc711d3"
                ]
            },
            "5df917564a9f347bc92edca3": {
                "mod_muzzle": [
                    "69e2558872ca7a7ea2ff1861"
                ],
                "mod_gas_block": [
                    "6065dc8a132d4d12c81fd8e3"
                ]
            },
            "69e2392804c7ff61f2ff1854": {
                "mod_tactical_000": [
                    "5b4736b986f77405cb415c10"
                ],
                "mod_sight_front": [
                    "04cf4cab76ab7f4c00000000"
                ]
            },
            "5b4736b986f77405cb415c10": {
                "mod_foregrip": [
                    "59fc48e086f77463b1118392"
                ]
            },
            "67124dcfa3541f2a1f0e788b": {
                "mod_stock": [
                    "6719023b612cc94b9008e78c"
                ],
                "mod_magazine": [
                    "6709133fa532466d5403fb7c"
                ],
                "mod_barrel": [
                    "670fd03dc424cf758f006946"
                ],
                "mod_scope": [
                    "570fd721d2720bc5458b4596"
                ]
            },
            "670fd03dc424cf758f006946": {
                "mod_muzzle": [
                    "670fd0eed8d4eae4790c818a"
                ]
            },
            "66e718dc498d978477e0ba75": {
                "mod_magazine": [
                    "66e71a7ffe3fc79ff68b582b"
                ],
                "mod_scope_000": [
                    "570fd6c2d2720bc6458b457f"
                ],
                "mod_stock": [
                    "66e71afecdde3cfc1146a53c"
                ],
                "mod_barrel_000": [
                    "66e71adf394bded29410a37e"
                ],
                "mod_handguard": [
                    "66e71dea56f10f22730b6a2d"
                ]
            },
            "66e71adf394bded29410a37e": {
                "mod_sight_front": [
                    "66e71ab0796060d309b7b244"
                ],
                "mod_muzzle": [
                    "5cf6937cd7f00c056c53fb39"
                ]
            },
            "66e71dea56f10f22730b6a2d": {
                "mod_foregrip": [
                    "59f8a37386f7747af3328f06"
                ]
            },
            "68452c3da87156b67d9ec538": {
                "mod_barrel": [
                    "684606c63dc00b27e95bb5fa"
                ],
                "mod_reciever": [
                    "68460744bf2da4e1fb5b4195"
                ],
                "mod_magazine": [
                    "68452ea1e41a7fbeb0b2ff4e"
                ],
                "mod_trigger": [
                    "68460625a5a59f05b08d55ba"
                ],
                "mod_hammer": [
                    "684606542ae9fafc455c4360"
                ]
            }
        }
'@
    'fsofixerlead' = @'
{
            "6871284e9a353bb50606f3ed": {
                "mod_muzzle": [
                    "68712ce2251b8d4c6c04ec1f"
                ],
                "mod_reciever": [
                    "57c44f4f2459772d2c627113"
                ],
                "mod_magazine": [
                    "65118f531b90b4fc77015083"
                ],
                "mod_pistol_grip": [
                    "6878cc5bd0c26d57bf0aa37a"
                ],
                "mod_stock": [
                    "6878ccf4181ac8a5b5077236"
                ],
                "mod_handguard": [
                    "687128c4505fed5f370b1625"
                ]
            },
            "68712ce2251b8d4c6c04ec1f": {
                "mod_muzzle": [
                    "68712cafa1be89347f0d817c"
                ]
            },
            "6878ccf4181ac8a5b5077236": {
                "mod_stock_000": [
                    "5a9eb32da2750c00171b3f9c"
                ]
            },
            "687128c4505fed5f370b1625": {
                "mod_foregrip": [
                    "5a7dbfc1159bd40016548fde"
                ],
                "mod_mount_000": [
                    "68712b57a1be89347f0d8179"
                ],
                "mod_mount_001": [
                    "68712b57a1be89347f0d8179"
                ],
                "mod_mount_002": [
                    "68712bd4251b8d4c6c04ec19"
                ],
                "mod_sight_front": [
                    "04cf4cab76ab7f4c00000000"
                ]
            },
            "68712b57a1be89347f0d8179": {
                "mod_tactical": [
                    "b1c1a92264b2e9c5dc64506e"
                ]
            },
            "68712bd4251b8d4c6c04ec19": {
                "mod_scope": [
                    "544a3a774bdc2d3a388b4567"
                ],
                "mod_sight_rear": [
                    "04cb4cab76ab7b4c00000000"
                ]
            },
            "66e718dc498d978477e0ba75": {
                "mod_magazine": [
                    "66e71a7ffe3fc79ff68b582b"
                ],
                "mod_scope_000": [
                    "570fd6c2d2720bc6458b457f"
                ],
                "mod_stock": [
                    "66e71afecdde3cfc1146a53c"
                ],
                "mod_barrel_000": [
                    "66e71adf394bded29410a37e"
                ],
                "mod_handguard": [
                    "66e71dea56f10f22730b6a2d"
                ]
            },
            "66e71adf394bded29410a37e": {
                "mod_sight_front": [
                    "66e71ab0796060d309b7b244"
                ],
                "mod_muzzle": [
                    "5cf6937cd7f00c056c53fb39"
                ]
            },
            "66e71dea56f10f22730b6a2d": {
                "mod_foregrip": [
                    "59f8a37386f7747af3328f06"
                ]
            },
            "68452c3da87156b67d9ec538": {
                "mod_barrel": [
                    "684606c63dc00b27e95bb5fa"
                ],
                "mod_reciever": [
                    "68460744bf2da4e1fb5b4195"
                ],
                "mod_magazine": [
                    "68452ea1e41a7fbeb0b2ff4e"
                ],
                "mod_trigger": [
                    "68460625a5a59f05b08d55ba"
                ],
                "mod_hammer": [
                    "684606542ae9fafc455c4360"
                ]
            }
        }
'@
    'fsofixerinnercircle' = @'
{
            "6871284e9a353bb50606f3ed": {
                "mod_muzzle": [
                    "68712ce2251b8d4c6c04ec1f"
                ],
                "mod_reciever": [
                    "57c44f4f2459772d2c627113"
                ],
                "mod_magazine": [
                    "65118f531b90b4fc77015083"
                ],
                "mod_pistol_grip": [
                    "6878cc5bd0c26d57bf0aa37a"
                ],
                "mod_stock": [
                    "6878ccf4181ac8a5b5077236"
                ],
                "mod_handguard": [
                    "687128c4505fed5f370b1625"
                ]
            },
            "68712ce2251b8d4c6c04ec1f": {
                "mod_muzzle": [
                    "68712cafa1be89347f0d817c"
                ]
            },
            "6878ccf4181ac8a5b5077236": {
                "mod_stock_000": [
                    "5a9eb32da2750c00171b3f9c"
                ]
            },
            "687128c4505fed5f370b1625": {
                "mod_foregrip": [
                    "5a7dbfc1159bd40016548fde"
                ],
                "mod_mount_000": [
                    "68712b57a1be89347f0d8179"
                ],
                "mod_mount_001": [
                    "68712b57a1be89347f0d8179"
                ],
                "mod_mount_002": [
                    "68712bd4251b8d4c6c04ec19"
                ],
                "mod_sight_front": [
                    "04cf4cab76ab7f4c00000000"
                ]
            },
            "68712b57a1be89347f0d8179": {
                "mod_tactical": [
                    "b1c1a92264b2e9c5dc64506e"
                ]
            },
            "68712bd4251b8d4c6c04ec19": {
                "mod_scope": [
                    "544a3a774bdc2d3a388b4567"
                ],
                "mod_sight_rear": [
                    "04cb4cab76ab7b4c00000000"
                ]
            },
            "66e718dc498d978477e0ba75": {
                "mod_magazine": [
                    "66e71a7ffe3fc79ff68b582b"
                ],
                "mod_scope_000": [
                    "570fd6c2d2720bc6458b457f"
                ],
                "mod_stock": [
                    "66e71afecdde3cfc1146a53c"
                ],
                "mod_barrel_000": [
                    "66e71adf394bded29410a37e"
                ],
                "mod_handguard": [
                    "66e71dea56f10f22730b6a2d"
                ]
            },
            "66e71adf394bded29410a37e": {
                "mod_sight_front": [
                    "66e71ab0796060d309b7b244"
                ],
                "mod_muzzle": [
                    "5cf6937cd7f00c056c53fb39"
                ]
            },
            "66e71dea56f10f22730b6a2d": {
                "mod_foregrip": [
                    "59f8a37386f7747af3328f06"
                ]
            },
            "6275303a9f372d6ea97f9ec7": {
                "mod_pistol_grip": [
                    "63f5feead259b42f0b4d6d0f"
                ],
                "mod_stock": [
                    "6516e91f609aaf354b34b3e2"
                ],
                "mod_magazine": [
                    "627bce33f21bc425b06ab967"
                ],
                "mod_foregrip": [
                    "59f8a37386f7747af3328f06"
                ],
                "mod_scope": [
                    "6284bd5f95250a29bc628a30"
                ]
            },
            "6516e91f609aaf354b34b3e2": {
                "mod_stock_000": [
                    "6516e9bc5901745209404287"
                ]
            },
            "68452c3da87156b67d9ec538": {
                "mod_barrel": [
                    "684606c63dc00b27e95bb5fa"
                ],
                "mod_reciever": [
                    "68460744bf2da4e1fb5b4195"
                ],
                "mod_magazine": [
                    "68452ea1e41a7fbeb0b2ff4e"
                ],
                "mod_trigger": [
                    "68460625a5a59f05b08d55ba"
                ],
                "mod_hammer": [
                    "684606542ae9fafc455c4360"
                ]
            }
        }
'@
}

Write-Host "=== FSO Phase B: Applying mae-designed loadouts ===" -ForegroundColor Cyan
Write-Host ""

$totalSwaps = 0
$filesPatched = 0

foreach ($tierName in $tierModsJson.Keys) {
    $file = Join-Path $botDir "$tierName.json"

    if (-not (Test-Path $file)) {
        Write-Host "  SKIP $tierName file not found" -ForegroundColor Yellow
        continue
    }

    Write-Host "  Patching $tierName.json" -ForegroundColor Cyan

    $content = Get-Content $file -Raw

    # Step 1: apply ID swaps via String.Replace
    foreach ($oldId in $idSwaps.Keys) {
        $newId = $idSwaps[$oldId]
        $occurrences = ([regex]::Matches($content, [regex]::Escape($oldId))).Count
        if ($occurrences -gt 0) {
            $content = $content.Replace($oldId, $newId)
            Write-Host ("    ID swap {0} times {1} to {2}" -f $occurrences, $oldId, $newId)
            $totalSwaps += $occurrences
        }
    }

    # Step 2: locate and replace the inventory mods block
    $newModsJson = $tierModsJson[$tierName]

    $modsStart = $content.IndexOf([char]34 + "mods" + [char]34 + ":")
    if ($modsStart -lt 0) {
        Write-Host "    ERROR could not find mods key" -ForegroundColor Red
        continue
    }

    $openBrace = $content.IndexOf([char]123, $modsStart)
    if ($openBrace -lt 0) {
        Write-Host "    ERROR could not find opening brace" -ForegroundColor Red
        continue
    }

    # Walk through matching braces to find the closing brace
    $depth = 0
    $closeBrace = -1
    for ($i = $openBrace; $i -lt $content.Length; $i++) {
        $c = $content[$i]
        if ($c -eq [char]123) { $depth++ }
        elseif ($c -eq [char]125) {
            $depth--
            if ($depth -eq 0) {
                $closeBrace = $i
                break
            }
        }
    }

    if ($closeBrace -lt 0) {
        Write-Host "    ERROR could not find matching closing brace" -ForegroundColor Red
        continue
    }

    # Replace the mods block
    $before = $content.Substring(0, $openBrace)
    $after = $content.Substring($closeBrace + 1)
    $content = $before + $newModsJson + $after

    Write-Host "    Replaced inventory.mods block"

    # Step 3: validate JSON
    try {
        $null = $content | ConvertFrom-Json
        Set-Content -Path $file -Value $content -Encoding UTF8 -NoNewline
        Write-Host "    OK saved with valid JSON" -ForegroundColor Green
        $filesPatched++
    } catch {
        Write-Host "    ERROR JSON invalid post-patch: $_" -ForegroundColor Red
        Write-Host "    File NOT saved - original preserved" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "  Files patched $filesPatched of $($tierModsJson.Count)"
Write-Host "  Total ID swaps $totalSwaps"
Write-Host ""

# === Final verification ===
Write-Host "=== FINAL VERIFICATION ===" -ForegroundColor Cyan
foreach ($tierName in $tierModsJson.Keys) {
    $file = Join-Path $botDir "$tierName.json"
    try {
        $bot = Get-Content $file -Raw | ConvertFrom-Json
        $tier = ($tierName -replace "fsofixer","").ToUpper()
        Write-Host ""
        Write-Host "  $tier" -ForegroundColor Yellow

        Write-Host "    FirstPrimaryWeapon:"
        $bot.inventory.equipment.FirstPrimaryWeapon.PSObject.Properties | ForEach-Object {
            Write-Host ("      {0}: weight {1}" -f $_.Name, $_.Value)
        }

        if ($bot.inventory.equipment.Holster) {
            Write-Host "    Holster:"
            $bot.inventory.equipment.Holster.PSObject.Properties | ForEach-Object {
                Write-Host ("      {0}: weight {1}" -f $_.Name, $_.Value)
            }
        }

        $modCount = ($bot.inventory.mods.PSObject.Properties | Measure-Object).Count
        Write-Host "    inventory.mods entries: $modCount"
    } catch {
        Write-Host "    Error reading ${tierName}: $_" -ForegroundColor Red
    }
}
