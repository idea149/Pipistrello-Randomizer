using Il2CppPipistrello;
using HarmonyLib;

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
}
