using System;
using System.IO;
using System.Windows.Forms;

namespace party
{
    static class Program
    {
        public static string BaseAssetPath
        {
            get { return Properties.Settings.Default.BaseAssetPath; }
            set
            {
                Properties.Settings.Default.BaseAssetPath = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Properties.Settings.Default.Reload();

            while (BaseAssetPath == "")
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select base asset directory";
                fbd.SelectedPath = Path.Combine(Environment.CurrentDirectory, @"..\..\art");

                DialogResult fbd_result = fbd.ShowDialog();
                if (fbd_result == DialogResult.OK)
                {
                    BaseAssetPath = fbd.SelectedPath;
                }
                else if (fbd_result == DialogResult.Cancel)
                {
                    return;
                }
            }

            Environment.CurrentDirectory = BaseAssetPath;

            PreviewWindow preview = new PreviewWindow();
            PartyAPI.Initialize(preview.previewDisplayHolder.Handle);
            preview.previewDisplayHolder.ClientSizeChanged += new EventHandler(preview_ResizeEnd);
            preview.Show();

            MainForm main_form = new MainForm();
            if (args.Length == 1)
            {
                main_form.LoadParticleSystem(args[0]);
            }
            Application.Run(main_form);

            PartyAPI.Shutdown();
        }

        static void preview_ResizeEnd(object sender, EventArgs e)
        {
            PartyAPI.ResetView();
        }
    }
}
