using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// A scope.
    /// </summary>
    internal sealed class Scope
    {
        public Scope(bool isLoopOrSwitch = false)
            => _isLoopOrSwitch = isLoopOrSwitch;
        /// <summary>
        /// The parent scope.
        /// </summary>
        public Scope Parent { get; init; }

        /// <summary>
        /// Is this scope a loop/switch?
        /// </summary>
        private bool _isLoopOrSwitch { get; set; } = false;

        /// <summary>
        /// The current namespace.
        /// </summary>
        public Namespace CurrentNamespace { get; set; } = null;

        /// <summary>
        /// The declarations.
        /// </summary>
        private List<Declaration> _decls { get; } = new List<Declaration>();

        /// <summary>
        /// The templated declarations (enums, structs, unions, etc.)
        /// </summary>
        private List<Declaration> _tdecls { get; } = new List<Declaration>();

        /// <summary>
        /// Get all the accessible declarations in this scope.
        /// </summary>
        public IEnumerable<Declaration> Declarations
        {
            get
            {
                var decls = _decls;
                if (Parent != null)
                    decls.AddRange(Parent.Declarations);
                return decls;
            }
        }

        /// <summary>
        /// The templated declarations.
        /// </summary>
        public IEnumerable<Declaration> TemplatedDeclarations
        {
            get
            {
                var decls = _tdecls;
                if (Parent != null)
                    decls.AddRange(Parent.TemplatedDeclarations);
                return decls;
            }
        }

        /// <summary>
        /// Returns if this scope contained in a loop or a switch.
        /// </summary>
        public bool IsLoopOrSwitch
        {
            get
            {
                var isLoopOrSwitch = _isLoopOrSwitch;
                if (Parent != null && !isLoopOrSwitch)
                    isLoopOrSwitch = Parent.IsLoopOrSwitch ? true : false;
                return isLoopOrSwitch;
            }
        }

        /// <summary>
        /// Tries to look up a declaration.
        /// </summary>
        /// <param name="decl">The declaration.</param>
        /// <param name="identifier">The identifier to look for.</param>
        /// <returns></returns>
        public bool TryLookup(out Declaration decl, LinearNamespaceAccessExpression identifier)
            => TryLookup(out decl, identifier.Namespace, identifier.Identifier);

        public bool TryLookupT(out Declaration decl, LinearNamespaceAccessExpression identifier)
            => TryLookupT(out decl, identifier.Namespace, identifier.Identifier);

        /// <summary>
        /// Tries to look up a declaration.
        /// </summary>
        /// <param name="decl">The declaration.</param>
        /// <param name="namespace">The namespace to search in.</param>
        /// <param name="identifier">The identifier to look for.</param>
        /// <returns>True if found.</returns>
        public bool TryLookup(out Declaration decl, Namespace @namespace, string identifier)
        {
            foreach (var _decl in _decls)
            {
                if (@namespace == CurrentNamespace && identifier == _decl.Identifier)
                {
                    decl = _decl;
                    return true;
                }
            }
            if (Parent != null && Parent.TryLookup(out var pDecl, @namespace, identifier))
            {
                decl = pDecl;
                return true;
            }
            decl = null;
            return false;
        }

        /// <summary>
        /// Tries to look up a templated declaration. 
        /// </summary>
        /// <param name="declaration">The declaration.</param>
        /// <param name="namespace">The namespace to search in.</param>
        /// <param name="identifier">The identifier to look for.</param>
        /// <returnsTrue if found.></returns>
        public bool TryLookupT(out Declaration declaration, Namespace @namespace, string identifier)
        {
            foreach (var _tdecl in _tdecls)
                if (@namespace == CurrentNamespace && identifier == _tdecl.Identifier)
                {
                    declaration = _tdecl;
                    return true;
                }
            if (Parent != null && Parent.TryLookupT(out var pDecl, @namespace, identifier))
            {
                declaration = pDecl;
                return true;
            }
            declaration = null;
            return false;
        }

        /// <summary>
        /// Checks if this declaration is already declared.
        /// </summary>
        /// <param name="declaration">The declaration to check.</param>
        /// <returns>True if found.</returns>
        private bool IsAlreadyDeclared(Declaration declaration)
        {
            foreach (var _decl in _decls)
                if (_decl.Namespace == declaration.Namespace && _decl.Identifier == declaration.Identifier)
                    return true;
            foreach (var _tdecl in _tdecls)
                if (_tdecl.Namespace == declaration.Namespace && _tdecl.Identifier == declaration.Identifier)
                    return true;
            if (Parent != null && Parent.IsAlreadyDeclared(declaration))
                return true;
            return false;
        }

        /// <summary>
        /// Tries to declare.
        /// </summary>
        /// <param name="declaration">The declaration.</param>
        /// <returns>True if the declaration was successfully declared.</returns>
        public bool TryDeclare(Declaration declaration)
        {
            if (declaration == null)
                return false;
            if (CurrentNamespace != declaration.Namespace) return false;
            if (IsAlreadyDeclared(declaration)) return false;

            _decls.Add(declaration);
            return true;
        }

        /// <summary>
        /// Tries declaring a templated object (enum, struct, union, etc.)
        /// </summary>
        /// <param name="tDeclaration">The templated declaration.</param>
        /// <returns>True if success.</returns>
        public bool TryDeclareT(Declaration tDeclaration)
        {
            if (tDeclaration == null) return false;
            if (CurrentNamespace != tDeclaration.Namespace) return false;
            if (IsAlreadyDeclared(tDeclaration)) return false;
            _tdecls.Add(tDeclaration);
            return true;
        }
    }
}
