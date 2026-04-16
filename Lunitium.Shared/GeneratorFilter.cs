using System;
using System.Linq;
using System.Threading;
using Lunitium.Shared.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lunitium.Shared
{
    public static class GeneratorFilter
    {
        public static Func<SyntaxNode, CancellationToken, bool> FilterClassAttribute(string className)
        {
            return (node, token) =>
            {
                if (!(node is ClassDeclarationSyntax classDeclaration))
                    return false;

                var attributeName = className;
                var shortName = attributeName.EndsWith("Attribute")
                    ? attributeName.Substring(0, attributeName.Length - 9)
                    : attributeName;

                return classDeclaration.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .Any(attribute =>
                    {
                        var appliedName = attribute.Name.GetBaseAttributeName();
                        
                        return appliedName == attributeName || 
                               appliedName == shortName;
                    });
            };
        }

        public static bool FilterClass(SyntaxNode node, CancellationToken cancellationToken)
        {
            return node is ClassDeclarationSyntax;
        }
    }
}