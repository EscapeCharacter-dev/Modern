using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing
{
    /// <summary>
    /// Represents a token.
    /// </summary>
    internal sealed class Token
    {
        /// <summary>
        /// Token kind.
        /// </summary>
        public enum TokenKind
        {
            EndOfFile,
            Invalid,

            Cube,
            PlusPlus,
            MinusMinus,
            OpenParenthesis,
            ClosingParenthesis,
            OpenSquareBracket,
            ClosingSquareBracket,
            OpenBracket,
            ClosingBracket,
            Dot,
            Arrow,
            Plus,
            Minus,
            Exclamation,
            Tilde,
            Star,
            Ampersand,
            Slash,
            Percent,
            LeftArrows,
            RightArrows,
            LeftArrow,
            RightArrow,
            LeftArrowEqual,
            RightArrowEqual,
            LeftArrowsEqual,
            RightArrowsEqual,
            Equals,
            ExclamationEqual,
            Caret,
            Pipe,
            Ampersands,
            Carets,
            Pipes,
            QuestionMark,
            Colon,
            Semicolon,
            Equal,
            PlusEqual,
            MinusEqual,
            StarEqual,
            SlashEqual,
            PercentEqual,
            AmpersandEqual,
            CaretEqual,
            PipeEqual,
            Comma,
            At,
            Dollar,
            DotDotDot,
            DotDot,

            Void,
            Byte,
            SByte,
            Bool,
            Char,
            UChar,
            WChar,
            Short,
            UShort,
            Int,
            UInt,
            Long,
            ULong,
            Huge,
            UHuge,
            Float,
            Double,
            LDouble,
            Function,

            Break,
            Case,
            Const,
            Continue,
            Default,
            Do,
            Else,
            Enum,
            Extern,
            For,
            Goto,
            If,
            Return,
            Sizeof,
            Struct,
            Switch,
            Union,
            Typeof,
            Volatile,
            While,
            Start,
            End,
            Between,
            Namespace,
            Using,
            Ctor,
            Public,

            IntLiteral,
            FloatLiteral,
            StringLiteral,
            Identifier
        }

        /// <summary>
        /// The kind of the token.
        /// </summary>
        public TokenKind Kind { get; init; }

        public bool IsTerminator
        {
            get
            {
                return Kind == TokenKind.Semicolon || Kind == TokenKind.Colon || Kind == TokenKind.ClosingParenthesis ||
                    Kind == TokenKind.ClosingSquareBracket || Kind == TokenKind.ClosingBracket;
            }
        }

        /// <summary>
        /// The text the token was shaped from (source code).
        /// </summary>
        public string Source { get; init; }

        /// <summary>
        /// The text representation of the token.
        /// </summary>
        public string Representation => Data is not null ? Data.ToString() : "";

        /// <summary>
        /// Data that is attached to the token.
        /// </summary>
        public object Data { get; init; }

        /// <summary>
        /// Line number of the token.
        /// </summary>
        public int Line { get; init; }

        /// <summary>
        /// Column in line of the token.
        /// </summary>
        public int Column { get; init; }

        public Token() { }
        public Token(TokenKind kind, (int, int) pos)
        {
            Kind = kind;
            Line = pos.Item1;
            Column = pos.Item2;
            Source = kind.GetType().Name.ToLower();
        }
    }
}
