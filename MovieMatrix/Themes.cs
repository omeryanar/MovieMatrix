using System.Windows.Media;
using DevExpress.Xpf.Core;

namespace MovieMatrix
{
    public class ThemeManager
    {
        public static void RegisterMaterialThemes()
        {
            Register("1", "Blue", "#FF2196F3", "#FF42A5F5", "#FF1E88E5");
            Register("2", "LightBlue", "#FF03A9F4", "#FF29B6F6", "#FF039BE5");
            Register("3", "Cyan", "#FF00BCD4", "#FF26C6DA", "#FF00ACC1");
            Register("4", "Pink", "#FFE91E63", "#FFEC407A", "#FFD81B60");
            Register("5", "Orange", "#FFFF9800", "#FFFFA726", "#FFFB8C00");           
            Register("6", "Purple", "#FF9C27B0", "#FFAB47BC", "#FF8E24AA");
            Register("7", "DeepPurple", "#FF673AB7", "#FF7E57C2", "#FF5E35B1");
            Register("8", "Indigo", "#FF3F51B5", "#FF5C6BC0", "#FF3949AB");
        }

        public static void Register(string name, string fullName, string baseColor, string lightColor, string darkColor)
        {
            ThemePalette palette = new ThemePalette(name);
            palette.SetColor("Border", (Color)ColorConverter.ConvertFromString("#FF484848"));
            palette.SetColor("Delimeter", (Color)ColorConverter.ConvertFromString("#FF484848"));

            palette.SetColor("HoverBorder", (Color)ColorConverter.ConvertFromString("#FF404040"));
            palette.SetColor("HoverBackground", (Color)ColorConverter.ConvertFromString("#FF404040"));
            palette.SetColor("Button.Background", (Color)ColorConverter.ConvertFromString("#FF404040"));

            palette.SetColor("Editor.Background", (Color)ColorConverter.ConvertFromString("#FF282828"));
            palette.SetColor("Window.Background", (Color)ColorConverter.ConvertFromString("#FF282828"));
            palette.SetColor("Control.Background", (Color)ColorConverter.ConvertFromString("#FF282828"));

            palette.SetColor("Focused", (Color)ColorConverter.ConvertFromString(baseColor));
            palette.SetColor("SelectionBorder", (Color)ColorConverter.ConvertFromString(baseColor));
            palette.SetColor("SelectionBackground", (Color)ColorConverter.ConvertFromString(baseColor));
            palette.SetColor("Backstage.Window.Background", (Color)ColorConverter.ConvertFromString(baseColor));

            palette.SetColor("Backstage.Delimeter", (Color)ColorConverter.ConvertFromString(lightColor));
            palette.SetColor("Backstage.HoverBackground", (Color)ColorConverter.ConvertFromString(lightColor));
            palette.SetColor("Backstage.Button.Background", (Color)ColorConverter.ConvertFromString(lightColor));

            palette.SetColor("Backstage.Editor.Background", (Color)ColorConverter.ConvertFromString(darkColor));
            palette.SetColor("Backstage.SelectionBackground", (Color)ColorConverter.ConvertFromString(darkColor));

            Theme theme = Theme.CreateTheme(palette, Theme.VS2017Dark, null, fullName, baseColor);
            Theme.RegisterTheme(theme);
        }
    }
}
