using UnityEngine;

namespace PokemonGO.Global
{
    public static class Layer
    {
        private static readonly string _pointer = "Pointer";
        public static int Pointer = LayerMask.NameToLayer(_pointer);
    
        private static readonly string _ground = "Ground";
        public static int Ground = LayerMask.NameToLayer(_ground);

        public static class Mask
        {
            public static int Pointer = LayerMask.GetMask(_pointer);
            public static int Ground = LayerMask.GetMask(_ground);
        }
    }
}