namespace mnc.Parsing.Expression
{
    /// <summary>
    /// An enum declaration.
    /// </summary>
    internal sealed record EnumDecl(
        string Identifier,
        ulong Index
        )
    {
        /// <summary>
        /// Compares an enum declaration. with a identifier.
        /// </summary>
        /// <param name="decl">The enum declaration.</param>
        /// <param name="str">The identifier.</param>
        /// <returns>True if equal.</returns>
        public static bool operator ==(EnumDecl decl, string str) => decl.Identifier == str;
        /// <summary>
        /// Compares an enum declaration. with a identifier.
        /// </summary>
        /// <param name="decl">The enum declaration.</param>
        /// <param name="str">The identifier.</param>
        /// <returns>True if not equal.</returns>
        public static bool operator !=(EnumDecl decl, string str) => decl.Identifier != str;

        public override string ToString()
        {
            return $"{Identifier}@{Index}";
        }
    }
}
