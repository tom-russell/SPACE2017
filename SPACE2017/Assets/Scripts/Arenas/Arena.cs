using System;
using System.Collections.Generic;

namespace Assets.Scripts.Arenas
{
    [Serializable]
    public class Arena
    {
        public float Width { get; set; }

        public float Depth { get; set; }

        public float AveragedSize { get { return (Width + Depth) / 2; } }

        public Nest StartingNest { get; set; }

        public List<Nest> NewNests { get; set; }

        public Arena()
        {
            NewNests = new List<Nest>();
        }
    }
}
