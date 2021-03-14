using System.Globalization;
using System.Threading;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;

namespace MovieMatrix.Resources
{
    public static class ResourceProvider
    {
        public static ObjectDataProvider ObjectDataProvider { get; private set; }

        public static void ChangeCulture(string cultureName)
        {
            try
            {
                CultureInfo culture = new CultureInfo(cultureName);
                ChangeCulture(culture);
            }
            catch { return; }
        }

        public static void ChangeCulture(CultureInfo newCulture)
        {
            try
            {
                CultureInfo oldCulture = Properties.Resources.Culture;
                Properties.Resources.Culture = newCulture;
                ObjectDataProvider.Refresh();

                Thread.CurrentThread.CurrentCulture = newCulture;
                Thread.CurrentThread.CurrentUICulture = newCulture;

                DXMessageBoxLocalizer.Active = new CustomMessageBoxLocalizer();

                CultureChangeMessage message = new CultureChangeMessage(oldCulture?.Name, newCulture?.Name);
                Messenger.Default.Send(message);
            }
            catch { return; }
        }

        static ResourceProvider()
        {
            ObjectDataProvider = new ObjectDataProvider();
            ObjectDataProvider.ObjectInstance = new Properties.Resources();
        }
    }

    public class CustomMessageBoxLocalizer : DXMessageBoxLocalizer
    {
        protected override void PopulateStringTable()
        {
            base.PopulateStringTable();

            AddString(DXMessageBoxStringId.Ok, Properties.Resources.OK);
            AddString(DXMessageBoxStringId.Cancel, Properties.Resources.Cancel);
            AddString(DXMessageBoxStringId.Yes, Properties.Resources.Yes);
            AddString(DXMessageBoxStringId.No, Properties.Resources.No);
        }
    }
}
