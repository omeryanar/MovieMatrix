using System;
using System.IO;
using Microsoft.Win32;

namespace MovieMatrix.Editors
{
    public class OpenFileDialogEdit : FileDialogEdit
    {
        public OpenFileDialogEdit()
        {
            OpenDialog = () =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = FileFilter;

                if (!String.IsNullOrEmpty(Text))
                {
                    openFileDialog.FileName = Text;
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(Text);
                }

                if (openFileDialog.ShowDialog() == true)
                {
                    EditValue = openFileDialog.FileName;
                    FileOKCommand?.Execute(EditValue);
                }
            };
        }
    }
}
