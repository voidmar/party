using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace party
{
    public enum BlendingMode
    {
        Add = 1,
        Alpha = 2,
        AddMaskR = 3,
        AddMaskG = 4,
        AddMaskB = 5,
        AddMaskA = 6,
        AlphaMaskR = 7,
        AlphaMaskG = 8,
        AlphaMaskB = 9,
        AlphaMaskA = 10,
    }

    public class ParticleGroup : IDisposable
    {
        public static List<ParticleGroup> LiveGroups = new List<ParticleGroup>();

        public ParticleGroup()
        {
            native_group = PartyAPI.CreateParticleGroup();

            emitter_group = new EmitterGroup();
            emitter_group.EmitterChanged += new EventHandler(emitter_group_EmitterChanged);
            emitter_group.EmitterZoneChanged += new EventHandler(emitter_group_EmitterZoneChanged);

            interpolator_group = new InterpolatorGroup();
            interpolator_group.Changed += new EventHandler(interpolator_group_Changed);

            LiveGroups.Add(this);
        }

        void emitter_group_EmitterChanged(object sender, EventArgs e)
        {
            UpdateEmitter((Emitter)sender);
        }

        void emitter_group_EmitterZoneChanged(object sender, EventArgs e)
        {
            UpdateEmitterZone((Zone)sender);
        }

        void interpolator_group_Changed(object sender, EventArgs e)
        {
            UpdateParameter((Interpolator)sender);
        }

        ~ParticleGroup()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return name;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                LiveGroups.Remove(this);
                PartyAPI.DestroyParticleGroup(native_group);
                native_group = IntPtr.Zero;

                disposed = true;
            }
        }

        public object Serialize(bool include_editor_data = false)
        {
            Hashtable group = new Hashtable();
            group["name"] = name;
            group["enabled"] = enabled;
            group["blending_mode"] = blending_mode.ToString();

            string serialized_texture_path = texture_path;
            if (serialized_texture_path == null) serialized_texture_path = "";
            if (serialized_texture_path.StartsWith(".\\"))
            {
                serialized_texture_path = texture_path.Substring(2);
            }
            serialized_texture_path = serialized_texture_path.Replace('\\', '/');

            group["texture_path"] = serialized_texture_path;
            group["texture_slices_x"] = slices.X;
            group["texture_slices_y"] = slices.Y;
            group["scale"] = scale.Serialize();
            group["parameters"] = interpolator_group.Serialize();
            group["emitter"] = emitter_group.Emitter.Serialize(include_editor_data);
            group["lifetime"] = lifetime.Serialize();
            group["friction"] = friction;
            group["gravity"] = gravity.Serialize();

            return group;
        }

        public void Deserialize(object o)
        {
            Hashtable group = (Hashtable)o;
            name = (string)group["name"];

            if (group["blending_mode"] is string)
            {
                BlendingMode = (BlendingMode)Enum.Parse(typeof(BlendingMode), (string)group["blending_mode"], true);
            }
            else BlendingMode = (BlendingMode)(double)group["blending_mode"];

            string new_texture_path = ((string)group["texture_path"]).Replace('/', '\\');
            if (!new_texture_path.StartsWith(".\\")) new_texture_path = ".\\" + new_texture_path;
            TexturePath = new_texture_path;

            if (group.ContainsKey("texture_slices_x"))
            {
                slices.X = (int)(double)group["texture_slices_x"];
                slices.Y = (int)(double)group["texture_slices_y"];
                PartyAPI.SetRendererTextureSlices(native_group, slices.X, slices.Y);
            }

            if (group.ContainsKey("scale"))
            {
                Scale = Vector2.Deserialize(group["scale"]);
            }


            if (group.ContainsKey("friction")) Friction = (float)(double)group["friction"];
            if (group.ContainsKey("gravity")) Gravity = Vector3.Deserialize(group["gravity"]);

            if (group.ContainsKey("enabled")) Enabled = (bool)group["enabled"];

            if (group.ContainsKey("model"))
            {
                Hashtable model = (Hashtable)group["model"];
                Lifetime = MinMaxField.Deserialize(model["lifetime"]);
                interpolator_group = InterpolatorGroup.Deserialize(model["interpolators"]);

                ArrayList param_list = (ArrayList)model["parameters"];
                foreach (Hashtable param in param_list)
                {
                    ModelParameterType type = (ModelParameterType)(int)(double)param["type"];
                    Interpolator interpolator = interpolator_group.Interpolators.Find((a) => a.Parameter == type);
                    interpolator.InterpolateOn = InterpolatorType.Lifetime;

                    MinMaxField start = MinMaxField.Deserialize(param["start"]);
                    MinMaxField end = MinMaxField.Deserialize(param["end"]);

                    interpolator.Entries[0].Value = start;
                    interpolator.Entries[0].Random = start.IsRandom;
                    if (start != end)
                    {
                        InterpolatorEntry end_entry = new InterpolatorEntry();
                        end_entry.Keyframe = 1;
                        end_entry.Value = end;
                        end_entry.Random = end.IsRandom;
                        interpolator.Entries.Add(end_entry);
                    }
                }
            }
            else
            {
                Lifetime = MinMaxField.Deserialize(group["lifetime"]);
                interpolator_group = InterpolatorGroup.Deserialize(group["parameters"]);
            }

            interpolator_group.Changed += new EventHandler(interpolator_group_Changed);

            if (group.ContainsKey("emitter_group"))
            {
                emitter_group.Deserialize(group["emitter_group"]);
            }
            else
            {
                emitter_group.Emitter = Emitter.Deserialize(group["emitter"]);
            }

            UpdateEmitter(emitter_group.Emitter);
            UpdateEmitterZone(emitter_group.Emitter.Zone);
            UpdateParameters();
        }

        void UpdateEmitter(Emitter emitter)
        {
            NativeSharedEmitterProperties shared_properties;
            shared_properties.flow = emitter.Flow;
            shared_properties.tank = emitter.Tank;
            shared_properties.force_min = emitter.Force.Min;
            shared_properties.force_max = emitter.Force.Max;
            shared_properties.use_full_zone = emitter.UseFullZone;

            NativeVector3 direction;
            switch (emitter.Type)
            {
                case EmitterType.Normal:
                    PartyAPI.SetEmitterNormal(native_group, ref shared_properties);
                    break;
                case EmitterType.Random:
                    PartyAPI.SetEmitterRandom(native_group, ref shared_properties);
                    break;
                case EmitterType.Spheric:
                    SphericEmitter spheric_emitter = (SphericEmitter)emitter;
                    direction = spheric_emitter.Direction;
                    PartyAPI.SetEmitterSpheric(native_group, ref shared_properties, ref direction, spheric_emitter.Angles.Min, spheric_emitter.Angles.Max);
                    break;
                case EmitterType.Static:
                    PartyAPI.SetEmitterStatic(native_group, ref shared_properties);
                    break;
                case EmitterType.Straight:
                    StraightEmitter straight_emitter = (StraightEmitter)emitter;
                    direction = straight_emitter.Direction;
                    PartyAPI.SetEmitterStraight(native_group, ref shared_properties, ref direction);
                    break;
            }

            NativeBurst[] burst_list = new NativeBurst[emitter.BurstList.Count];
            for (int i = 0; i < emitter.BurstList.Count; ++i)
            {
                burst_list[i] = new NativeBurst();
                burst_list[i].time = emitter.BurstList[i].Time;
                burst_list[i].tank = emitter.BurstList[i].Tank;
            }

            PartyAPI.SetEmitterBurstList(native_group, burst_list, burst_list.Length);
        }

        void UpdateEmitterZone(Zone zone)
        {
            NativeVector3 position = zone.Position;
            NativeVector3 direction; // or "extents"
            switch (zone.Type)
            {
                case ZoneType.AABB:
                    AABBZone aabb_zone = (AABBZone)zone;
                    direction = aabb_zone.Extents;
                    PartyAPI.SetZoneAABB(native_group, ref position, ref direction);
                    break;
                case ZoneType.Cylinder:
                    CylinderZone cylinder_zone = (CylinderZone)zone;
                    direction = cylinder_zone.Direction;
                    PartyAPI.SetZoneCylinder(native_group, ref position, ref direction, cylinder_zone.Radius, cylinder_zone.Length);
                    break;
                case ZoneType.Plane:
                    PlaneZone plane_zone = (PlaneZone)zone;
                    direction = plane_zone.Direction;
                    PartyAPI.SetZonePlane(native_group, ref position, ref direction);
                    break;
                case ZoneType.Point:
                    PartyAPI.SetZonePoint(native_group, ref position);
                    break;
                case ZoneType.Ring:
                    RingZone ring_zone = (RingZone)zone;
                    direction = ring_zone.Direction;
                    PartyAPI.SetZoneRing(native_group, ref position, ref direction, ring_zone.OuterRadius, ring_zone.OuterRadius);
                    break;
                case ZoneType.Sphere:
                    SphereZone sphere_zone = (SphereZone)zone;
                    PartyAPI.SetZoneSphere(native_group, ref position, sphere_zone.Radius);
                    break;
            }
        }

        void UpdateParameters()
        {
            PartyAPI.BeginUpdate();
            foreach (Interpolator interpolator in interpolator_group.Interpolators)
            {
                UpdateParameter(interpolator);
            }
            PartyAPI.EndUpdate();
        }

        void UpdateParameter(Interpolator interpolator)
        {
            if (interpolator.Parameter == ModelParameterType.RotationSpeed)
            {
                bool angle_interpolated = interpolator_group[ModelParameterType.Angle].Entries.Count > 1;
                PartyAPI.EnableModelParameter(native_group, ModelParameterType.Angle, angle_interpolated);
                PartyAPI.EnableModelParameter(native_group, ModelParameterType.RotationSpeed, !angle_interpolated);
            }

            InterpolatorKeyframe[] keyframes = new InterpolatorKeyframe[interpolator.Entries.Count];
            for (int i = 0; i < keyframes.Length; ++i)
            {
                InterpolatorEntry entry = interpolator.Entries[i];
                keyframes[i].x = entry.Keyframe;
                keyframes[i].y0 = entry.Value.Min;
                keyframes[i].y1 = entry.Value.Max;
            }

            if (interpolator.Parameter == ModelParameterType.RotationSpeed || interpolator.Parameter == ModelParameterType.Angle)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    keyframes[i].y0 = (float)(keyframes[i].y0 * Math.PI / 180.0);
                    keyframes[i].y1 = (float)(keyframes[i].y1 * Math.PI / 180.0);
                }
            }

            PartyAPI.SetInterpolatorKeyframes(native_group, interpolator.Parameter, keyframes, keyframes.Length);
        }


        bool disposed;
        internal IntPtr native_group;

        string name = "Group";
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        bool enabled = true;
        [DefaultValue(true)]
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    PartyAPI.EnableParticleGroup(native_group, value);
                }
            }
        }

        string texture_path;
        [Editor(typeof(TextureFilenameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string TexturePath
        {
            get { return texture_path; }
            set
            {
                texture_path = value;
                PartyAPI.SetRendererTexture(native_group, value);
            }
        }

        Vector2i slices = new Vector2i(1, 1);
        [DefaultValue(typeof(Vector2i), "1, 1")]
        public Vector2i TexturesSlices
        {
            get { return slices; }
            set
            {
                slices = value;
                if (slices.X < 1) slices.X = 1;
                if (slices.Y < 1) slices.Y = 1;
                PartyAPI.SetRendererTextureSlices(native_group, slices.X, slices.Y);
            }
        }

        BlendingMode blending_mode = BlendingMode.Alpha;
        [DefaultValue(BlendingMode.Alpha)]
        public BlendingMode BlendingMode
        {
            get { return blending_mode; }
            set
            {
                blending_mode = value;
                PartyAPI.SetRendererBlendingMode(native_group, value);
            }
        }

        Vector2 scale = new Vector2(1, 1);
        [DefaultValue(typeof(Vector2), "1, 1")]
        public Vector2 Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                PartyAPI.SetRendererScale(native_group, scale.X, scale.Y);
            }
        }

        MinMaxField lifetime = new MinMaxField(1);
        [DefaultValue(typeof(MinMaxField), "1")]
        public MinMaxField Lifetime
        {
            get { return lifetime; }
            set
            {
                if (lifetime != value)
                {
                    lifetime = value;
                    PartyAPI.SetModelLifetime(native_group, lifetime.Min, lifetime.Max);
                }
            }
        }

        InterpolatorGroup interpolator_group;
        public InterpolatorGroup Parameters
        {
            get { return interpolator_group; }
        }

        EmitterGroup emitter_group;
        public Emitter Emitter
        {
            get { return emitter_group.Emitter; }
        }

        Vector3 gravity = new Vector3(0, 0, 0);
        [DefaultValue(typeof(Vector3), "0, 0, 0")]
        public Vector3 Gravity
        {
            get { return gravity; }
            set
            {
                gravity = value;
                NativeVector3 native_gravity = gravity;
                PartyAPI.SetGravity(native_group, ref native_gravity);
            }
        }

        float friction = 0;
        [DefaultValue(0.0f)]
        public float Friction
        {
            get { return friction; }
            set
            {
                friction = value;
                PartyAPI.SetFriction(native_group, friction);
            }
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeBurst
    {
        public float time;
        public int tank;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct NativeSharedEmitterProperties
    {
        public float flow;
        public int tank;
        public float force_min;
        public float force_max;
        public bool use_full_zone;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct InterpolatorKeyframe
    {
        public float x;
        public float y0, y1;
    }

    class PartyAPI
    {
        const string native_dll = "party_native.dll";
        static ManualResetEvent shutdown_event;
        static Thread render_thread;

        public static void Initialize(IntPtr hwnd)
        {
            shutdown_event = new ManualResetEvent(false);
            render_thread = new Thread(RenderThreadProc);
            render_thread.Start(hwnd);
        }

        public static void Shutdown()
        {
            shutdown_event.Set();
            render_thread.Join();
        }

        static void RenderThreadProc(object hwnd)
        {
            NativeInitialize((IntPtr)hwnd);
            for (; ; )
            {
                if (shutdown_event.WaitOne(TimeSpan.Zero)) break;

                PreviewProperties properties = PreviewWindow.Properties;
                if (properties.Dirty)
                {
                    properties.Dirty = false;
                    PartyAPI.SetMotion(properties.MotionEnabled, properties.MotionRadius, properties.MotionSpeed);

                    System.Drawing.Color color = properties.BackgroundColor;
                    PartyAPI.SetBackgroundColor(color.R, color.G, color.B);
                }

                if (PreviewWindow.CameraDirty)
                {
                    PreviewWindow.CameraDirty = false;
                    PartyAPI.SetCameraDistance(PreviewWindow.Distance);
                    PartyAPI.SetCameraRotation(PreviewWindow.Yaw, PreviewWindow.Pitch);
                }

                RenderLoop();
            }
            NativeShutdown();
        }

        [DllImport(native_dll, EntryPoint = "_PTInitialize@4")]
        extern static void NativeInitialize(IntPtr hwnd);

        [DllImport(native_dll, EntryPoint = "_PTShutdown@0")]
        extern static void NativeShutdown();

        [DllImport(native_dll, EntryPoint = "_PTResetView@0")]
        public extern static void ResetView();

        [DllImport(native_dll, EntryPoint = "_PTRenderLoop@0")]
        extern static void RenderLoop();

        [DllImport(native_dll, EntryPoint = "_PTSetCameraDistance@4")]
        internal extern static void SetCameraDistance(float distance);

        [DllImport(native_dll, EntryPoint = "_PTSetCameraRotation@8")]
        internal extern static void SetCameraRotation(float yaw, float pitch);

        [DllImport(native_dll, EntryPoint = "_PTSetGridEnabled@4")]
        internal extern static void SetGridEnabled(bool enabled);

        [DllImport(native_dll, EntryPoint = "_PTSetBackgroundColor@12")]
        extern static void SetBackgroundColor(int r, int g, int b);

        [DllImport(native_dll, EntryPoint = "_PTSetMotion@12")]
        extern static void SetMotion(bool enabled, float radius, float speed);


        [DllImport(native_dll, EntryPoint = "_PTCreateParticleGroup@0")]
        internal extern static IntPtr CreateParticleGroup();

        [DllImport(native_dll, EntryPoint = "_PTDestroyParticleGroup@4")]
        internal extern static void DestroyParticleGroup(IntPtr group);

        [DllImport(native_dll, EntryPoint = "_PTEnableParticleGroup@8")]
        internal extern static void EnableParticleGroup(IntPtr group, bool enabled);


        [DllImport(native_dll, EntryPoint = "_PTSetRendererBlendingMode@8")]
        internal extern static void SetRendererBlendingMode(IntPtr group, BlendingMode blending_mode);

        [DllImport(native_dll, EntryPoint = "_PTSetRendererTexture@8")]
        internal extern static void SetRendererTexture(IntPtr group, [MarshalAs(UnmanagedType.LPWStr)]string path);

        [DllImport(native_dll, EntryPoint = "_PTSetRendererTextureSlices@12")]
        internal extern static void SetRendererTextureSlices(IntPtr group, int slices_x, int slices_y);

        [DllImport(native_dll, EntryPoint = "_PTSetRendererScale@12")]
        internal extern static void SetRendererScale(IntPtr group, float scale_x, float scale_y);


        [DllImport(native_dll, EntryPoint = "_PTSetGravity@8")]
        internal extern static void SetGravity(IntPtr group, ref NativeVector3 gravity);

        [DllImport(native_dll, EntryPoint = "_PTSetFriction@8")]
        internal extern static void SetFriction(IntPtr group, float friction);

        [DllImport(native_dll, EntryPoint = "_PTSetModelLifetime@12")]
        internal extern static void SetModelLifetime(IntPtr group, float min, float max);


        [DllImport(native_dll, EntryPoint = "_PTBeginUpdate@0")]
        internal extern static void BeginUpdate();

        [DllImport(native_dll, EntryPoint = "_PTEndUpdate@0")]
        internal extern static void EndUpdate();


        [DllImport(native_dll, EntryPoint = "_PTEnableModelParameter@12")]
        internal extern static void EnableModelParameter(IntPtr group, ModelParameterType parameter, bool value);

        [DllImport(native_dll, EntryPoint = "_PTSetInterpolatorKeyframes@16")]
        internal extern static void SetInterpolatorKeyframes(IntPtr group, ModelParameterType parameter, InterpolatorKeyframe[] keyframes, int count);


        [DllImport(native_dll, EntryPoint = "_PTSetEmitterNormal@8")]
        internal extern static void SetEmitterNormal(IntPtr group, ref NativeSharedEmitterProperties properties);

        [DllImport(native_dll, EntryPoint = "_PTSetEmitterRandom@8")]
        internal extern static void SetEmitterRandom(IntPtr group, ref NativeSharedEmitterProperties properties);

        [DllImport(native_dll, EntryPoint = "_PTSetEmitterSpheric@20")]
        internal extern static void SetEmitterSpheric(IntPtr group, ref NativeSharedEmitterProperties properties, ref NativeVector3 direction, float angle_min, float angle_max);

        [DllImport(native_dll, EntryPoint = "_PTSetEmitterStatic@8")]
        internal extern static void SetEmitterStatic(IntPtr group, ref NativeSharedEmitterProperties properties);

        [DllImport(native_dll, EntryPoint = "_PTSetEmitterStraight@12")]
        internal extern static void SetEmitterStraight(IntPtr group, ref NativeSharedEmitterProperties properties, ref NativeVector3 direction);

        [DllImport(native_dll, EntryPoint = "_PTSetEmitterBurstList@12")]
        internal extern static void SetEmitterBurstList(IntPtr group, NativeBurst[] bursts, int count);

        [DllImport(native_dll, EntryPoint = "_PTRestartEmitter@4")]
        internal extern static void RestartEmitter(IntPtr group);

        internal static void RestartLiveEmitters()
        {
            BeginUpdate();
            foreach (ParticleGroup group in ParticleGroup.LiveGroups)
            {
                RestartEmitter(group.native_group);
            }
            EndUpdate();
        }

        [DllImport(native_dll, EntryPoint = "_PTSetZoneAABB@12")]
        internal extern static void SetZoneAABB(IntPtr group, ref NativeVector3 position, ref NativeVector3 extents);

        [DllImport(native_dll, EntryPoint = "_PTSetZoneCylinder@20")]
        internal extern static void SetZoneCylinder(IntPtr group, ref NativeVector3 position, ref NativeVector3 direction, float radius, float length);

        [DllImport(native_dll, EntryPoint = "_PTSetZonePlane@12")]
        internal extern static void SetZonePlane(IntPtr group, ref NativeVector3 position, ref NativeVector3 direction);

        [DllImport(native_dll, EntryPoint = "_PTSetZonePoint@8")]
        internal extern static void SetZonePoint(IntPtr group, ref NativeVector3 position);

        [DllImport(native_dll, EntryPoint = "_PTSetZoneRing@20")]
        internal extern static void SetZoneRing(IntPtr group, ref NativeVector3 position, ref NativeVector3 direction, float inner_radius, float outer_radius);

        [DllImport(native_dll, EntryPoint = "_PTSetZoneSphere@12")]
        internal extern static void SetZoneSphere(IntPtr group, ref NativeVector3 position, float radius);
    }
}
