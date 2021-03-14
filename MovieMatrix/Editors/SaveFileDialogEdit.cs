using System;
using System.IO;
using Microsoft.Win32;

namespace MovieMatrix.Editors
{
    public class SaveFileDialogEdit : FileDialogEdit
    {
        public SaveFileDialogEdit()
        {
            OpenDialog = () =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = FileFilter;

                if (!String.IsNullOrEmpty(Text))
                {
                    saveFileDialog.FileName = Text;
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(Text);
                }

                if (saveFileDialog.ShowDialog() == true)
                {
                    EditValue = saveFileDialog.FileName;
                    FileOKCommand?.Execute(EditValue);
                }
            };
        }
    }
}
