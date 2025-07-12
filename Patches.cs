using Il2CppPipistrello;
using HarmonyLib;
using MelonLoader;
using Il2CppUtil;

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
    public static class Director_InitFromRecord_Patch
    {
        private static void Prefix(Director __instance, Game.Record record)
        {
            if (__instance.GetFlag(Translator.randomizerEnabledFlag) == 1)
            {
                // Starting randomizer game. Set game record
                record.flags[Translator.randomizerEnabledFlag] = 1;
            }
            if (!record.flags.ContainsKey(Translator.randomizerEnabledFlag) || record.flags[Translator.randomizerEnabledFlag] != 1)
            {
                // Randomizer disabled
                return;
            }

            if (!record.flags.ContainsKey(Translator.randomizerSeedUpperFlag) || !record.flags.ContainsKey(Translator.randomizerSeedLowerFlag))
            {
                // No stored seed, must generate
                MelonLogger.Msg("Generating a new seed");
                System.Random rand = new System.Random();

                // Split seed int 2 halves to avoid floating point conversion error
                int newSeed = rand.Next();
                int seedUpper = (int)(newSeed & 0xffff0000) >> 16;
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

    [HarmonyPatch(typeof(Menu), nameof(Menu.MakeSavefileNewMenu), new Type[] { typeof(Director), typeof(int) })]
    public static class Menu_MakeSavefileNewMenu_Patch
    {
        public static void Postfix(ref UIDialog __result, Director director, int savefileIndex)
        {
            Action randomizerButtonPressed = () => {
                // Mark next created save as randomizer game
                director.SetFlag(Translator.randomizerEnabledFlag, 1);
                // Start new game
                director.InitFromSavefile(savefileIndex, false);
            };

            // Create randomizer button
            UIButton button = new();
            string diceSprite = "custom/randomizer/dice";
            if (SpriteManager.SpriteExists(diceSprite))
            {
                MelonLogger.Msg("Setting dice sprite");
                button = button.SetIcon(diceSprite);
            }
            else
            {
                MelonLogger.Msg("Using backup icon");
                button = button.SetIcon("ui/icons/hook");
            }
            button.Centered();
            button.pressFn = randomizerButtonPressed;

            // Center and add randomizer button as UIElement object
            UIElement e = button.Cast<UIElement>();
            e = e.At(0, 3, 1, 4);
            __result.rootElement.subElements.Add(e);

            // Move "New Game", "New Game+", and "Back" buttons down
            // BUG: Does not play nice with debug options
            for (int i = 3; i < 6; i++)
            {
                UIElement currentElement = __result.rootElement.subElements[i];
                __result.rootElement.subElements[i] = currentElement.At(0, i + 4, 1, 1);
            }
        }
    }
}
