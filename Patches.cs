using Il2CppPipistrello;
using HarmonyLib;
using MelonLoader;

namespace Randomizer
{
    [HarmonyPatch(typeof(Director), nameof(Director.InstantiateFromMap), new Type[] { typeof(Mapvania.Object) })]
    public static class Director_InstantiateFromMap_Patch
    {
        private static void Prefix(Director __instance, Mapvania.Object mapObj)
        {
            Translator.TranslateObj(ref mapObj);
        }
    }


    [HarmonyPatch(typeof(Director), nameof(Director.InitFromRecord), new Type[] { typeof(Game.Record) })]
    public static class Director_InstantiaawdawdsteFromMap_Patch
    {
        private static void Prefix(Game.Record record)
        {
            if(!record.flags.ContainsKey(Translator.randomizerEnabledFlag) || record.flags[Translator.randomizerEnabledFlag] != 1)
            {
                // Randomizer disabled
                return;
            }

            if(!record.flags.ContainsKey(Translator.randomizerSeedUpperFlag) || !record.flags.ContainsKey(Translator.randomizerSeedLowerFlag))
            {
                // No stored seed, must generate
                MelonLogger.Msg("Generating a new seed");
                Random rand = new Random();

                // Split seed int 2 halves to avoid floating point conversion error
                int newSeed = rand.Next();
                int seedUpper = (int) (newSeed & 0xffff0000) >> 16;
                int seedLower = newSeed & 0xffff;

                record.flags[Translator.randomizerSeedUpperFlag] = seedUpper;
                record.flags[Translator.randomizerSeedLowerFlag] = seedLower;
            }
            else
            {
                MelonLogger.Msg("Found seed in game record");
            }
            int seed = (record.flags[Translator.randomizerSeedUpperFlag] << 8) | record.flags[Translator.randomizerSeedLowerFlag];
            MelonLogger.Msg($"seed from flags = {seed}");
            Translator.Randomize(seed);
        }
    }
}
