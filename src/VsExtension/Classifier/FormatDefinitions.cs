using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace VsExtension
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "InlineSucceeded")]
    [Name("InlineSucceeded")]
    [UserVisible(true)]
    [Order(After = Priority.Default)]
    internal sealed class InlineSucceededMethodFormat : ClassificationFormatDefinition
    {
        public InlineSucceededMethodFormat()
        {
            DisplayName = "Inlining Analyzer: Inlining Succeeded";
            this.BackgroundColor = Colors.LightGreen;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "InlineFailed")]
    [Name("InlineFailed")]
    [UserVisible(true)]
    [Order(After = Priority.Default)]
    internal sealed class InlineFailedMethodFormat : ClassificationFormatDefinition
    {
        public InlineFailedMethodFormat()
        {
            DisplayName = "Inlining Analyzer: Inlining Failed";
            this.BackgroundColor = Colors.LightSalmon;
        }
    }
}
