using Assets.Common;
using Assets.Scripts.Ticking;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Ants
{
    public class EmigrationInformation : ITickable
    {
        public bool ShouldBeRemoved { get { return false; } }

        public SimulationManager Simulation { get; private set; }

        public EmigrationData Data { get { return _data.Value; } }

        private Lazy<EmigrationData> _data;

        public EmigrationInformation(SimulationManager simulationManager)
        {
            Simulation = simulationManager;

            _data = new Lazy<EmigrationData>(() =>
            {
                var data = new EmigrationData();

                NestManager bestNest = null;

                foreach (var ni in Simulation.NestInfo)
                {
                    if (ni.IsStartingNest)
                    {
                        data.PassivesInOldNest = ni.AntsPassive.transform.childCount;
                    }
                    else
                    {
                        data.PassivesInNewNests.Add(ni.NestId, ni.AntsPassive.transform.childCount);

                        if(bestNest == null || bestNest.quality < ni.Nest.quality)
                        {
                            bestNest = ni.Nest;
                        }
                    }
                }

                var antsInBestNest = bestNest == null ? 0 : data.PassivesInNewNests[Simulation.GetNestID(bestNest)];
                var emigratedAnts = (int)data.PassivesInNewNests.Sum(p => p.Value);
                var totalAnts = emigratedAnts+ data.PassivesInOldNest;

                data.EmigrationCompletion = (float)(totalAnts - data.PassivesInOldNest) / (float)totalAnts;
                data.EmigrationRelativeAccuracy = (float)antsInBestNest / (float)emigratedAnts;
                data.EmigrationAbsoluteAccuracy = (float)antsInBestNest / (float)totalAnts;

                return data;
            });
        }

        public void Tick(float elapsedSimulationMS)
        {
            _data.ReloadValue = true;
        }

        public void SimulationStarted()
        {
        }

        public void SimulationStopped()
        {
        }

        public class EmigrationData
        {
            public int PassivesInOldNest { get; set; }

            public float EmigrationCompletion { get; set; }
            public float EmigrationRelativeAccuracy { get; set; }
            public float EmigrationAbsoluteAccuracy { get; set; }

            public Dictionary<int, int> PassivesInNewNests { get; set; }

            public EmigrationData()
            {
                PassivesInNewNests = new Dictionary<int, int>();
            }
        }
    }
}
