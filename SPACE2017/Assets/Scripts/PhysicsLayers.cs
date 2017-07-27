namespace Assets.Scripts
{
    public class PhysicsLayers
    {
        public static int Ants { get; private set; }
        public static int Walls { get; private set; }
        public static int AntsAndWalls { get; private set; }


        // Layer masks work with bit operations: "1 << x" is a mask including only colliders from layer x 
        static PhysicsLayers()
        {
            // Layers 0-7 are Unity Built-in layers
            int antLayer = 8;
            int wallLayer = 9;

            Ants = 1 << antLayer;
            Walls = 1 << wallLayer;
            AntsAndWalls = Ants | Walls;
        }
    }
}
