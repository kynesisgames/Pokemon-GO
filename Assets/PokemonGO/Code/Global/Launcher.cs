using UnityEngine;

namespace PokemonGO.Global
{
    public static class Launcher
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            Application.targetFrameRate = 60;
        }
    }
}
