using System;
using System.ComponentModel;

namespace party
{
    class Vector2iConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(Vector2i))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is Vector2i)
            {
                Vector2i vector = (Vector2i)value;
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
                    return new Vector2i();
                }
                try
                {
                    string[] parts = ((string)value).Split(',');
                    Vector2i vector = new Vector2i();
                    vector.X = int.Parse(parts[0]);
                    vector.Y = int.Parse(parts[1]);
                    return vector;
                }
                catch { }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    [TypeConverter(typeof(Vector2iConverter))]
    public struct Vector2i
    {
        int x, y;

        [NotifyParentProperty(true)]
        public int X
        {
            get { return x; }
            set { x = value; }
        }

        [NotifyParentProperty(true)]
        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public Vector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
