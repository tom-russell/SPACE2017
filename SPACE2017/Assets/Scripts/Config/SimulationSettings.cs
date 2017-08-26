using System;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections.Generic;
using Assets.Common;

namespace Assets.Scripts.Config
{
    [Serializable]
    public class SimulationSettings
    {
        public string ArenaName { get; set; }

        public string ArenaFilename { get; set; }

        [SerializeField]
        public ExperimentName ExperimentName { get; set; }

        [SerializeField]
        public RandomSeed RandomSeed { get; set; }

        [SerializeField]
        public SimulationEndPoint SimulationEndPoint { get; set; }

        [SerializeField]
        public ColonySize ColonySize { get; set; }

        [SerializeField]
        public QuorumThreshold QuorumThreshold { get; set; }

        [SerializeField]
        public ProportionActive ProportionActive { get; set; }

        [SerializeField]
        public StartingNestQuality StartingNestQuality { get; set; }

        [SerializeField]
        public FirstNewNestQuality FirstNewNestQuality { get; set; }

        [SerializeField]
        public SecondNewNestQuality SecondNewNestQuality { get; set; }

        [SerializeField]
        public StartingTimeScale StartingTimeScale { get; set; }

        [SerializeField]
        public RandomiseTimeScale RandomiseTimeScale { get; set; }

        [SerializeField]
        public MaximumSimulationRunTime MaximumSimulationRunTime { get; set; }

        [SerializeField]
        public WaitNewNestFactor WaitNewNestFactor { get; set; }

        [SerializeField]
        public AntsLayPheromones AntsLayPheromones { get; set; }

        [SerializeField]
        public AntsReverseTandemRun AntsReverseTandemRun { get; set; }

        [SerializeField]
        public ModifyParameterName ModifyParameter1Name { get; set; }

        [SerializeField]
        public ModifyParameterValue ModifyParameter1Value { get; set; }

        [SerializeField]
        public ModifyParameterName ModifyParameter2Name { get; set; }

        [SerializeField]
        public ModifyParameterValue ModifyParameter2Value { get; set; }

        [SerializeField]
        public OutputTickRate OutputTickRate { get; set; }

        [SerializeField]
        public OutputEmigrationData OutputEmigrationData { get; set; }

        [SerializeField]
        public OutputColonyData OutputColonyData { get; set; }

        [SerializeField]
        public OutputAntDelta OutputAntDelta { get; set; }

        [SerializeField]
        public OutputAntStateDistribution OutputAntStateDistribution { get; set; }

        [SerializeField]
        public OutputAntDetail OutputAntDetail { get; set; }

        [SerializeField]
        public OutputLegacyData OutputLegacyData { get; set; }

        [SerializeField]
        public OutputAntDebug OutputAntDebug { get; set; }

        [SerializeField]
        public OutputEndSimData OutputEndSimData { get; set; }

        [XmlIgnore]
        public List<SimulationPropertyBase> AllProperties { get { return _allProperties.Value; } }

        private Lazy<List<SimulationPropertyBase>> _allProperties;

        // Sections for colony
        // Sections for ant behaviour etc
        // Sections for map

        public SimulationSettings()
        {
            ArenaName = "Equidistant";

            ExperimentName = new ExperimentName();

            RandomSeed = new RandomSeed();
            SimulationEndPoint = new SimulationEndPoint();
            
            StartingTimeScale = new StartingTimeScale();
            RandomiseTimeScale = new RandomiseTimeScale();
            MaximumSimulationRunTime = new MaximumSimulationRunTime();

            ColonySize = new ColonySize();
            QuorumThreshold = new QuorumThreshold();
            ProportionActive = new ProportionActive();

            WaitNewNestFactor = new WaitNewNestFactor();
            AntsLayPheromones = new AntsLayPheromones();
            AntsReverseTandemRun = new AntsReverseTandemRun();

            StartingNestQuality = new StartingNestQuality();
            FirstNewNestQuality = new FirstNewNestQuality();
            SecondNewNestQuality = new SecondNewNestQuality();

            ModifyParameter1Name = new ModifyParameterName();
            ModifyParameter1Value = new ModifyParameterValue();
            ModifyParameter2Name = new ModifyParameterName();
            ModifyParameter2Value = new ModifyParameterValue();

            OutputTickRate = new OutputTickRate();
            OutputEmigrationData = new OutputEmigrationData();
            OutputColonyData = new OutputColonyData();
            OutputAntStateDistribution = new OutputAntStateDistribution();
            OutputAntDelta = new OutputAntDelta();
            OutputAntDetail = new OutputAntDetail();
            OutputLegacyData = new OutputLegacyData();
            OutputAntDebug = new OutputAntDebug();
            OutputEndSimData = new OutputEndSimData();

            _allProperties = new Lazy<List<SimulationPropertyBase>>(() =>
             {
                 return new List<SimulationPropertyBase>
                {
                    ExperimentName,

                    RandomSeed,
                    SimulationEndPoint,
                    StartingTimeScale,
                    RandomiseTimeScale,
                    MaximumSimulationRunTime,

                    ColonySize,
                    QuorumThreshold,
                    ProportionActive,

                    StartingNestQuality,
                    FirstNewNestQuality,
                    SecondNewNestQuality,

                    WaitNewNestFactor,
                    AntsLayPheromones,
                    AntsReverseTandemRun,

                    ModifyParameter1Name,
                    ModifyParameter1Value,
                    ModifyParameter2Name,
                    ModifyParameter2Value,

                    OutputTickRate,
                    OutputEmigrationData,
                    OutputColonyData,
                    OutputAntDelta,
                    OutputAntStateDistribution,
                    OutputAntDetail,
                    OutputLegacyData,
                    OutputAntDebug,
                    OutputEndSimData
               };
             });
        }
    }
}
