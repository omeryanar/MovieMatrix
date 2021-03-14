using System;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Xpf.Editors;

namespace MovieMatrix.Editors
{
    public abstract class FileDialogEdit : ButtonEdit
    {
        public string FileFilter
        {
            get { return (string)GetValue(FileFilterProperty); }
            set { SetValue(FileFilterProperty, value); }
        }
        public static readonly DependencyProperty FileFilterProperty = DependencyProperty.Register("FileFilter", typeof(string), typeof(FileDialogEdit));

        public ICommand FileOKCommand
        {
            get { return (ICommand)GetValue(FileOKCommandProperty); }
            set { SetValue(FileOKCommandProperty, value); }
        }
        public static readonly DependencyProperty FileOKCommandProperty = DependencyProperty.Register("FileOKCommand", typeof(ICommand), typeof(FileDialogEdit));

        protected virtual Action OpenDialog { get; set; }

        public FileDialogEdit()
        {
            Height = 25;
            IsTextEditable = false;
            AllowDefaultButton = false;

            Buttons.Add(new ButtonInfo()
            {
                GlyphKind = GlyphKind.Regular,
                Command = new DelegateCommand(() => { OpenDialog?.Invoke(); }, () => FileOKCommand?.CanExecute(EditValue) == true)
            });
        }
    }
}
