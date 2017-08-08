namespace Assets.Scripts.Output
{
    public abstract class FixedTickResults : Results
    {
        private int _tickRate;
        private int _untilTick = 0;

        public FixedTickResults(SimulationManager simulation, string fileNameWithoutExtension) :
            base(simulation, fileNameWithoutExtension)
        {
            _tickRate = simulation.Settings.OutputTickRate.Value;
        }

        public override void Step(long step)
        {
            _untilTick--;

            if(_untilTick <= 0)
            {
                _untilTick = _tickRate;
                OutputData(step);
            }
        }

        protected abstract void OutputData(long step);
    }
}
