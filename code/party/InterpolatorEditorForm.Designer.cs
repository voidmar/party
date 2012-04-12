namespace party
{
    partial class InterpolatorEditorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.SplitContainer splitContainer1;
            System.Windows.Forms.SplitContainer splitContainer2;
            this.interpolatorListbox = new System.Windows.Forms.ListBox();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.interpolatorEditorControl = new party.InterpolatorEditorControl();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(this.interpolatorEditorControl);
            splitContainer1.Size = new System.Drawing.Size(706, 517);
            splitContainer1.SplitterDistance = 200;
            splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(this.interpolatorListbox);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(this.propertyGrid);
            splitContainer2.Size = new System.Drawing.Size(200, 517);
            splitContainer2.SplitterDistance = 258;
            splitContainer2.TabIndex = 1;
            // 
            // interpolatorListbox
            // 
            this.interpolatorListbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.interpolatorListbox.FormattingEnabled = true;
            this.interpolatorListbox.IntegralHeight = false;
            this.interpolatorListbox.Location = new System.Drawing.Point(0, 0);
            this.interpolatorListbox.Name = "interpolatorListbox";
            this.interpolatorListbox.Size = new System.Drawing.Size(200, 258);
            this.interpolatorListbox.TabIndex = 0;
            this.interpolatorListbox.SelectedIndexChanged += new System.EventHandler(this.interpolatorListbox_SelectedIndexChanged);
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.HelpVisible = false;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGrid.Size = new System.Drawing.Size(200, 255);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // interpolatorEditorControl
            // 
            this.interpolatorEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.interpolatorEditorControl.Group = null;
            this.interpolatorEditorControl.Location = new System.Drawing.Point(0, 0);
            this.interpolatorEditorControl.Name = "interpolatorEditorControl";
            this.interpolatorEditorControl.Selected = null;
            this.interpolatorEditorControl.SelectedEntry = null;
            this.interpolatorEditorControl.Size = new System.Drawing.Size(502, 517);
            this.interpolatorEditorControl.TabIndex = 0;
            this.interpolatorEditorControl.Text = "interpolatorEditorControl";
            // 
            // InterpolatorEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(706, 517);
            this.Controls.Add(splitContainer1);
            this.KeyPreview = true;
            this.Name = "InterpolatorEditorForm";
            this.Text = "InterpolatorEditor";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).EndInit();
            splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox interpolatorListbox;
        private InterpolatorEditorControl interpolatorEditorControl;
        private System.Windows.Forms.PropertyGrid propertyGrid;



    }
}