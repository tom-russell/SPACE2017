using Assets.Scripts.Config;
using Assets.Scripts.Output;
using Assets.Scripts.Ticking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Assets
{
    public class ResultsManager : IDisposable, ITickable
    {
        public SimulationManager Simulation { get; private set; }

        public bool ShouldBeRemoved { get { return false; } }

        private List<Results> _results;

        public ResultsManager(SimulationManager simulation)
        {
            Simulation = simulation;
            SetupOutput();
        }

        private void SetupOutput()
        {
            var outDir = "Results";

            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            var experimentName = Simulation.Settings.ExperimentName.Value;

            if (string.IsNullOrEmpty(experimentName))
                experimentName = DateTime.Now.ToString("yyyyMMddHHmm");

            var experimentPath = Path.Combine(outDir, experimentName);

            if (Directory.Exists(experimentPath))
            {
                int suffix = 1;
                while (Directory.Exists(experimentPath + "_" + suffix))
                    suffix++;

                experimentPath = experimentPath + "_" + suffix;
            }

            Directory.CreateDirectory(experimentPath);

            using (StreamWriter sw = new StreamWriter(Path.Combine(experimentPath, "settings.xml")))
            {
                XmlSerializer xml = new XmlSerializer(typeof(SimulationSettings));

                xml.Serialize(sw, Simulation.Settings);
            }

            _results = new List<Results>();

            _results.Add(new ExecutionResults(Simulation, experimentPath));

            if (Simulation.Settings.OutputEmigrationData.Value)
                _results.Add(new EmigrationResults(Simulation, experimentPath));
            if (Simulation.Settings.OutputColonyData.Value)
                _results.Add(new ColonyResults(Simulation, experimentPath));
            if (Simulation.Settings.OutputAntDelta.Value)
                _results.Add(new AntDeltaResults(Simulation, experimentPath));
            if (Simulation.Settings.OutputAntDetail.Value)
                _results.Add(new AntDetailedResults(Simulation, experimentPath));
            if (Simulation.Settings.OutputLegacyData.Value)
                _results.Add(new LegacyResults(Simulation, experimentPath));
            if (Simulation.Settings.OutputAntDebug.Value)
                _results.Add(new AntDebugResults(Simulation, experimentPath));
            if (Simulation.Settings.OutputAntStateDistribution.Value)
                _results.Add(new AntStateDistributionResults(Simulation, experimentPath));
            if (Simulation.Settings.OutputEndSimData.Value)
                _results.Add(new EndSimDataResults(Simulation, experimentPath));
        }

        public void Dispose()
        {
            foreach (var res in _results)
                res.Dispose();
        }

        public void Tick(float elapsedSimulationMS)
        {
            foreach (var res in _results)
                res.Step(Simulation.TickManager.CurrentTick);
        }

        public void SimulationStarted()
        {
            foreach (var res in _results)
                res.SimulationStarted();
        }

        public void SimulationStopped()
        {
            foreach (var res in _results)
                res.SimulationStopped();
        }
    }
}
