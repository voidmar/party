using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace party
{
    internal class PInvoke
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern bool PathRelativePathTo(
             [Out] StringBuilder pszPath,
             [In] string pszFrom,
             [In] FileAttributes dwAttrFrom,
             [In] string pszTo,
             [In] FileAttributes dwAttrTo
        );
    }

    class TextureFilenameEditor : UITypeEditor
    {
        OpenFileDialog open_file_dialog = null;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (service != null)
                {
                    if (open_file_dialog == null)
                    {
                        open_file_dialog = new OpenFileDialog();
                        open_file_dialog.Filter = "Texture Files (*.tga, *.dds)|*.tga;*.dds|All Files (*.*)|*.*";
                        open_file_dialog.Title = "Select texture file";
                    }
                    if (value as string != null)
                    {
                        string path_to_edit = (string)value;
                        if (path_to_edit.StartsWith(".\\")) path_to_edit = path_to_edit.Substring(2);
                        path_to_edit = Path.Combine(Program.BaseAssetPath, path_to_edit);

                        open_file_dialog.FileName = Path.GetFileName(path_to_edit);
                        open_file_dialog.InitialDirectory = Path.GetDirectoryName(path_to_edit);
                    }
                    if (open_file_dialog.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder final_path = new StringBuilder(260);
                        PInvoke.PathRelativePathTo(final_path, Program.BaseAssetPath, FileAttributes.Directory, open_file_dialog.FileName, FileAttributes.Normal);
                        value = final_path.ToString();
                    }
                }
            }
            return value;
        }
    }
}
