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

            if(!record.flags.ContainsKey(Translator.randomizerSeedFlag))
            {
                // No stored seed, must generate
                MelonLogger.Msg("Generating a new seed");
                Random rand = new Random();

                // Induce floating point conversion error that file save/load will encounter
                float seedFloat = rand.Next();
                int seedInt = (int) seedFloat;
                record.flags[Translator.randomizerSeedFlag] = seedInt;
            }
            else
            {
                MelonLogger.Msg("Found seed in game record");
            }
            int seed = record.flags[Translator.randomizerSeedFlag];
            MelonLogger.Msg($"seed from flags = {seed}");
            Translator.Randomize(seed);
        }

    }
}
