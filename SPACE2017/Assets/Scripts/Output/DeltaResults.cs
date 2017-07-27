namespace Assets.Scripts.Output
{
    public abstract class DeltaResults : Results
    {
        public DeltaResults(SimulationManager simulation, string fileNameWithoutExtension) :
            base(simulation, fileNameWithoutExtension)
        {
        }
    }
}
