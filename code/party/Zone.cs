using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace party
{
    public enum ZoneType
    {
        AABB,
        Cylinder,
        [Browsable(false)]
        Line,
        Plane,
        Point,
        Ring,
        Sphere
    }

    public class ZoneTypeChangedArgs : EventArgs
    {
        ZoneType new_type;

        public ZoneTypeChangedArgs(ZoneType value)
        {
            new_type = value;
        }

        public ZoneType NewType
        {
            get { return new_type; }
        }
    }

    public class ZoneGroup
    {
        public event EventHandler Changed;

        Zone current;
        public Zone Zone
        {
            get { return current; }
            set
            {
                if (zones[(int)value.Type] != value)
                {
                    OnLeave(zones[(int)value.Type]);
                    zones[(int)value.Type] = value;
                    OnJoin(value);
                }
                current = value;
            }
        }

        Zone[] zones;
        public Zone this[ZoneType type]
        {
            get { return zones[(int)type]; }
            set
            {
                Debug.Assert(value.Type == type);
                if (zones[(int)type] != value)
                {
                    OnLeave(zones[(int)type]);
                    if (current == zones[(int)type]) current = value;
                    zones[(int)type] = value;
                    OnJoin(value);
                }
            }
        }

        public ZoneGroup()
        {
            zones = new Zone[7];
            zones[(int)ZoneType.AABB] = new AABBZone();
            zones[(int)ZoneType.Cylinder] = new CylinderZone();
            zones[(int)ZoneType.Line] = null;
            zones[(int)ZoneType.Plane] = new PlaneZone();
            zones[(int)ZoneType.Point] = new PointZone();
            zones[(int)ZoneType.Ring] = new RingZone();
            zones[(int)ZoneType.Sphere] = new SphereZone();
            current = zones[(int)ZoneType.Point];

            foreach (Zone zone in zones)
            {
                if (zone == null) continue;
                OnJoin(zone);
            }
        }

        public object Serialize(bool include_editor_data)
        {
            ArrayList zone_list = new ArrayList();
            foreach (Zone zone in zones)
            {
                if (zone == null) zone_list.Add(null);
                else zone_list.Add(zone.Serialize());
            }

            Hashtable zone_group = new Hashtable();
            zone_group["current"] = Array.IndexOf(zones, current);
            zone_group["zones"] = zone_list;
            return zone_group;
        }

        public void Deserialize(object o)
        {
            Hashtable zone_group = (Hashtable)o;
            ArrayList zone_list = (ArrayList)zone_group["zones"];
            foreach (Hashtable zone_data in zone_list)
            {
                if (zone_data == null) continue;
                int type = (int)(double)zone_data["type"];
                zones[type].Deserialize(zone_data);
            }
            current = zones[(int)(double)zone_group["current"]];
        }

        void OnJoin(Zone zone)
        {
            zone.TypeChanged += zone_TypeChanged;
            zone.Changed += zone_Changed;
        }

        void OnLeave(Zone zone)
        {
            zone.TypeChanged -= zone_TypeChanged;
            zone.Changed -= zone_Changed;
        }

        void zone_TypeChanged(object sender, ZoneTypeChangedArgs e)
        {
            Zone new_zone = zones[(int)e.NewType];
            new_zone.CopySupportedValues(current);
            current = new_zone;

            if (Changed != null) Changed(current, e);
        }

        void zone_Changed(object sender, EventArgs e)
        {
            if (Changed != null) Changed(sender, e);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class Zone
    {
        public event EventHandler<ZoneTypeChangedArgs> TypeChanged;
        public event EventHandler Changed;

        [RefreshProperties(RefreshProperties.All)]
        abstract public ZoneType Type
        {
            get;
            set;
        }

        Vector3 position;
        [DefaultValue(typeof(Vector3), "0, 0, 0")]
        public Vector3 Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    position = value;
                    OnChanged();
                }
            }
        }

        public virtual void CopySupportedValues(Zone other)
        {
            position = other.position;
        }

        public virtual object Serialize()
        {
            Hashtable zone = new Hashtable();
            zone["type"] = Type.ToString();
            zone["position"] = position.Serialize();
            return zone;
        }

        public virtual void Deserialize(Hashtable zone)
        {
            position = Vector3.Deserialize(zone["position"]);
        }

        public static Zone Deserialize(object o)
        {
            Hashtable zone_data = (Hashtable)o;

            Zone zone = null;

            ZoneType type;
            if (zone_data["type"] is string) type = (ZoneType)Enum.Parse(typeof(ZoneType), (string)zone_data["type"], true);
            else type = (ZoneType)(int)(double)zone_data["type"];

            switch (type)
            {
                case ZoneType.AABB:
                    zone = new AABBZone();
                    break;
                case ZoneType.Cylinder:
                    zone = new CylinderZone();
                    break;
                case ZoneType.Plane:
                    zone = new PlaneZone();
                    break;
                case ZoneType.Point:
                    zone = new PointZone();
                    break;
                case ZoneType.Ring:
                    zone = new RingZone();
                    break;
                case ZoneType.Sphere:
                    zone = new SphereZone();
                    break;
            }

            if (zone != null) zone.Deserialize(zone_data);
            return zone;
        }

        protected void OnTypeChanged(ZoneType new_type)
        {
            if (TypeChanged != null) TypeChanged(this, new ZoneTypeChangedArgs(new_type));
        }

        protected void OnChanged()
        {
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
    }

    public class AABBZone : Zone
    {
        public override ZoneType Type
        {
            get { return ZoneType.AABB; }
            set
            {
                if (value != ZoneType.AABB) OnTypeChanged(value);
            }
        }

        Vector3 extents;
        [DefaultValue(typeof(Vector3), "0, 0, 0")]
        public Vector3 Extents
        {
            get { return extents; }
            set
            {
                if (extents != value)
                {
                    extents = value;
                    OnChanged();
                }
            }
        }

        public override object Serialize()
        {
            Hashtable zone = (Hashtable)base.Serialize();
            zone["extents"] = extents.Serialize();
            return zone;
        }

        public override void Deserialize(Hashtable zone)
        {
            base.Deserialize(zone);
            extents = Vector3.Deserialize(zone["extents"]);
        }
    }

    public class CylinderZone : Zone
    {
        public override ZoneType Type
        {
            get { return ZoneType.Cylinder; }
            set
            {
                if (value != ZoneType.Cylinder) OnTypeChanged(value);
            }
        }

        Vector3 direction = new Vector3(0, 1, 0);
        [DefaultValue(typeof(Vector3), "0, 1, 0")]
        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                if (direction != value)
                {
                    direction = value;
                    OnChanged();
                }
            }
        }

        float radius = 1.0f;
        [DefaultValue(1.0f)]
        public float Radius
        {
            get { return radius; }
            set
            {
                if (radius != value)
                {
                    radius = value;
                    OnChanged();
                }
            }
        }

        float length = 1.0f;
        [DefaultValue(1.0f)]
        public float Length
        {
            get { return length; }
            set
            {
                if (length != value)
                {
                    length = value;
                    OnChanged();
                }
            }
        }

        public override void CopySupportedValues(Zone other)
        {
            base.CopySupportedValues(other);
            if (other.Type == ZoneType.Ring)
            {
                direction = ((RingZone)other).Direction;
            }
            else if (other.Type == ZoneType.Plane)
            {
                direction = ((PlaneZone)other).Direction;
            }
        }

        public override object Serialize()
        {
            Hashtable zone = (Hashtable)base.Serialize();
            zone["direction"] = direction.Serialize();
            zone["radius"] = radius;
            zone["length"] = length;
            return zone;
        }

        public override void Deserialize(Hashtable zone)
        {
            base.Deserialize(zone);
            direction = Vector3.Deserialize(zone["direction"]);
            radius = (float)(double)zone["radius"];
            length = (float)(double)zone["length"];
        }
    }

    public class PlaneZone : Zone
    {
        public override ZoneType Type
        {
            get { return ZoneType.Plane; }
            set
            {
                if (value != ZoneType.Plane) OnTypeChanged(value);
            }
        }

        Vector3 direction = new Vector3(0, 1, 0);
        [DefaultValue(typeof(Vector3), "0, 1, 0")]
        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                if (direction != value)
                {
                    direction = value;
                    OnChanged();
                }
            }
        }

        public override void CopySupportedValues(Zone other)
        {
            base.CopySupportedValues(other);
            if (other.Type == ZoneType.Ring)
            {
                direction = ((RingZone)other).Direction;
            }
            else if (other.Type == ZoneType.Cylinder)
            {
                direction = ((CylinderZone)other).Direction;
            }
        }

        public override object Serialize()
        {
            Hashtable zone = (Hashtable)base.Serialize();
            zone["direction"] = direction.Serialize();
            return zone;
        }

        public override void Deserialize(Hashtable zone)
        {
            base.Deserialize(zone);
            direction = Vector3.Deserialize(zone["direction"]);
        }
    }

    public class PointZone : Zone
    {
        public override ZoneType Type
        {
            get { return ZoneType.Point; }
            set
            {
                if (value != ZoneType.Point) OnTypeChanged(value);
            }
        }
    }

    public class RingZone : Zone
    {
        public override ZoneType Type
        {
            get { return ZoneType.Ring; }
            set
            {
                if (value != ZoneType.Ring) OnTypeChanged(value);
            }
        }

        Vector3 direction = new Vector3(0, 1, 0);
        [DefaultValue(typeof(Vector3), "0, 1, 0")]
        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                if (direction != value)
                {
                    direction = value;
                    OnChanged();
                }
            }
        }

        float inner_radius = 0;
        [DefaultValue(0.0f)]
        public float InnerRadius
        {
            get { return inner_radius; }
            set
            {
                if (inner_radius != value)
                {
                    inner_radius = value;
                    OnChanged();
                }
            }
        }

        float outer_radius = 1;
        [DefaultValue(1.0f)]
        public float OuterRadius
        {
            get { return outer_radius; }
            set
            {
                if (outer_radius != value)
                {
                    outer_radius = value;
                    OnChanged();
                }
            }
        }

        public override void CopySupportedValues(Zone other)
        {
            base.CopySupportedValues(other);
            if (other.Type == ZoneType.Cylinder)
            {
                direction = ((CylinderZone)other).Direction;
            }
            else if (other.Type == ZoneType.Plane)
            {
                direction = ((PlaneZone)other).Direction;
            }
        }

        public override object Serialize()
        {
            Hashtable zone = (Hashtable)base.Serialize();
            zone["direction"] = direction.Serialize();
            zone["inner_radius"] = inner_radius;
            zone["outer_radius"] = outer_radius;
            return zone;
        }

        public override void Deserialize(Hashtable zone)
        {
            base.Deserialize(zone);
            direction = Vector3.Deserialize(zone["direction"]);
            inner_radius = (float)(double)zone["inner_radius"];
            outer_radius = (float)(double)zone["outer_radius"];
        }
    }

    public class SphereZone : Zone
    {
        public override ZoneType Type
        {
            get { return ZoneType.Sphere; }
            set
            {
                if (value != ZoneType.Sphere) OnTypeChanged(value);
            }
        }

        float radius = 1;
        [DefaultValue(1.0f)]
        public float Radius
        {
            get { return radius; }
            set
            {
                if (radius != value)
                {
                    radius = value;
                    OnChanged();
                }
            }
        }

        public override object Serialize()
        {
            Hashtable zone = (Hashtable)base.Serialize();
            zone["radius"] = radius;
            return zone;
        }

        public override void Deserialize(Hashtable zone)
        {
            base.Deserialize(zone);
            radius = (float)(double)zone["radius"];
        }
    }

}
