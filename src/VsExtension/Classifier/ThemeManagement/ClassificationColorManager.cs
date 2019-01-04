using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;

namespace VsExtension
{
    public static class ClassificationColorManager
    {
        private static readonly Dictionary<VisualStudioColorTheme, ColorDefinition> _themeColors = new Dictionary<VisualStudioColorTheme, ColorDefinition>();
                
        static ClassificationColorManager()
        {
            _themeColors.Add(VisualStudioColorTheme.LightOrBlue, new ColorDefinition
            {
                SucceededColor = Colors.LightGreen,
                FailedColor = Colors.LightSalmon
            });

            _themeColors.Add(VisualStudioColorTheme.Dark, new ColorDefinition
            {
                SucceededColor = Colors.DarkGreen,
                FailedColor = Colors.DarkRed
            });
        }
        
        public static VisualStudioColorTheme GetCurrentTheme()
        {
            var color = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            if (color.GetBrightness() > 0.5)
                return VisualStudioColorTheme.Dark;

            return VisualStudioColorTheme.LightOrBlue;
        }

        public static ColorDefinition GetDefaultColors()
        {
            var currentTheme = GetCurrentTheme();
            return _themeColors[currentTheme];
        }

        public static void OnThemeChanged(IClassificationFormatMapService formatMapService, IClassificationTypeRegistryService typeRegistryService)
        {
            var currentTheme = GetCurrentTheme();

           var colorDefinition = _themeColors[currentTheme];

            var formatMap = formatMapService.GetClassificationFormatMap(category: "text");
            try
            {
                formatMap.BeginBatchUpdate();
                UpdateFormatMap(formatMap, typeRegistryService, "InlineSucceeded", colorDefinition.SucceededColor);
                UpdateFormatMap(formatMap, typeRegistryService, "InlineFailed", colorDefinition.FailedColor);
            }
            finally
            {
                formatMap.EndBatchUpdate();
            }
        }

        private static void UpdateFormatMap(IClassificationFormatMap formatMap, IClassificationTypeRegistryService typeRegistryService, string type, Color color)
        {
            var classificationType = typeRegistryService.GetClassificationType(type);
            var oldProp = formatMap.GetTextProperties(classificationType);

            var backgroundBrush = new SolidColorBrush(color);

            var newProp = TextFormattingRunProperties.CreateTextFormattingRunProperties(
                oldProp.ForegroundBrush, backgroundBrush, oldProp.Typeface, null, null, oldProp.TextDecorations,
                oldProp.TextEffects, oldProp.CultureInfo);

            formatMap.SetTextProperties(classificationType, newProp);
        }
    }
}
