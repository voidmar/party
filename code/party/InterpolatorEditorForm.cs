using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace party
{
    public partial class InterpolatorEditorForm : Form
    {
        public static event EventHandler InterpolatorChanged;

        static ParticleGroup group;
        public static ParticleGroup CurrentGroup
        {
            get { return group; }
            set
            {
                group = value;
                if (instance != null && value == null) instance.Close();
            }
        }

        static InterpolatorEditorForm instance = null;
        InterpolatorGroup current_group;
        public static void Show(InterpolatorGroup group)
        {
            if (instance == null) instance = new InterpolatorEditorForm();
            instance.Reset(group);
            instance.Show();
            instance.Focus();
        }

        public InterpolatorEditorForm()
        {
            InitializeComponent();

            interpolatorEditorControl.EntrySelectionChanged += new EventHandler(interpolatorEditorControl_EntrySelectionChanged);
        }

        void Reset(InterpolatorGroup group)
        {
            if (current_group != null) current_group.Changed -= current_group_Changed;
            current_group = group;
            current_group.Changed += new EventHandler(current_group_Changed);

            interpolatorListbox.Items.Clear();
            foreach (Interpolator interpolator in group.Interpolators)
            {
                interpolatorListbox.Items.Add(interpolator);
            }
            interpolatorEditorControl.Group = group;

            this.Text = CurrentGroup.Name + " - Parameter Editor";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            instance = null;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == (char)Keys.Escape)
            {
                interpolatorEditorControl.Selected = null;
            }
        }

        void interpolatorListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            interpolatorEditorControl.Selected = (Interpolator)interpolatorListbox.SelectedItem;
            propertyGrid.SelectedObject = interpolatorListbox.SelectedItem;
        }

        void interpolatorEditorControl_EntrySelectionChanged(object sender, EventArgs e)
        {
            if (interpolatorEditorControl.SelectedEntry != null)
            {
                propertyGrid.SelectedObject = interpolatorEditorControl.SelectedEntry;
            }
            else
            {
                propertyGrid.SelectedObject = interpolatorListbox.SelectedItem;
            }
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (propertyGrid.SelectedObject != interpolatorListbox.SelectedItem)
            {
                interpolatorEditorControl.PropertyChanged();
            }
            interpolatorEditorControl.Invalidate();

            if (InterpolatorChanged != null) InterpolatorChanged(this, EventArgs.Empty);
        }

        void current_group_Changed(object sender, EventArgs e)
        {
            if (InterpolatorChanged != null) InterpolatorChanged(this, EventArgs.Empty);
        }
    }

    class InterpolatorGroupEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            InterpolatorGroup interpolator_group = value as InterpolatorGroup;
            if (interpolator_group != null)
            {
                InterpolatorEditorForm.Show(interpolator_group);
            }
            return value;
        }
    }

}
