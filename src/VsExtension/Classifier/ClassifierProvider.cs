using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsExtension.Model;

namespace VsExtension
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("CSharp")]
    [TagType(typeof(IClassificationTag))]
    internal class ClassifierProvider : ITaggerProvider, IDisposable
    {
#pragma warning disable CS0649
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry; // Set via MEF

        [Import]
        internal IAnalyzerModel AnalyzerModel; // Set via MEF

        [Import]
        internal ICodeModel CodeModel; // Set via MEF

        [Import]
        internal IClassificationFormatMapService FormatMapService; // Set via MEF

        [Import]
        internal IClassificationTypeRegistryService TypeRegistryService; // Set via MEF
        

#pragma warning restore CS0649

        public ClassifierProvider()
        {
            VSColorTheme.ThemeChanged += UpdateTheme;
        }

        private void UpdateTheme(ThemeChangedEventArgs e)
        {
            ClassificationColorManager.OnThemeChanged(FormatMapService, TypeRegistryService);
        }
        
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return (ITagger<T>)new Classifier(buffer, ClassificationRegistry, AnalyzerModel, CodeModel);
        }

        public void Dispose()
        {
            VSColorTheme.ThemeChanged -= UpdateTheme;
        }
    }
}
