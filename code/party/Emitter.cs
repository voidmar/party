using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;

namespace party
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Burst
    {
        public float Time
        {
            get;
            set;
        }

        public int Tank
        {
            get;
            set;
        }
    }

    [Editor(typeof(CustomCollectionEditor), typeof(UITypeEditor))]
    public class BurstList : List<Burst>, ICareAboutChanges
    {
        public event EventHandler Changed;

        public void NotifyChanged()
        {
            Sort((a, b) => a.Time.CompareTo(b.Time));

            if (Changed != null) Changed(this, EventArgs.Empty);
        }

        public object Serialize()
        {
            ArrayList bursts = new ArrayList();
            foreach (Burst burst in this)
            {
                bursts.Add(burst.Time);
                bursts.Add(burst.Tank);
            }
            return bursts;
        }

        public void Deserialize(object o)
        {
            Clear();

            ArrayList bursts = (ArrayList)o;
            for (int i = 0; i < bursts.Count; i += 2)
            {
                Burst burst = new Burst();
                burst.Time = (float)(double)bursts[i];
                burst.Tank = (int)(double)bursts[i + 1];
                Add(burst);
            }
        }
    }

    public enum EmitterType
    {
        Normal,
        Random,
        Spheric,
        Static,
        Straight,
    }

    public class EmitterTypeChangedArgs : EventArgs
    {
        EmitterType new_type;

        public EmitterTypeChangedArgs(EmitterType value)
        {
            new_type = value;
        }

        public EmitterType NewType
        {
            get { return new_type; }
        }
    }

    public class EmitterGroup
    {
        public event EventHandler EmitterChanged;
        public event EventHandler EmitterZoneChanged;

        Emitter current;
        public Emitter Emitter
        {
            get { return current; }
            set
            {
                if (emitters[(int)value.Type] != value)
                {
                    OnLeave(emitters[(int)value.Type]);
                    emitters[(int)value.Type] = value;
                    OnJoin(value);
                }
                current = value;
            }
        }

        Emitter[] emitters;
        public Emitter this[EmitterType type]
        {
            get { return emitters[(int)type]; }
            set
            {
                Debug.Assert(value.Type == type);
                if (emitters[(int)type] != value)
                {
                    OnLeave(emitters[(int)type]);
                    if (current == emitters[(int)type]) current = value;
                    emitters[(int)type] = value;
                    OnJoin(value);
                }
            }
        }

        public EmitterGroup()
        {
            emitters = new Emitter[5];
            emitters[(int)EmitterType.Normal] = new NormalEmitter();
            emitters[(int)EmitterType.Random] = new RandomEmitter();
            emitters[(int)EmitterType.Spheric] = new SphericEmitter();
            emitters[(int)EmitterType.Static] = new StaticEmitter();
            emitters[(int)EmitterType.Straight] = new StraightEmitter();
            current = emitters[(int)EmitterType.Random];

            foreach (Emitter emitter in emitters)
            {
                if (emitter == null) continue;
                OnJoin(emitter);
            }
        }

        public object Serialize(bool include_editor_data)
        {
            ArrayList emitter_list = new ArrayList();
            foreach (Emitter emitter in emitters)
            {
                if (emitter == null) emitter_list.Add(null);
                else emitter_list.Add(emitter.Serialize(include_editor_data));
            }

            Hashtable emitter_group = new Hashtable();
            emitter_group["current"] = Array.IndexOf(emitters, current);
            emitter_group["emitters"] = emitter_list;
            return emitter_group;
        }

        public void Deserialize(object o)
        {
            Hashtable emitter_group = (Hashtable)o;
            ArrayList emitter_list = (ArrayList)emitter_group["emitters"];
            foreach (Hashtable emitter_data in emitter_list)
            {
                int type;

                if (emitter_data["type"] is string) type = (int)Enum.Parse(typeof(EmitterType), (string)emitter_data["type"], true);
                else type = (int)(double)emitter_data["type"];

                emitters[type].Deserialize(emitter_data);
            }
            current = emitters[(int)(double)emitter_group["current"]];
        }

        void OnJoin(Emitter emitter)
        {
            emitter.TypeChanged += emitter_TypeChanged;
            emitter.Changed += emitter_Changed;
            emitter.ZoneChanged += emitter_ZoneChanged;
        }

        void OnLeave(Emitter emitter)
        {
            emitter.TypeChanged -= emitter_TypeChanged;
            emitter.Changed -= emitter_Changed;
            emitter.ZoneChanged -= emitter_ZoneChanged;
        }

        void emitter_TypeChanged(object sender, EmitterTypeChangedArgs e)
        {
            Emitter new_emitter = emitters[(int)e.NewType];
            new_emitter.CopySupportedValues(current);
            current = new_emitter;

            if (EmitterChanged != null) EmitterChanged(current, e);
        }

        void emitter_Changed(object sender, EventArgs e)
        {
            if (EmitterChanged != null) EmitterChanged(sender, e);
        }

        void emitter_ZoneChanged(object sender, EventArgs e)
        {
            if (EmitterZoneChanged != null) EmitterZoneChanged(sender, e);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class Emitter
    {
        public event EventHandler<EmitterTypeChangedArgs> TypeChanged;
        public event EventHandler Changed;
        public event EventHandler ZoneChanged;

        float flow;
        public float Flow
        {
            get { return flow; }
            set
            {
                if (flow != value)
                {
                    flow = value;
                    OnChanged();
                }
            }
        }

        MinMaxField force;
        [DefaultValue(typeof(MinMaxField), "0")]
        public MinMaxField Force
        {
            get { return force; }
            set
            {
                if (force != value)
                {
                    force = value;
                    OnChanged();
                }
            }
        }

        int tank = -1;
        [DefaultValue(-1)]
        public int Tank
        {
            get { return tank; }
            set
            {
                if (tank != value)
                {
                    tank = value;
                    OnChanged();
                }
            }
        }

        BurstList burst_list = new BurstList();
        public BurstList BurstList
        {
            get { return burst_list; }
        }

        bool use_full_zone = true;
        [DefaultValue(true)]
        public bool UseFullZone
        {
            get { return use_full_zone; }
            set
            {
                if (use_full_zone != value)
                {
                    use_full_zone = value;
                    OnChanged();
                }
            }
        }

        ZoneGroup zone_group;
        public Zone Zone
        {
            get { return zone_group.Zone; }
        }

        [RefreshProperties(RefreshProperties.All)]
        abstract public EmitterType Type
        {
            get;
            set;
        }

        protected void OnTypeChanged(EmitterType new_type)
        {
            if (TypeChanged != null) TypeChanged(this, new EmitterTypeChangedArgs(new_type));
        }

        protected void OnChanged()
        {
            if (Changed != null) Changed(this, EventArgs.Empty);
        }

        public Emitter()
        {
            zone_group = new ZoneGroup();
            zone_group.Changed += new EventHandler(zone_group_Changed);
            burst_list.Changed += new EventHandler(burst_list_Changed);
        }

        public virtual void CopySupportedValues(Emitter other)
        {
            flow = other.flow;
            force = other.force;
            tank = other.tank;
            burst_list.Clear();
            burst_list.AddRange(other.burst_list);
            use_full_zone = other.use_full_zone;
            zone_group = other.zone_group;
        }

        public virtual object Serialize(bool include_editor_data)
        {
            Hashtable emitter = new Hashtable();
            emitter["type"] = Type.ToString();
            emitter["flow"] = flow;
            emitter["tank"] = tank;
            if (burst_list.Count > 0) emitter["bursts"] = burst_list.Serialize();
            emitter["force"] = force.Serialize();
            emitter["use_full_zone"] = use_full_zone;
            emitter["zone"] = zone_group.Zone.Serialize();

            if (include_editor_data)
            {
                emitter["zone_group"] = zone_group.Serialize(include_editor_data);
            }

            return emitter;
        }

        public virtual void Deserialize(Hashtable emitter)
        {
            flow = (float)(double)emitter["flow"];
            tank = (int)(double)emitter["tank"];
            if (emitter.ContainsKey("bursts")) burst_list.Deserialize(emitter["bursts"]);
            force = MinMaxField.Deserialize(emitter["force"]);
            use_full_zone = (bool)emitter["use_full_zone"];

            if (emitter.ContainsKey("zone_group"))
            {
                zone_group.Deserialize(emitter["zone_group"]);
            }
            else
            {
                zone_group.Zone = Zone.Deserialize(emitter["zone"]);
            }
        }

        public static Emitter Deserialize(object o)
        {
            Hashtable emitter_data = (Hashtable)o;


            EmitterType type;
            if (emitter_data["type"] is string) type = (EmitterType)Enum.Parse(typeof(EmitterType), (string)emitter_data["type"], true);
            else type = (EmitterType)(int)(double)emitter_data["type"];

            Emitter emitter = null;
            switch (type)
            {
                case EmitterType.Normal:
                    emitter = new NormalEmitter();
                    break;
                case EmitterType.Random:
                    emitter = new RandomEmitter();
                    break;
                case EmitterType.Spheric:
                    emitter = new SphericEmitter();
                    break;
                case EmitterType.Static:
                    emitter = new StaticEmitter();
                    break;
                case EmitterType.Straight:
                    emitter = new StraightEmitter();
                    break;
            }

            emitter.Deserialize(emitter_data);
            return emitter;
        }

        void burst_list_Changed(object sender, EventArgs e)
        {
            OnChanged();
        }

        void zone_group_Changed(object sender, EventArgs e)
        {
            if (ZoneChanged != null) ZoneChanged(sender, e);
        }
    }

    public class NormalEmitter : Emitter
    {
        public override EmitterType Type
        {
            get { return EmitterType.Normal; }
            set
            {
                if (value != EmitterType.Normal) OnTypeChanged(value);
            }
        }

        bool inverted = false;

        [DefaultValue(false)]
        public bool Inverted
        {
            get { return inverted; }
            set
            {
                if (inverted != value)
                {
                    inverted = value;
                    OnChanged();
                }
            }
        }

        public override object Serialize(bool include_editor_data)
        {
            Hashtable emitter = (Hashtable)base.Serialize(include_editor_data);
            emitter["inverted"] = inverted;
            return emitter;
        }

        public override void Deserialize(Hashtable emitter)
        {
            base.Deserialize(emitter);
            if (emitter.ContainsKey("inverted"))
            {
                inverted = (bool)emitter["inverted"];
            }
        }
    }

    public class RandomEmitter : Emitter
    {
        public override EmitterType Type
        {
            get { return EmitterType.Random; }
            set
            {
                if (value != EmitterType.Random) OnTypeChanged(value);
            }
        }
    }

    public class SphericEmitter : Emitter
    {
        public override EmitterType Type
        {
            get { return EmitterType.Spheric; }
            set
            {
                if (value != EmitterType.Spheric) OnTypeChanged(value);
            }
        }

        Vector3 direction = new Vector3(0, 1, 0);
        MinMaxField angles;

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

        [DefaultValue(typeof(MinMaxField), "0")]
        public MinMaxField Angles
        {
            get { return angles; }
            set
            {
                if (angles != value)
                {
                    angles = value;
                    OnChanged();
                }
            }
        }

        public override void CopySupportedValues(Emitter other)
        {
            base.CopySupportedValues(other);
            if (other.Type == EmitterType.Straight)
            {
                direction = ((StraightEmitter)other).Direction;
            }
        }

        public override object Serialize(bool include_editor_data)
        {
            Hashtable emitter = (Hashtable)base.Serialize(include_editor_data);
            emitter["direction"] = direction.Serialize();
            emitter["angles"] = angles.Serialize();
            return emitter;
        }

        public override void Deserialize(Hashtable emitter)
        {
            base.Deserialize(emitter);
            direction = Vector3.Deserialize(emitter["direction"]);
            angles = MinMaxField.Deserialize(emitter["angles"]);
        }
    }

    public class StaticEmitter : Emitter
    {
        public override EmitterType Type
        {
            get { return EmitterType.Static; }
            set
            {
                if (value != EmitterType.Static) OnTypeChanged(value);
            }
        }
    }

    public class StraightEmitter : Emitter
    {
        public override EmitterType Type
        {
            get { return EmitterType.Straight; }
            set
            {
                if (value != EmitterType.Straight) OnTypeChanged(value);
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

        public override void CopySupportedValues(Emitter other)
        {
            base.CopySupportedValues(other);
            if (other.Type == EmitterType.Spheric)
            {
                direction = ((SphericEmitter)other).Direction;
            }
        }

        public override object Serialize(bool include_editor_data)
        {
            Hashtable emitter = (Hashtable)base.Serialize(include_editor_data);
            emitter["direction"] = direction.Serialize();
            return emitter;
        }

        public override void Deserialize(Hashtable emitter)
        {
            base.Deserialize(emitter);
            direction = Vector3.Deserialize(emitter["direction"]);
        }
    }
}

