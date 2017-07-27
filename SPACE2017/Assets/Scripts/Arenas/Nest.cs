using System;

namespace Assets.Scripts.Arenas
{
    [Serializable]
    public class Nest
    {
        public float Width { get; set; }

        public float Depth { get; set; }

        public float PositionX { get; set; }

        public float PositionZ { get; set; }

        public float Quality { get; set; }
    }
}
