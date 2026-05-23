using System.Collections.Generic;
using MoreBotsAPI;
using Mono.Cecil;

namespace FSO.NorvinskSection1.Prepatch
{
    /// <summary>
    /// Registers the FSO custom bot types via MoreBotsAPI.
    /// 
    /// Enum range: 708300-708399 (reserved for FSO: Norvinsk Section 1).
    /// Structure modeled on RUAFComeHome.Prepatch.WildSpawnTypePatch (verified via dotPeek).
    /// </summary>
    public static class WildSpawnTypePatch
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static void Patch(ref AssemblyDefinition assembly)
        {
            // Brains we want our fixers' AI behavior to apply to.
            // PMC + ExUsec gives us PMC-tier combat behavior with the Ex-USEC patrol/regroup behaviors.
            var brainsToApply = new List<string>
            {
                "PMC",
                "ExUsec"
            };

            // AI layers to strip out so they don't trigger inappropriate behaviors.
            // Copied from RUAF's working config - these are the layers PMC-flavored bots typically don't want.
            var layersToRemove = new List<string>
            {
                "Request",
                "KnightFight",
                "PmcBear",
                "PmcUsec",
                "ExURequest",
                "StationaryWS",
                "Utility peace"
            };

            // Difficulty filter: only spawn at difficulty 1 (Normal).
            // Indices 0, 2, 3 = Easy, Hard, Impossible — all excluded.
            var normalOnly = new List<int> { 0, 2, 3 };

            // Brain ID. 9 matches RUAF's working configuration.
            int baseBrain = 9;

            // === 708300: Fledgling Fixer (Rookie) ===
            // Entry-level operatives. SMGs + flashbangs. Support role in patrols.
            var rookie = new CustomWildSpawnType(708300, "fsoFixerRookie", "FSO", baseBrain, true, true);
            rookie.SetCountAsBossForStatistics(new bool?(false));
            rookie.SetShouldUseFenceNoBossAttack(false);
            rookie.SetExcludedDifficulties(normalOnly);

            var rookieSAIN = new SAINSettings(rookie.WildSpawnTypeValue)
            {
                Name = "Fledgling Fixer",
                Description = "FSO Norvinsk Section 1 - entry-level operative. SMG + flashbang support role.",
                Section = "FSO",
                BaseBrain = "PMC",
                BrainsToApply = brainsToApply,
                LayersToRemove = layersToRemove,
                DifficultyModifier = 0.5f
            };
            rookie.SetSAINSettings(rookieSAIN);
            CustomWildSpawnTypeManager.RegisterWildSpawnType(rookie, assembly);

            // === 708301: Office Fixer (Operative) ===
            // Standard career fixers. Suppressed carbines, frags.
            var operative = new CustomWildSpawnType(708301, "fsoFixerOperative", "FSO", baseBrain, true, true);
            operative.SetCountAsBossForStatistics(new bool?(false));
            operative.SetShouldUseFenceNoBossAttack(false);
            operative.SetExcludedDifficulties(normalOnly);

            var operativeSAIN = new SAINSettings(operative.WildSpawnTypeValue)
            {
                Name = "Office Fixer",
                Description = "FSO Norvinsk Section 1 - standard career fixer. Suppressed carbine, high-pen ammo.",
                Section = "FSO",
                BaseBrain = "PMC",
                BrainsToApply = brainsToApply,
                LayersToRemove = layersToRemove,
                DifficultyModifier = 0.66f
            };
            operative.SetSAINSettings(operativeSAIN);
            CustomWildSpawnTypeManager.RegisterWildSpawnType(operative, assembly);

            // === 708302: Field Specialist ===
            // Randomized role: marksman, breacher, or LMG suppression.
            var specialist = new CustomWildSpawnType(708302, "fsoFixerSpecialist", "FSO", baseBrain, true, true);
            specialist.SetCountAsBossForStatistics(new bool?(false));
            specialist.SetShouldUseFenceNoBossAttack(false);
            specialist.SetExcludedDifficulties(normalOnly);

            var specialistSAIN = new SAINSettings(specialist.WildSpawnTypeValue)
            {
                Name = "Field Specialist",
                Description = "FSO Norvinsk Section 1 - specialist role: DMR, AA-12, or M249.",
                Section = "FSO",
                BaseBrain = "PMC",
                BrainsToApply = brainsToApply,
                LayersToRemove = layersToRemove,
                DifficultyModifier = 0.66f
            };
            specialist.SetSAINSettings(specialistSAIN);
            CustomWildSpawnTypeManager.RegisterWildSpawnType(specialist, assembly);

            // === 708303: Senior Fixer (Lead) ===
            // Patrol leader. Tougher than standard operatives.
            var lead = new CustomWildSpawnType(708303, "fsoFixerLead", "FSO", baseBrain, true, true);
            lead.SetCountAsBossForStatistics(new bool?(false));
            lead.SetShouldUseFenceNoBossAttack(false);
            lead.SetExcludedDifficulties(normalOnly);

            var leadSAIN = new SAINSettings(lead.WildSpawnTypeValue)
            {
                Name = "Senior Fixer",
                Description = "FSO Norvinsk Section 1 - patrol leader. Top-tier AS VAL Mod.4.",
                Section = "FSO",
                BaseBrain = "PMC",
                BrainsToApply = brainsToApply,
                LayersToRemove = layersToRemove,
                DifficultyModifier = 0.8f
            };
            lead.SetSAINSettings(leadSAIN);
            CustomWildSpawnTypeManager.RegisterWildSpawnType(lead, assembly);

            // === 708304: Inner Circle ===
            // Mae's personal team. Labs-only, Quest 5 finale. Maximum difficulty.
            var innerCircle = new CustomWildSpawnType(708304, "fsoFixerInnerCircle", "FSO", baseBrain, true, true);
            innerCircle.SetCountAsBossForStatistics(new bool?(false));
            innerCircle.SetShouldUseFenceNoBossAttack(false);
            innerCircle.SetExcludedDifficulties(normalOnly);

            var innerCircleSAIN = new SAINSettings(innerCircle.WildSpawnTypeValue)
            {
                Name = "Inner Circle",
                Description = "FSO Norvinsk Section 1 - Mae's personal team. Thermal optics, MIKOR, LMG.",
                Section = "FSO",
                BaseBrain = "PMC",
                BrainsToApply = brainsToApply,
                LayersToRemove = layersToRemove,
                DifficultyModifier = 1.0f
            };
            innerCircle.SetSAINSettings(innerCircleSAIN);
            CustomWildSpawnTypeManager.RegisterWildSpawnType(innerCircle, assembly);

            // Group all five FSO bot types together so they can spawn as a patrol.
            CustomWildSpawnTypeManager.AddSuitableGroup(new List<int>
            {
                708300,
                708301,
                708302,
                708303,
                708304
            });
        }
    }
}