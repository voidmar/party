using System;
using System.Collections;
using System.ComponentModel;

namespace party
{
    class Vector2Converter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(Vector2))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is Vector2)
            {
                Vector2 vector = (Vector2)value;
                return string.Format("{0}, {1}", vector.X, vector.Y);
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
                if (((string)value).Trim().Length == 0)
                {
                    return new Vector2();
                }
                try
                {
                    string[] parts = ((string)value).Split(',');
                    Vector2 vector = new Vector2();
                    vector.X = float.Parse(parts[0]);
                    vector.Y = float.Parse(parts[1]);
                    return vector;
                }
                catch { }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    [TypeConverter(typeof(Vector2Converter))]
    public struct Vector2
    {
        float x, y;

        [NotifyParentProperty(true)]
        public float X
        {
            get { return x; }
            set { x = value; }
        }

        [NotifyParentProperty(true)]
        public float Y
        {
            get { return y; }
            set { y = value; }
        }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public object Serialize()
        {
            ArrayList vector = new ArrayList();
            vector.Add(x);
            vector.Add(y);
            return vector;
        }

        public static Vector2 Deserialize(object o)
        {
            ArrayList vector = (ArrayList)o;
            return new Vector2((float)(double)vector[0], (float)(double)vector[1]);
        }
    }
}
