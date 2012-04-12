using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace party
{
    public partial class PreviewWindow : Form
    {
        public static PreviewWindow Instance;
        public static volatile bool CameraDirty;
        public static float Distance = 10;
        public static float Yaw = (float)(-45 * Math.PI / 180.0f);
        public static float Pitch = (float)(-45 * Math.PI / 180.0f);
        public static PreviewProperties Properties = new PreviewProperties();

        public PreviewWindow()
        {
            InitializeComponent();
            Instance = this;

            previewDisplayHolder.MouseDown += new MouseEventHandler(previewDisplayHolder_MouseDown);
            previewDisplayHolder.MouseMove += new MouseEventHandler(previewDisplayHolder_MouseMove);
            previewDisplayHolder.MouseUp += new MouseEventHandler(previewDisplayHolder_MouseUp);
            previewDisplayHolder.KeyDown += new KeyEventHandler(previewDisplayHolder_KeyDown);

            propertyGrid.SelectedObject = Properties;
        }

        private void PreviewWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; // do not want
        }

        void previewDisplayHolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                PartyAPI.RestartLiveEmitters();
            }
        }

        Point initial_mouse_location;
        Point screen_center;
        Point last_mouse_location;
        bool cursor_visible = true;
        void previewDisplayHolder_MouseDown(object sender, MouseEventArgs e)
        {
            previewDisplayHolder.Focus();

            if (e.Button != MouseButtons.Right) return;

            initial_mouse_location = Cursor.Position;
            last_mouse_location = initial_mouse_location;
        }

        void previewDisplayHolder_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            if (!cursor_visible)
            {
                Cursor.Position = initial_mouse_location;
                Cursor.Show();
                cursor_visible = true;
            }
        }

        void previewDisplayHolder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            Point delta_location = new Point();
            if (cursor_visible)
            {
                delta_location.X = Cursor.Position.X - last_mouse_location.X;
                delta_location.Y = Cursor.Position.Y - last_mouse_location.Y;
                last_mouse_location = Cursor.Position;

                int overall_movement_x = Cursor.Position.X - initial_mouse_location.X;
                int overall_movement_y = Cursor.Position.Y - initial_mouse_location.Y;
                if (Math.Sqrt(overall_movement_x * overall_movement_x + overall_movement_y * overall_movement_y) > 100)
                {
                    cursor_visible = false;
                    screen_center.X = Screen.PrimaryScreen.Bounds.X + Screen.PrimaryScreen.Bounds.Width / 2;
                    screen_center.Y = Screen.PrimaryScreen.Bounds.Y + Screen.PrimaryScreen.Bounds.Height / 2;
                    Cursor.Position = screen_center;
                    Cursor.Hide();
                }
            }
            else
            {
                delta_location.X = Cursor.Position.X - screen_center.X;
                delta_location.Y = Cursor.Position.Y - screen_center.Y;
                Cursor.Position = screen_center;
            }

            if (e.Button == MouseButtons.Right)
            {
                Pitch += delta_location.Y / -60.0f;
                Yaw += delta_location.X / -60.0f;
                CameraDirty = true;
            }

            /*
            if (e.Button == MouseButtons.Left)
            {
                Distance += delta_location.Y / -40.0f;
                CameraDirty = true;
            }
            /**/
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Distance += e.Delta / 120.0f * -0.75f;
            CameraDirty = true;
        }

        private void toggleGridToolStripButton_CheckedChanged(object sender, EventArgs e)
        {
            PartyAPI.SetGridEnabled(toggleGridToolStripButton.Checked);
        }

        private void PreviewWindow_Load(object sender, EventArgs e)
        {
            ToolStripManager.LoadSettings(this);
        }

        private void PreviewWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            ToolStripManager.SaveSettings(this);
        }

        private void preferencesToolStripButton_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer.Panel1Collapsed = !preferencesToolStripButton.Checked;

            if (splitContainer.Panel1Collapsed) previewDisplayHolder.Focus();
        }

        private void toggleMotionToolStripButton_CheckedChanged(object sender, EventArgs e)
        {
            Properties.MotionEnabled = toggleMotionToolStripButton.Checked;
            propertyGrid.Refresh();
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            toggleMotionToolStripButton.Checked = Properties.MotionEnabled;
        }
    }

    public enum BackgroundMode
    {
        Color,
        Image
    };

    public class PreviewProperties
    {
        public bool Dirty = false;

        /*
        BackgroundMode mode;
        public BackgroundMode BackgroundMode
        {
            get { return mode; }
            set
            {
                mode = value;
                Dirty = true;
            }
        }
        */

        Color background_color = Color.Black;
        public Color BackgroundColor
        {
            get { return background_color; }
            set 
            { 
                background_color = value;
                Dirty = true;
            }
        }

        bool motion_enabled = false;
        [RefreshProperties(RefreshProperties.All)]
        public bool MotionEnabled
        {
            get { return motion_enabled; }
            set 
            { 
                motion_enabled = value;
                Dirty = true;
            }
        }

        float motion_radius = 2;
        public float MotionRadius
        {
            get { return motion_radius; }
            set
            {
                motion_radius = value;
                Dirty = true;
            }
        }

        float motion_speed = 2;
        public float MotionSpeed
        {
            get { return motion_speed; }
            set
            {
                motion_speed = value;
                Dirty = true;
            }
        }
    }
}
