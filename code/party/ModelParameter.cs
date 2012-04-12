using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace party
{
    public enum ModelParameterType
    {
        Red,
        Green,
        Blue,
        Alpha,
        Size,
        Mass,
        Angle,
        TextureIndex,
        RotationSpeed,
        SizeMultiplier,
        //Custom1,
        //Custom2,
    };

    public class InterpolatorEntry
    {
        public float Keyframe { get; set; }

        MinMaxField value;
        [RefreshProperties(RefreshProperties.All)]
        public MinMaxField Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                Random = value.IsRandom;
            }
        }

        public bool Random { get; set; }

        public object Serialize()
        {
            Hashtable entry = new Hashtable();
            entry.Add("keyframe", Keyframe);
            entry.Add("value", Value.Serialize());
            return entry;
        }

        public static InterpolatorEntry Deserialize(object o)
        {
            Hashtable entry_data = (Hashtable)o;
            InterpolatorEntry entry = new InterpolatorEntry();
            entry.Keyframe = (float)(double)entry_data["keyframe"];
            entry.Value = MinMaxField.Deserialize(entry_data["value"]);
            entry.Random = entry.Value.IsRandom;
            return entry;
        }
    }

    public enum InterpolatorType
    {
        Age,
        Lifetime,
        [Browsable(false)]
        Parameter,
        Velocity
    };

    [Editor(typeof(InterpolatorGroupEditor), typeof(UITypeEditor))]
    public class InterpolatorGroup
    {
        public event EventHandler Changed;

        List<Interpolator> interpolators = new List<Interpolator>();
        public List<Interpolator> Interpolators
        {
            get { return interpolators; }
        }

        public Interpolator this[ModelParameterType param]
        {
            get { return interpolators[(int)param]; }
        }

        public InterpolatorGroup()
        {
            Array interpolator_params = Enum.GetValues(typeof(ModelParameterType));
            foreach (object param in interpolator_params)
            {
                Interpolator interpolator = new Interpolator();
                interpolator.Parameter = (ModelParameterType)param;
                interpolator.Reset();
                Interpolators.Add(interpolator);
            }
        }

        public object Serialize()
        {
            ArrayList serialized_interpolators = new ArrayList(interpolators.Count);
            foreach (Interpolator interpolator in interpolators)
            {
                if (interpolator.IsDefault) continue;
                serialized_interpolators.Add(interpolator.Serialize());
            }
            return serialized_interpolators;
        }

        public static InterpolatorGroup Deserialize(object o)
        {
            ArrayList serialized_interpolators = (ArrayList)o;
            InterpolatorGroup group = new InterpolatorGroup();
            if (serialized_interpolators != null)
            {
                foreach (object interpolator_data in serialized_interpolators)
                {
                    Interpolator interpolator = Interpolator.Deserialize(interpolator_data);
                    group.interpolators[(int)interpolator.Parameter] = interpolator;
                }
            }
            return group;
        }

        public void OnChanged(Interpolator interpolator)
        {
            if (Changed != null) Changed(interpolator, EventArgs.Empty);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Interpolator
    {
        public static float GetDefaultValueForParameter(ModelParameterType parameter)
        {
            switch (parameter)
            {
                case ModelParameterType.Red:
                case ModelParameterType.Green:
                case ModelParameterType.Blue:
                case ModelParameterType.Alpha:
                case ModelParameterType.Size:
                case ModelParameterType.Mass:
                    return 1.0f;
                case ModelParameterType.Angle:
                case ModelParameterType.TextureIndex:
                case ModelParameterType.RotationSpeed:
                    return 0.0f;
                case ModelParameterType.SizeMultiplier:
                    return 1.0f;
                default:
                    return 0.0f;
            }
        }

        [DefaultValue(InterpolatorType.Lifetime)]
        InterpolatorType interpolate_on = InterpolatorType.Lifetime;
        public InterpolatorType InterpolateOn
        {
            get { return interpolate_on; }
            internal set
            {
                if (interpolate_on != value)
                {
                    interpolate_on = value;
                }
            }
        }

        ModelParameterType parameter;
        [Browsable(false)]
        public ModelParameterType Parameter
        {
            get
            {
                return parameter;
            }
            set
            {
                if (parameter != value)
                {
                    parameter = value;
                }
            }
        }

        List<InterpolatorEntry> entries;
        [Browsable(false)]
        public List<InterpolatorEntry> Entries
        {
            get { return entries; }
        }

        public Interpolator()
        {
            Reset();
        }

        public void Reset()
        {
            entries = new List<InterpolatorEntry>();

            InterpolatorEntry default_entry = new InterpolatorEntry();
            default_entry.Keyframe = 0;
            default_entry.Value = new MinMaxField(GetDefaultValueForParameter(parameter));
            entries.Add(default_entry);
        }

        [Browsable(false)]
        public bool IsDefault
        {
            get
            {
                return entries.Count == 1 && !entries[0].Value.IsRandom &&
                    entries[0].Value.Min == GetDefaultValueForParameter(parameter);
            }
        }

        public object Serialize()
        {
            Hashtable interpolator_data = new Hashtable();
            interpolator_data["interpolate_on"] = interpolate_on.ToString();
            interpolator_data["parameter"] = parameter.ToString();

            ArrayList serialized_entries = new ArrayList();
            foreach (InterpolatorEntry entry in entries)
            {
                serialized_entries.Add(entry.Serialize());
            }
            interpolator_data["entries"] = serialized_entries;

            return interpolator_data;
        }

        public override string ToString()
        {
            return parameter.ToString();
        }

        public static Interpolator Deserialize(object o)
        {
            Hashtable interpolator_data = (Hashtable)o;
            Interpolator interpolator = new Interpolator();

            object interpolate_on = interpolator_data["interpolate_on"];
            if (interpolate_on is string)
            {
                Enum.TryParse<InterpolatorType>((string)interpolate_on, true, out interpolator.interpolate_on);
            }
            else
            {
                interpolator.interpolate_on = (InterpolatorType)(int)(double)interpolator_data["interpolate_on"];
            }

            object parameter = interpolator_data["parameter"];
            if (parameter is string)
            {
                Enum.TryParse<ModelParameterType>((string)parameter, true, out interpolator.parameter);
            }
            else
            {
                interpolator.parameter = (ModelParameterType)(int)(double)interpolator_data["parameter"];
            }

            ArrayList serialized_entries = (ArrayList)interpolator_data["entries"];
            interpolator.entries.Clear();
            foreach (object entry in serialized_entries)
            {
                interpolator.entries.Add(InterpolatorEntry.Deserialize(entry));
            }

            return interpolator;
        }
    }
}
