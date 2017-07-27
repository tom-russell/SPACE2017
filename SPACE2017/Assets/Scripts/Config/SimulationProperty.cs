using System;
using System.Xml.Serialization;

namespace Assets.Scripts.Config
{
    public abstract class SimulationPropertyBase
    {
        [XmlIgnore]
        public abstract string Name { get; }

        [XmlIgnore]
        public abstract string Description { get; }

        public abstract string GetValue();

        public abstract string SetValue(string newValue);
    }

    [Serializable]
    public abstract class SimulationProperty<T> : SimulationPropertyBase
    {
        public T Value { get; set; }

        public override string GetValue()
        {
            return Value == null ? string.Empty : Value.ToString();
        }
    }

    public abstract class SimulationBoolProperty : SimulationProperty<bool>
    {
        public override string SetValue(string newValue)
        {
            bool v;
            if (!bool.TryParse(newValue, out v))
                return newValue;

            Value = v;
            return v.ToString();
        }
    }

    public abstract class SimulationStringProperty : SimulationProperty<string>
    {
        public override string SetValue(string newValue)
        {
            Value = newValue;
            return newValue;
        }
    }

    public abstract class SimulationFloatProperty : SimulationProperty<float>
    {
        public abstract float? MinValue { get; }

        public abstract float? MaxValue { get; }

        public override string SetValue(string newValue)
        {
            // If unable to parse then return the attempted value
            float v;
            if (!float.TryParse(newValue, out v))
                return newValue;

            if (MinValue.HasValue && v < MinValue.Value)
            {
                Value = MinValue.Value;
                return MinValue.Value.ToString();
            }

            if (MaxValue.HasValue && v > MaxValue.Value)
            {
                Value = MaxValue.Value;
                return MaxValue.Value.ToString();
            }

            Value = v;
            return v.ToString();
        }
    }

    public abstract class SimulationIntProperty : SimulationProperty<int>
    {
        public abstract int? MinValue { get; }

        public abstract int? MaxValue { get; }

        public override string SetValue(string newValue)
        {
            // If unable to parse then return the attempted value
            int v;
            if (!int.TryParse(newValue, out v))
                return newValue;

            if (MinValue.HasValue && v < MinValue.Value)
            {
                Value = MinValue.Value;
                return MinValue.Value.ToString();
            }

            if (MaxValue.HasValue && v > MaxValue.Value)
            {
                Value = MaxValue.Value;
                return MaxValue.Value.ToString();
            }

            Value = v;
            return v.ToString();
        }
    }
}
