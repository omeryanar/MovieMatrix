using System;
using System.Collections.Generic;
using System.Windows;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Printing.Native.Lines;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraPrinting.Native.ExportOptionsControllers;

namespace MovieMatrix.Helper
{
    public class ExportHelper
    {
        public static bool? Export(ExportFormat format, PrintingSystem printingSystem)
        {
            ExportOptionsBase options = GetExportOptions(format, printingSystem);
            ExportOptionsControllerBase controller = ExportOptionsControllerBase.GetControllerByOptions(options);
            LineBase[] lines = Array.ConvertAll(controller.GetExportLines(options, new LineFactory(),
                AvailableExportModes, new List<ExportOptionKind>()), line => (LineBase)line);

            if (lines.Length > 0)
            {
                LinesWindow linesWindow = new LinesWindow();
                linesWindow.WindowStyle = WindowStyle.ToolWindow;
                linesWindow.Owner = Application.Current.MainWindow;
                linesWindow.Title = Properties.Resources.ExportOptions;
                linesWindow.SetLines(lines);

                return linesWindow.ShowDialog();
            }

            return null;
        }

        public static AvailableExportModes AvailableExportModes
        {
            get
            {
                if (availableExportModes == null)
                {
                    availableExportModes = new AvailableExportModes(
                        Enum.GetValues(typeof(RtfExportMode)) as IEnumerable<RtfExportMode>,
                        Enum.GetValues(typeof(DocxExportMode)) as IEnumerable<DocxExportMode>,
                        Enum.GetValues(typeof(HtmlExportMode)) as IEnumerable<HtmlExportMode>,
                        Enum.GetValues(typeof(ImageExportMode)) as IEnumerable<ImageExportMode>,
                        Enum.GetValues(typeof(XlsExportMode)) as IEnumerable<XlsExportMode>,
                        Enum.GetValues(typeof(XlsxExportMode)) as IEnumerable<XlsxExportMode>
                    );
                }

                return availableExportModes;
            }
        }
        private static AvailableExportModes availableExportModes;

        public static ExportOptionsBase GetExportOptions(ExportFormat format, PrintingSystem printingSystem)
        {
            switch (format)
            {
                case ExportFormat.Pdf:
                    return printingSystem.ExportOptions.Pdf;
                case ExportFormat.Htm:
                    return printingSystem.ExportOptions.Html;
                case ExportFormat.Mht:
                    return printingSystem.ExportOptions.Mht;
                case ExportFormat.Rtf:
                    return printingSystem.ExportOptions.Rtf;
                case ExportFormat.Docx:
                    return printingSystem.ExportOptions.Docx;
                case ExportFormat.Xls:
                    return printingSystem.ExportOptions.Xls;
                case ExportFormat.Xlsx:
                    return printingSystem.ExportOptions.Xlsx;
                case ExportFormat.Csv:
                    return printingSystem.ExportOptions.Csv;
                case ExportFormat.Txt:
                    return printingSystem.ExportOptions.Text;
                case ExportFormat.Image:
                    return printingSystem.ExportOptions.Image;
                case ExportFormat.Xps:
                    return printingSystem.ExportOptions.Xps;
                default:
                    return null;
            }
        }
    }
}
