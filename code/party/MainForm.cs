using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using Procurios.Public;

namespace party
{
    public partial class MainForm : Form
    {
        bool dirty = false;
        int document_count = 1;
        string document_path = "new 1";

        public MainForm()
        {
            InitializeComponent();

            EnableGroupContextItems(false);
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParticleGroup group = new ParticleGroup();
            groupListbox.Items.Add(group);

            MakeDirty();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParticleGroup group = (ParticleGroup)groupListbox.SelectedItem;
            groupListbox.Items.Remove(group);
            group.Dispose();

            MakeDirty();
        }

        private void groupListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableGroupContextItems(groupListbox.SelectedItem != null);
            groupPropertyGrid.SelectedObject = groupListbox.SelectedItem;

            InterpolatorEditorForm.CurrentGroup = (ParticleGroup)groupListbox.SelectedItem;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ToolStripManager.LoadSettings(this);

            InterpolatorEditorForm.InterpolatorChanged += new EventHandler(InterpolatorEditorForm_InterpolatorChanged);
            groupPropertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(groupPropertyGrid_PropertyValueChanged);

            ResetTitlebar();
        }

        void groupPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            MakeDirty();

            if (e.ChangedItem.Label == "Name")
            {
                object selected = groupListbox.SelectedItem;
                ArrayList groups = new ArrayList(groupListbox.Items);
                groupListbox.SuspendLayout();
                groupListbox.Items.Clear();
                groupListbox.Items.AddRange(groups.ToArray());
                groupListbox.SelectedItem = selected;
                groupListbox.ResumeLayout();
            }
        }

        void InterpolatorEditorForm_InterpolatorChanged(object sender, EventArgs e)
        {
            MakeDirty();
        }

        void MakeDirty()
        {
            dirty = true;
            ResetTitlebar();
        }

        void ResetTitlebar()
        {
            this.Text = string.Format("{0}{1} - party", Path.GetFileNameWithoutExtension(document_path), dirty ? "*" : "");
        }


        public void LoadParticleSystem(string path)
        {
            ++document_count;
            document_path = path;
            dirty = false;
            ResetTitlebar();

            StreamReader input = new StreamReader(path);
            string json = input.ReadToEnd();
            input.Close();

            PartyAPI.BeginUpdate();
            foreach (ParticleGroup group in groupListbox.Items)
            {
                group.Dispose();
            }
            groupListbox.Items.Clear();
            groupPropertyGrid.SelectedObject = null;

            ArrayList groups = (ArrayList)JSON.JsonDecode(json);
            foreach (object group_data in groups)
            {
                ParticleGroup group = new ParticleGroup();
                group.Deserialize(group_data);
                groupListbox.Items.Add(group);
            }
            PartyAPI.EndUpdate();
        }

        public void SaveParticleSystem(string path)
        {
            document_path = path;
            dirty = false;
            ResetTitlebar();

            ArrayList groups = new ArrayList();
            foreach (ParticleGroup group in groupListbox.Items)
            {
                groups.Add(group.Serialize());
            }

            string json = JSON.JsonEncode(groups);

            StreamWriter output = new StreamWriter(path);
            output.Write(json);
            output.Close();
        }

        DialogResult ShowLastChanceDialog()
        {
            if (!dirty) return DialogResult.OK;

            DialogResult result = MessageBox.Show(this, string.Format("Save file '{0}'?", document_path), "Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

            if (result == DialogResult.Yes) result = ShowSaveFileDialog();

            return result;
        }

        DialogResult ShowSaveFileDialog()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JSON files (*.json)|*.json|All Files (*.*)|*.*";
            sfd.OverwritePrompt = true;
            sfd.RestoreDirectory = true;
            sfd.InitialDirectory = Path.GetDirectoryName(document_path);
            sfd.FileName = Path.GetFileNameWithoutExtension(document_path);

            DialogResult result = sfd.ShowDialog();

            if (result != DialogResult.OK) return result;

            SaveParticleSystem(sfd.FileName);

            return result;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = ShowLastChanceDialog();
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            ToolStripManager.SaveSettings(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setAssetPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = Program.BaseAssetPath;
            fbd.Description = "Select base asset directory";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Program.BaseAssetPath = fbd.SelectedPath;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ShowLastChanceDialog() == DialogResult.Cancel) return;

            foreach (ParticleGroup group in groupListbox.Items)
            {
                group.Dispose();
            }
            groupListbox.Items.Clear();
            groupPropertyGrid.SelectedObject = null;

            document_path = "new " + (++document_count);
            dirty = false;
            ResetTitlebar();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ShowLastChanceDialog() == DialogResult.Cancel) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON files (*.json)|*.json|All Files (*.*)|*.*";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            LoadParticleSystem(ofd.FileName);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSaveFileDialog();
        }

        private void restartToolStripButton_Click(object sender, EventArgs e)
        {
            PartyAPI.RestartLiveEmitters();
        }

        private void revealPreviewToolStripButton_Click(object sender, EventArgs e)
        {
            PreviewWindow.Instance.Focus();
        }

        void EnableGroupContextItems(bool enable)
        {
            deleteToolStripMenuItem.Enabled = enable;
            deleteGroupToolStripButton.Enabled = enable;
            copyToolStripMenuItem.Enabled = enable;
        }

        void EnablePasteItems(bool enable)
        {
            pasteToolStripMenuItem.Enabled = enable;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            EnablePasteItems(Clipboard.ContainsData("party-emitter"));
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!groupListbox.Focused) return;

            ParticleGroup group = (ParticleGroup)groupListbox.SelectedItem;
            if (group == null) return;

            string json = JSON.JsonEncode(group.Serialize());
            DataObject data_object = new DataObject("party-emitter", json);
            Clipboard.SetDataObject(data_object, true);

            EnablePasteItems(true);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IDataObject data_object = Clipboard.GetDataObject();

            string emitter_json = (string)data_object.GetData("party-emitter");
            if (emitter_json == null) return;

            object emitter_data = JSON.JsonDecode(emitter_json);

            ParticleGroup group = new ParticleGroup();
            group.Deserialize(emitter_data);

            groupListbox.SuspendLayout();
            groupListbox.Items.Add(group);
            groupListbox.ResumeLayout();
            groupListbox.Invalidate(true);

            MakeDirty();
        }
    }

}
