using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace party
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVector3
    {
        public float x, y, z;

        public NativeVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    class Vector3Converter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(Vector3))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is Vector3)
            {
                Vector3 vector = (Vector3)value;
                return string.Format("{0}, {1}, {2}", vector.X, vector.Y, vector.Z);
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
                try
                {
                    string[] parts = ((string)value).Split(',');
                    Vector3 vector = new Vector3();
                    vector.X = float.Parse(parts[0]);
                    vector.Y = float.Parse(parts[1]);
                    vector.Z = float.Parse(parts[2]);
                    return vector;
                }
                catch { }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    [TypeConverter(typeof(Vector3Converter))]
    public struct Vector3
    {
        float x, y, z;

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

        [NotifyParentProperty(true)]
        public float Z
        {
            get { return z; }
            set { z = value; }
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator NativeVector3(Vector3 v)
        {
            return new NativeVector3(v.x, v.y, v.z);
        }

        public static bool operator == (Vector3 left, Vector3 right)
        {
            return (left.x == right.x) && (left.y == right.y) && (left.z == right.z);
        }

        public static bool operator != (Vector3 left, Vector3 right)
        {
            return (left.x != right.x) || (left.y != right.y) || (left.z != right.z); 
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            return (Vector3)obj == this;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() * y.GetHashCode() * z.GetHashCode();
        }

        public object Serialize()
        {
            ArrayList vector = new ArrayList();
            vector.Add(x);
            vector.Add(y);
            vector.Add(z);
            return vector;
        }

        public static Vector3 Deserialize(object o)
        {
            ArrayList vector = (ArrayList)o;
            return new Vector3((float)(double)vector[0], (float)(double)vector[1], (float)(double)vector[2]);
        }
    }
}
