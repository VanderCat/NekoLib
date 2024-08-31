/*--------------------------------------------------------------------------------------------
 *  Based on Microsoft.Unity.Analyzers
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License.
 *-------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NekoLib.Analyzers.Extensions;

internal static class SuppressionAnalysisContextExtension
{
    public static T? GetSuppressibleNode<T>(this Microsoft.CodeAnalysis.Diagnostics.SuppressionAnalysisContext context, Diagnostic diagnostic) where T : SyntaxNode
    {
        return GetSuppressibleNode<T>(context, diagnostic, _ => true);
    }

    public static T? GetSuppressibleNode<T>(this Microsoft.CodeAnalysis.Diagnostics.SuppressionAnalysisContext context, Diagnostic diagnostic, Func<T, bool> predicate) where T : SyntaxNode
    {
        var location = diagnostic.Location;
        var sourceTree = location.SourceTree;
        var root = sourceTree?.GetRoot(context.CancellationToken);

        return root?
            .FindNode(location.SourceSpan)
            .DescendantNodesAndSelf()
            .OfType<T>()
            .FirstOrDefault(predicate);
    }
}