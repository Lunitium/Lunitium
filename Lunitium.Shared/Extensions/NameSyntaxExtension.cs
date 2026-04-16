using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lunitium.Shared.Extensions
{
    public static class NameSyntaxExtension
    {
        public static string GetBaseAttributeName(this NameSyntax nameSyntax)
        {
            while (true)
            {
                switch (nameSyntax)
                {
                    case GenericNameSyntax generic:
                        return generic.Identifier.Text;
                    case IdentifierNameSyntax identifier:
                        return identifier.Identifier.Text;
                    case QualifiedNameSyntax qualified:
                        nameSyntax = qualified.Right;
                        continue;
                    case AliasQualifiedNameSyntax alias:
                        nameSyntax = alias.Name;
                        continue;
                    default:
                        return nameSyntax.ToString();
                }
            }
        }
    }
}