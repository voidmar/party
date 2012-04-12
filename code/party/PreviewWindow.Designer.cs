namespace party
{
    partial class PreviewWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.previewDisplayHolder = new party.PreviewDisplayHolder();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toggleGridToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toggleMotionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.preferencesToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(960, 519);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(960, 544);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.BackColor = System.Drawing.Color.Black;
            this.splitContainer.Panel1.Controls.Add(this.propertyGrid);
            this.splitContainer.Panel1Collapsed = true;
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.BackColor = System.Drawing.Color.Black;
            this.splitContainer.Panel2.Controls.Add(this.previewDisplayHolder);
            this.splitContainer.Size = new System.Drawing.Size(960, 519);
            this.splitContainer.SplitterDistance = 320;
            this.splitContainer.TabIndex = 1;
            // 
            // propertyGrid
            // 
            this.propertyGrid.CommandsVisibleIfAvailable = false;
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.HelpVisible = false;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGrid.Size = new System.Drawing.Size(320, 100);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // previewDisplayHolder
            // 
            this.previewDisplayHolder.BackColor = System.Drawing.Color.Black;
            this.previewDisplayHolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewDisplayHolder.Location = new System.Drawing.Point(0, 0);
            this.previewDisplayHolder.Name = "previewDisplayHolder";
            this.previewDisplayHolder.Size = new System.Drawing.Size(960, 519);
            this.previewDisplayHolder.TabIndex = 0;
            this.previewDisplayHolder.Text = "previewDisplayHolder";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleGridToolStripButton,
            this.toggleMotionToolStripButton,
            this.toolStripSeparator1,
            this.preferencesToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(960, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 0;
            // 
            // toggleGridToolStripButton
            // 
            this.toggleGridToolStripButton.CheckOnClick = true;
            this.toggleGridToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toggleGridToolStripButton.Image = global::party.Properties.Resources.icon_togglegrid;
            this.toggleGridToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toggleGridToolStripButton.Name = "toggleGridToolStripButton";
            this.toggleGridToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.toggleGridToolStripButton.ToolTipText = "Toggle Grid";
            this.toggleGridToolStripButton.CheckedChanged += new System.EventHandler(this.toggleGridToolStripButton_CheckedChanged);
            // 
            // toggleMotionToolStripButton
            // 
            this.toggleMotionToolStripButton.CheckOnClick = true;
            this.toggleMotionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toggleMotionToolStripButton.Image = global::party.Properties.Resources.icon_togglemotion;
            this.toggleMotionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toggleMotionToolStripButton.Name = "toggleMotionToolStripButton";
            this.toggleMotionToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.toggleMotionToolStripButton.Text = "toolStripButton2";
            this.toggleMotionToolStripButton.ToolTipText = "Toggle Motion";
            this.toggleMotionToolStripButton.CheckedChanged += new System.EventHandler(this.toggleMotionToolStripButton_CheckedChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // preferencesToolStripButton
            // 
            this.preferencesToolStripButton.CheckOnClick = true;
            this.preferencesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.preferencesToolStripButton.Image = global::party.Properties.Resources.icon_showpreferences;
            this.preferencesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.preferencesToolStripButton.Name = "preferencesToolStripButton";
            this.preferencesToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.preferencesToolStripButton.Text = "toolStripButton1";
            this.preferencesToolStripButton.ToolTipText = "Preferences";
            this.preferencesToolStripButton.CheckedChanged += new System.EventHandler(this.preferencesToolStripButton_CheckedChanged);
            // 
            // PreviewWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 544);
            this.ControlBox = false;
            this.Controls.Add(this.toolStripContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreviewWindow";
            this.ShowInTaskbar = false;
            this.Text = "Preview";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreviewWindow_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PreviewWindow_FormClosed);
            this.Load += new System.EventHandler(this.PreviewWindow_Load);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        internal PreviewDisplayHolder previewDisplayHolder;
        private System.Windows.Forms.ToolStripButton toggleGridToolStripButton;
        private System.Windows.Forms.ToolStripButton toggleMotionToolStripButton;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton preferencesToolStripButton;
    }
}