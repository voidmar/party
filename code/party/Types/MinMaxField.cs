using System;
using System.Collections;
using System.ComponentModel;

namespace party
{
    class MinMaxFieldConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(MinMaxField))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is MinMaxField)
            {
                MinMaxField min_max = (MinMaxField)value;
                if (min_max.Min == min_max.Max) return string.Format("{0}", min_max.Min);
                else return string.Format("{0}, {1}", min_max.Min, min_max.Max);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] parts = ((string)value).Split(',');
                if (parts.Length == 1)
                {
                    MinMaxField min_max = new MinMaxField();
                    min_max.Min = min_max.Max = float.Parse(parts[0]);
                    return min_max;
                }
                else if (parts.Length >= 1)
                {
                    MinMaxField min_max = new MinMaxField();
                    min_max.Min = float.Parse(parts[0]);
                    min_max.Max = float.Parse(parts[1]);
                    return min_max;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    [TypeConverter(typeof(MinMaxFieldConverter))]
    public struct MinMaxField
    {
        float min, max;

        public MinMaxField(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public MinMaxField(float value)
        {
            min = max = value;
        }

        [NotifyParentProperty(true)]
        public float Min
        {
            get { return min; }
            set { min = value; }
        }

        [NotifyParentProperty(true)]
        public float Max
        {
            get { return max; }
            set { max = value; }
        }

        [Browsable(false)]
        public bool IsRandom
        {
            get
            {
                return Min != Max;
            }
        }

        public bool Normalize()
        {
            float new_max = Math.Max(min, max);
            float new_min = Math.Min(min, max);
            bool needed_normalize = max != new_max;
            max = new_max;
            min = new_min;
            return needed_normalize;
        }

        public static bool operator ==(MinMaxField left, MinMaxField right)
        {
            return (left.Min == right.Min) && (left.Max == right.Max);
        }

        public static bool operator ==(MinMaxField left, float right)
        {
            return (left.Min == right) && (left.Max == right);
        }

        public static bool operator !=(MinMaxField left, MinMaxField right)
        {
            return (left.Min != right.Min) || (left.Max != right.Max);
        }

        public static bool operator !=(MinMaxField left, float right)
        {
            return (left.Min != right) || (left.Max != right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(GetType() == obj.GetType() || obj.GetType() == typeof(float))) return false;
            return (MinMaxField)obj == this;
        }

        public override int GetHashCode()
        {
            return Min.GetHashCode() * Max.GetHashCode();
        }

        public object Serialize()
        {
            ArrayList min_max = new ArrayList();
            min_max.Add(min);
            min_max.Add(max);
            return min_max;
        }

        public static MinMaxField Deserialize(object o)
        {
            ArrayList min_max = (ArrayList)o;
            return new MinMaxField((float)(double)min_max[0], (float)(double)min_max[1]);
        }

        public static MinMaxField Lerp(MinMaxField a, MinMaxField b, float alpha)
        {
            MinMaxField value = new MinMaxField();
            value.Min = a.Min + (b.Min - a.Min) * alpha;
            value.Max = a.Max + (b.Max - a.Max) * alpha;
            return value;
        }
    }
}
