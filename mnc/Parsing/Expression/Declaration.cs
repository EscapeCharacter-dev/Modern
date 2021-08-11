using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// A declaration.
    /// </summary>
    internal sealed class Declaration
    {
        /// <summary>
        /// The kind of declaration.
        /// </summary>
        public enum DeclarationKind
        {
            Function,
            Variable,
            Constant,
            Enum,
            Struct,
            Union,
            ExternalFunction,
            ExternalVariable,
            ExternalConstant,
            ExternalStruct,
            ExternalUnion,
            PublicFunction,
            PublicVariable,
            PublicConstant,
            PublicEnum,
            PublicStruct,
            PublicUnion,
        }

        /// <summary>
        /// Declaration kind.
        /// </summary>
        public DeclarationKind Kind { get; init; }

        /// <summary>
        /// The namespace of this declaration. null means no namespace, or the function is private.
        /// </summary>
        public Namespace Namespace { get; init; }

        /// <summary>
        /// The identifier.
        /// </summary>
        public string Identifier { get; init; }

        /// <summary>
        /// The default value.
        /// </summary>
        public (CExpression AsExpression,
            Statement AsStatement,
            IEnumerable<EnumDecl> AsEnumDeclarations,
            IEnumerable<Declaration> AsStructureOrUnion
            ) DefaultValue { get; set; }
            = new (null, null, Enumerable.Empty<EnumDecl>(), Enumerable.Empty<Declaration>());

        /// <summary>
        /// Tries to get an enum declaration.
        /// </summary>
        /// <returns>The reference to the enum declaration.</returns>
        public EnumDecl GetEnumDecl(string identifier)
        {
            if (DefaultValue.AsEnumDeclarations == null || DefaultValue.AsEnumDeclarations.Count() == 0)
                return null;
            foreach (var decl in DefaultValue.AsEnumDeclarations)
                if (decl.Identifier == identifier)
                    return decl;
            return null;
        }

        /// <summary>
        /// Tries to get a struct/union declaration.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns>The reference to the member declaration.</returns>
        public Declaration GetDeclarations(string identifier)
        {
            if (DefaultValue.AsStructureOrUnion == null || DefaultValue.AsStructureOrUnion.Count() == 0)
                return null;
            foreach (var decl in DefaultValue.AsStructureOrUnion)
                if (decl.Identifier == identifier)
                    return decl;
            return null;
        }

        /// <summary>
        /// The type of the declared object.
        /// </summary>
        public TypeInfo Type { get; init; }

        /// <summary>
        /// The address of where this symbol is pinned to.
        /// </summary>
        public ulong PinnedAt = 0;

        public override string ToString()
        {
            if (Kind == DeclarationKind.Enum)
            {
                var str = $"ENUM {Namespace}::{Identifier}{{";
                foreach (var enumDecl in DefaultValue.AsEnumDeclarations)
                    str += enumDecl + ",";
                return str.TrimEnd(',') + "}";
            }
            else if (Kind == DeclarationKind.Struct)
            {
                var str = $"STRUCT {Namespace}::{Identifier}{{";
                foreach (var decl in DefaultValue.AsStructureOrUnion)
                    str += decl + ",";
                return str.TrimEnd(',') + "}";
            }
            else if (Kind == DeclarationKind.Union)
            {
                var str = $"UNION {Namespace}::{Identifier}{{";
                foreach (var decl in DefaultValue.AsStructureOrUnion)
                    str += decl + ",";
                return str.TrimEnd(',') + "}";
            }
            var baseString = $"DECLARE<{Type}> {Namespace}::{Identifier}";
            if (Kind == DeclarationKind.Function || Kind == DeclarationKind.PublicFunction) baseString += $" {(DefaultValue.AsStatement != null ? DefaultValue.AsStatement : DefaultValue.AsExpression)}";
            else baseString += $" {(DefaultValue.AsExpression != null ? DefaultValue.AsExpression.ToString() : "(no expression)")}";
            return baseString;
        }
    }
}
