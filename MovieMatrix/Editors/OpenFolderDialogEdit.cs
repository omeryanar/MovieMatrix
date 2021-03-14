using DevExpress.Mvvm;

namespace MovieMatrix.Editors
{
    public class OpenFolderDialogEdit: FileDialogEdit
    {
        public OpenFolderDialogEdit()
        {
            OpenDialog = () =>
            {
                OpenFolderDialogService openFolderDialog = new OpenFolderDialogService();
                if (openFolderDialog.ShowDialog(Text) == true)
                {
                    EditValue = openFolderDialog.Folder.Path;
                    FileOKCommand?.Execute(EditValue);
                }
            };
        }
    }
}
