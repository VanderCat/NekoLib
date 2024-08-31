/*--------------------------------------------------------------------------------------------
 *	Based on Microsoft.Unity.Analyzers
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License.
 *-------------------------------------------------------------------------------------------*/

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace NekoLib.Analyzers.Extensions;

internal static class MethodSymbolExtension
{
    public static bool Matches(this IMethodSymbol symbol, MethodInfo method)
    {
        if (method.Name != symbol.Name)
            return false;

        if (!symbol.ReturnType.Matches(method.ReturnType))
            return false;

        var parameters = method.GetParameters();
        if (parameters.Length < symbol.Parameters.Length)
            return false;

        for (var i = 0; i < symbol.Parameters.Length; i++)
        {
            if (!symbol.Parameters[i].Type.Matches(parameters[i].ParameterType))
                return false;
        }

        return true;
    }
}