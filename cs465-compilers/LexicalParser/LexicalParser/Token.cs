using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalParser {

    #region TokenTypes enum
    public enum TokenType {
        //Reserved
        AndSym,
        ArraySym,
        BeginSym,
        CharSym,
        ChrSym,
        DivSym,
        DoSym,
        ElseSym,
        EndSym,
        IfSym,
        IntegerSym,
        ModSym,
        NotSym,
        OfSym,
        OrSym,
        OrdSym,
        ProcedureSym,
        ProgramSym,
        ReadSym,
        ReadLnSym,
        ThenSym,
        VarSym,
        WhileSym,
        WriteSym,
        WriteLnSym,

        //operator
        Plus,
        Minus,
        Times,
        Less,
        LessEqual,
        NotEqual,
        Greater,
        GreaterEqual,
        Equal,
        Assign,
        Colon,
        Semicolon,
        Comma,
        LeftParen,
        RightParen,
        LeftBracket,
        RightBracket,
        Period,

        //other
        Identifier,
        Number,
        QuoteString,
        LiteralChar,
        EOfSym,
        Illegal,
    }

    #endregion

    public class Token {
        public TokenType Type { get; set; }
        public String Value { get; set; }
        public String ErrorMsg { get; set; }

        public Token() { }
        public Token(TokenType type, String value) {
            Type = type;
            Value = value;
        }

        public String GetTokenName(){
            return Enum.GetName(typeof(TokenType), Type);
        }
    }
}
