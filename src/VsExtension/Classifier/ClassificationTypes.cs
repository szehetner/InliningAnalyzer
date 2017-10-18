//------------------------------------------------------------------------------
// <copyright file="MethodCallClassifierClassificationDefinition.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace VsExtension
{
    internal static class ClassificationTypes
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("InlineSucceeded")]
        internal static ClassificationTypeDefinition InlineSucceededType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("InlineFailed")]
        internal static ClassificationTypeDefinition InlineFailedType;

#pragma warning restore 169
    }
}
