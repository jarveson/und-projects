using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalParser.Parser {
    public class Parser {
        Stack<Token> parseStack = new Stack<Token>();
        bool parseSuccessful = true;
        bool hitEndOfFile = false;
        List<Token> Tokens;
        int currentToken = 0;
        Assembler assembler;
        SymbolTable symbolTable = new SymbolTable();

        public bool Parse(List<Token> tokens) {
            //check for the correct start to our mini pascal program
            Tokens = tokens;

            //check for 'program <name> header'
            //should move this into statement probably
            if (Tokens[currentToken].Type != TokenType.ProgramSym) {
                WriteError("Missing 'Program' symbol", Tokens[currentToken]);
                AdvanceToken();
            }

            //next token should be an identifier...
            AdvanceToken();
            if (Tokens[currentToken].Type != TokenType.Identifier) {
                WriteError("Missing program identifier", Tokens[currentToken]);
            }
            assembler = new Assembler(Tokens[currentToken].Value);

            //eat the input until semicolon
            while (Tokens[currentToken].Type != TokenType.Semicolon) {
                AdvanceToken();
                if (currentToken >= Tokens.Count-1) {
                    WriteError("Expected Semicolon end of first line", Tokens[Tokens.Count-1]);
                    return false;
                }
            }
            AdvanceToken();

            //parse variables
            if (Tokens[currentToken].Type == TokenType.VarSym) {
                if (!ParseVariables()) {
                    WriteError("Error parsing variables", Tokens[Tokens.Count - 1]);
                    return false;
                }
            }

            if (Tokens[currentToken].Type != TokenType.BeginSym) {
                WriteError("Missing 'Begin' symbol", Tokens[currentToken]);
            }
            AdvanceToken();
            Statement();
            while (Tokens[currentToken].Type == TokenType.Semicolon ) {
                AdvanceToken();
                Statement();
            }
            if (Tokens[currentToken].Type != TokenType.EndSym) {
                WriteError("Expected 'End' symbol", Tokens[currentToken]);
            }

            if (parseSuccessful) {
                assembler.FinalizeCode();
            }
            else {
                assembler.RemoveFile();
            }
            return parseSuccessful;
        }

        private bool ParseVariables() {
            List<string> someVars = new List<string>();
            while (Tokens[currentToken].Type != TokenType.BeginSym) {
                if (currentToken >= Tokens.Count) {
                    return false;
                }
                if (Tokens[currentToken].Type == TokenType.Semicolon){
                    AdvanceToken();
                    continue;
                }

                if (Tokens[currentToken].Type == TokenType.Identifier) {
                    someVars.Add(Tokens[currentToken].Value);
                }
                else if (Tokens[currentToken].Type == TokenType.Colon) {
                    //next is a type identifier..hopefully
                    AdvanceToken();
                    if (Tokens[currentToken].Type == TokenType.IntegerSym) {
                        foreach (string id in someVars) {
                            symbolTable.Insert(id, assembler.AllocateVariableMemory(VarType.Int));
                            symbolTable.SetType(id, VarType.Int);
                        }
                    }
                    else if (Tokens[currentToken].Type == TokenType.CharSym) {
                        foreach (string id in someVars) {
                            symbolTable.Insert(id, assembler.AllocateVariableMemory(VarType.Char));
                            symbolTable.SetType(id, VarType.Char);
                        }
                    }
                    else if (Tokens[currentToken].Type == TokenType.ArraySym) {
                        //k this requires some thinking..i probably have to allocate memory differently to use this
                        //this just results in a terrible nested if statement to keep error checking in line
                        //assuming example like array1:array[1..3] of <simpleType>
                        AdvanceToken();
                        if (Tokens[currentToken].Type == TokenType.LeftBracket) {
                            AdvanceToken();
                            if (Tokens[currentToken].Type == TokenType.Number) {
                                int startRange = int.Parse(Tokens[currentToken].Value);
                                AdvanceToken();
                                if (Tokens[currentToken].Type == TokenType.DoublePeriod) {
                                    AdvanceToken();
                                    if (Tokens[currentToken].Type == TokenType.Number) {
                                        int endRange = int.Parse(Tokens[currentToken].Value);
                                        AdvanceToken();
                                        if (Tokens[currentToken].Type == TokenType.RightBracket) {
                                            //almost there
                                            AdvanceToken();
                                            if (Tokens[currentToken].Type == TokenType.OfSym){
                                                AdvanceToken();
                                                //cool, basically got it
                                                if (Tokens[currentToken].Type == TokenType.IntegerSym || Tokens[currentToken].Type == TokenType.CharSym){
                                                    //check range...
                                                    if (startRange >= endRange) {
                                                        WriteError("Invalid Array Declaration: startrange must be less than endrange", Tokens[currentToken]);
                                                    }
                                                    TokenType type = Tokens[currentToken].Type;
                                                    if (type == TokenType.IntegerSym) {
                                                        foreach (string id in someVars) {
                                                            symbolTable.InsertArray(id, assembler.AllocateVariableMemory(VarType.Int, endRange - startRange), VarType.Int);
                                                        }
                                                    }
                                                    else {
                                                        foreach (string id in someVars) {
                                                            symbolTable.InsertArray(id, assembler.AllocateVariableMemory(VarType.Char, endRange - startRange), VarType.Char);
                                                        }
                                                    }
                                                }
                                                else WriteError("Invalid Array Declaration: expected char or int type", Tokens[currentToken]);
                                            }
                                            else WriteError("Invalid Array Declaration: expected of symbol", Tokens[currentToken]);
                                        }
                                        else WriteError("Invalid Array Declaration: expected right bracket", Tokens[currentToken]);
                                    }
                                    else WriteError("Invalid Array Declaration: expected end range int", Tokens[currentToken]);
                                }
                                else WriteError("Invalid Array Declaration: expected doubleperiod", Tokens[currentToken]);
                            }
                            else WriteError("Invalid Array Declaration: expected integer", Tokens[currentToken]);
                        }
                        else WriteError("Invalid Array Declaration: expected left bracket", Tokens[currentToken]);
                    }
                    else {
                        WriteError("Expected Variable Type", Tokens[currentToken]);
                    }
                }

                AdvanceToken();
            }
            return true;
        }

        private void Statement() {
            switch (Tokens[currentToken].Type) {
                case TokenType.Semicolon:
                    //need this to support empty statemnt
                    break;
                case TokenType.ReadSym:
                case TokenType.ReadLnSym:
                case TokenType.WriteSym:
                case TokenType.WriteLnSym:
                    //reserved words...call function
                    Function();
                    break;
                case TokenType.Identifier:
                    if (Tokens[currentToken + 1].Type == TokenType.LeftParen) {
                        //calling a function
                        Function();
                    }
                    //else assume assignment
                    else Assign();
                    break;
                case TokenType.IfSym:
                    IfStatement();
                    break;
                case TokenType.WhileSym:
                    WhileStatement();
                    break;
                case TokenType.BeginSym:
                    BeginStatement();
                    break;
                case TokenType.ProcedureSym:
                    ProcedureStatement();
                    break;
                case TokenType.EndSym:
                    //just break and return, let outer thing catch the end symbol for now
                    break;
                default:
                    //eat empty statement or anything else for the time being 
                    AdvanceToken();
                    break;
            }
        }

        private void Assign() {
            VarType vtype = VarType.Invalid, etype = VarType.Invalid;
            int varAddress = Variable(ref vtype, ref etype);
            etype = VarType.Invalid;
            if (Tokens[currentToken].Type != TokenType.Assign) {
                WriteError("Expected Assign Statement", Tokens[currentToken]);
            }
            AdvanceToken();
            Expr(0, ref etype);
            if (vtype != etype) {
                WriteErrorLine("Invalid Type Operation for assignment" + etype.ToString(), Tokens[currentToken]);
            }
            assembler.EmitStore(varAddress);
        }

        private void BeginStatement() {
            AdvanceToken();
            Statement();
            while (Tokens[currentToken].Type == TokenType.Semicolon) {
                AdvanceToken();
                Statement();
            }
            if (Tokens[currentToken].Type != TokenType.EndSym) {
                WriteError("Expected 'End' symbol", Tokens[currentToken]);
            }
            AdvanceToken();
        }

        private void Function() {
            VarType type = VarType.Invalid;
            VarType sub = VarType.Invalid;
            switch (Tokens[currentToken].Type) {
                case TokenType.ReadSym:
                case TokenType.ReadLnSym:
                    AdvanceToken();
                    if (Tokens[currentToken].Type != TokenType.LeftParen) {
                        WriteError("Expected '('", Tokens[currentToken]);
                    }
                    else {
                        AdvanceToken();
                        int address = Variable(ref type, ref sub);
                        if (type == VarType.Int) {
                            assembler.EmitRead(address, false);
                        }
                        else if (type == VarType.Char) {
                            assembler.EmitRead(address, true);
                        }
                        else {
                            WriteErrorLine("Expected variable or non-array", Tokens[currentToken]);
                        }
                        if (Tokens[currentToken].Type != TokenType.RightParen) {
                            WriteError("Expected ')'", Tokens[currentToken]);
                        }
                    }
                    break;
                case TokenType.WriteSym:
                    AdvanceToken();
                    if (Tokens[currentToken].Type != TokenType.LeftParen) {
                        WriteError("Expected '('", Tokens[currentToken]);
                    }
                    else {
                        AdvanceToken();
                        int address = Variable(ref type, ref sub);
                        if (type == VarType.Int) {
                            assembler.EmitWriteInt(address);
                        }
                        else if (type == VarType.Char) {
                            assembler.EmitWriteChr(address);
                        }
                        else if (type == VarType.String) {
                            assembler.EmitWriteString(address);
                        }
                        else if (type == VarType.Number) {
                            assembler.EmitWriteRawInt(address);
                        }
                        if (Tokens[currentToken].Type != TokenType.RightParen) {
                            WriteError("Expected ')'", Tokens[currentToken]);
                        }
                    }
                    break;
                case TokenType.WriteLnSym:
                    AdvanceToken();
                    if (Tokens[currentToken].Type != TokenType.LeftParen) {
                        WriteError("Expected '('", Tokens[currentToken]);
                    }
                    else {
                        AdvanceToken();
                        int address = Variable(ref type, ref sub);
                        if (type == VarType.Int) {
                            assembler.EmitWriteInt(address,true);
                        }
                        else if (type == VarType.Char) {
                            assembler.EmitWriteChr(address,true);
                        }
                        else if (type == VarType.String){
                            assembler.EmitWriteString(address, true);
                        }
                        else if (type == VarType.Number) {
                            assembler.EmitWriteRawInt(address,true);
                        }
                        if (Tokens[currentToken].Type != TokenType.RightParen) {
                            WriteError("Expected ')'", Tokens[currentToken]);
                        }
                    }
                    break;
                default:
                    if (symbolTable.GetLabel(Tokens[currentToken].Value) == null) {
                        WriteError("Unknown Function call", Tokens[currentToken]);
                    }
                    break;
            }
            while (Tokens[currentToken].Type != TokenType.Semicolon) {
                AdvanceToken();
                if (hitEndOfFile) {
                    break;
                }
            }
        }

        //here we compile a while statement, hope its works
        private void WhileStatement() {
            AdvanceToken();
            string label1 = symbolTable.GenerateLabel();
            string label2 = symbolTable.GenerateLabel();
            assembler.EmitLoc(label1);
            VarType type = VarType.Invalid;
            Expr(0, ref type);
            if (type != VarType.Bool) {
                WriteErrorLine("Invalid while statement", Tokens[currentToken]);
            }
            assembler.EmitJumpFalse(label2);
            if (Tokens[currentToken].Type != TokenType.DoSym) {
                WriteError("Expected 'Do' symbol", Tokens[currentToken]);
            }
            AdvanceToken();
            Statement();
            assembler.EmitJump(label1);
            assembler.EmitLoc(label2);
        }

        private void ProcedureStatement() {
            AdvanceToken();

            //next token should be an identifier...
            if (Tokens[currentToken].Type != TokenType.Identifier) {
                WriteError("Missing procedure identifier", Tokens[currentToken]);
            }

            //eat the input until semicolon
            while (Tokens[currentToken].Type != TokenType.Semicolon) {
                AdvanceToken();
                if (currentToken >= Tokens.Count - 1) {
                    WriteError("Expected Semicolon at end of procedure call", Tokens[Tokens.Count - 1]);
                }
            }
            AdvanceToken();

            //parse variables
            if (Tokens[currentToken].Type == TokenType.VarSym) {
                if (!ParseVariables()) {
                    WriteError("Error parsing variables", Tokens[Tokens.Count - 1]);
                }
            }

            if (Tokens[currentToken].Type != TokenType.BeginSym) {
                WriteError("Missing 'Begin' symbol", Tokens[currentToken]);
            }
            AdvanceToken();
            Statement();
            while (Tokens[currentToken].Type == TokenType.Semicolon) {
                AdvanceToken();
                Statement();
            }
            if (Tokens[currentToken].Type != TokenType.EndSym) {
                WriteError("Expected 'End' symbol", Tokens[currentToken]);
            }
        }

        private void IfStatement() {
            String label = symbolTable.GenerateLabel();
            AdvanceToken();
            symbolTable.GenerateLabel();
            VarType type = VarType.Invalid;
            Expr(0, ref type);
            if (type != VarType.Bool) {
                WriteErrorLine("Invalid if statement", Tokens[currentToken]);
            }

            if (Tokens[currentToken].Type != TokenType.ThenSym) {
                WriteError("Expected 'Then' symbol", Tokens[currentToken]);
            }
            AdvanceToken();
            assembler.EmitJumpFalse(label);
            Statement();
            if (Tokens[currentToken].Type != TokenType.ElseSym) {
                assembler.EmitLoc(label);
            }
            else {
                AdvanceToken();
                string label2 = symbolTable.GenerateLabel();
                assembler.EmitJump(label2);
                assembler.EmitLoc(label);
                Statement();
                assembler.EmitLoc(label2);
            }

        }

        private int Variable(ref VarType type, ref VarType subType) {
            if (Tokens[currentToken].Type != TokenType.Identifier && Tokens[currentToken].Type != TokenType.DoubleQuoteString && Tokens[currentToken].Type != TokenType.Number) {
                WriteError("Expected Identifier", Tokens[currentToken]);
                AdvanceToken();
                return 0;
            }
            int address = 0;
            if (symbolTable.Lookup(Tokens[currentToken].Value) == -1) {
                if (Tokens[currentToken].Type == TokenType.DoubleQuoteString) {
                    address = assembler.AllocateStaticMemory(Tokens[currentToken].Value);
                    type = VarType.String;
                    AdvanceToken();
                    return address;
                }
                if (Tokens[currentToken].Type == TokenType.Number) {
                    address = int.Parse(Tokens[currentToken].Value);
                    type = VarType.Number;
                    AdvanceToken();
                    return address;
                }
                //variable not found
                WriteErrorLine("Undeclared variable '" + Tokens[currentToken].Value +"'" , Tokens[currentToken]);
                type = VarType.Invalid;
            }
            else {
                address = symbolTable.Lookup(Tokens[currentToken].Value);
                type = symbolTable.GetType(Tokens[currentToken].Value);
            }

            AdvanceToken();

            //loading an array
            if (Tokens[currentToken].Type == TokenType.LeftBracket) {
                AdvanceToken();
                if (type != VarType.Invalid) {
                    if (type != VarType.Array) {
                        WriteErrorLine("Variable treated like array but not declared as such", Tokens[currentToken]);
                    }
                    else {
                        subType = symbolTable.GetArrayType(Tokens[currentToken-2].Value);
                    }
                }
                //else just eat the brackets, no need to error check, already threw an error
                VarType ttype = VarType.Invalid;
                Expr(0, ref ttype);
                if (Tokens[currentToken].Type != TokenType.RightBracket) {
                    WriteError("Expected end of array", Tokens[currentToken]);
                }
                AdvanceToken();
                if (ttype != VarType.Int)
                    WriteErrorLine("Expected int for array index", Tokens[currentToken]);
                //emit some stuff idk
            }

            return address;
        }

        private void Expr(int reg, ref VarType type) {
            SimpleExpr(reg, ref type);
            switch (Tokens[currentToken].Type) {
                case TokenType.Less:
                case TokenType.Greater:
                case TokenType.GreaterEqual:
                case TokenType.LessEqual:
                case TokenType.NotEqual:
                case TokenType.Equal:
                    VarType tType = VarType.Invalid;
                    AdvanceToken();
                    SimpleExpr(reg, ref tType);
                    if (tType == type) {
                        type = VarType.Bool;
                    }
                    else WriteErrorLine("Invalid Type Operation for expression", Tokens[currentToken]);
                    assembler.EmitCompare(Tokens[currentToken].Type);
                    break;
                default:
                    //not a comparative token, do nothing
                    break;
            }
        }

        private void SimpleExpr(int reg, ref VarType type) {
            VarType tType = VarType.Invalid, ttType = VarType.Invalid;
            Term(reg, ref tType);
            TokenType opType = Tokens[currentToken].Type;
            while (Tokens[currentToken].Type == TokenType.Plus || Tokens[currentToken].Type == TokenType.Minus || Tokens[currentToken].Type == TokenType.OrSym) {
                opType = Tokens[currentToken].Type;
                AdvanceToken();
                Term(reg + 1, ref ttType);
                if (opType == TokenType.Plus) {
                    //assembler.EmitRegisterRegister("A", reg, reg + 1);
                    assembler.EmitAdd();
                }
                else if (opType == TokenType.Minus) {
                    assembler.EmitMinus();
                }
                else if (opType == TokenType.OrSym) {
                    //do something, not sure what
                }
                if (ttType != tType) {
                    WriteErrorLine("Invalid Type Operation for expression", Tokens[currentToken]);
                }
            }
            type = tType;
        }

        private void Term(int treg, ref VarType type) {
            int mreg=0, dreg=0;
            Factor(treg, ref type);
            VarType ttype = VarType.Invalid;
            while (Tokens[currentToken].Type == TokenType.Times || Tokens[currentToken].Type == TokenType.DivSym || Tokens[currentToken].Type == TokenType.ModSym) {
                TokenType opType = Tokens[currentToken].Type;
                AdvanceToken();
                Factor(mreg, ref ttype);
                if (opType == TokenType.Times) {
                    assembler.EmitMultiply();
                }
                else if (opType == TokenType.DivSym) {
                    assembler.EmitDivision();
                }
                else {
                    //not sure for mod
                }
                if (ttype != type) {
                    WriteErrorLine("Invalid Type Conversion for term", Tokens[currentToken]);
                }

            }
        }

        private void Factor(int reg, ref VarType type) {
            switch (Tokens[currentToken].Type) {
                case TokenType.Number:
                    //assembler.EmitLoad(reg, int.Parse( Tokens[currentToken].Value));
                    assembler.PushInt(int.Parse(Tokens[currentToken].Value));
                    type = VarType.Int;
                    break;
                case TokenType.Identifier:
                    if (symbolTable.Lookup(Tokens[currentToken].Value) == -1) {
                        WriteError("Expected known variable", Tokens[currentToken]);
                        type = VarType.Invalid;
                    }
                    else {
                        //assembler.EmitLoadRegAddr(reg, symbolTable.Lookup(Tokens[currentToken].Value));
                        type = symbolTable.GetType(Tokens[currentToken].Value);
                        assembler.PushVariable(symbolTable.Lookup(Tokens[currentToken].Value));
                    }
                    break;
                case TokenType.QuoteString:
                    //assembler.EmitLoad(reg, Tokens[currentToken].Value.ToCharArray()[0]);
                    assembler.PushChar(Tokens[currentToken].Value.ToCharArray()[0]);
                    type = VarType.Char;
                    break;
                case TokenType.NotSym:
                    AdvanceToken();
                    //no idea what to emit here...
                    Factor(reg, ref type);
                    break;
                case TokenType.LeftParen:
                    AdvanceToken();
                    Expr(reg, ref type);
                    if (Tokens[currentToken].Type != TokenType.RightParen) {
                        WriteError("Expected Right parenthesis", Tokens[currentToken]);
                    }
                    break;
                case TokenType.ReadLnSym:
                case TokenType.ReadSym:
                case TokenType.WriteLnSym:
                case TokenType.WriteSym:
                    WriteError("Invalid function use in expression", Tokens[currentToken]);
                    break;
                case TokenType.OrdSym:
                case TokenType.ChrSym:
                    //these should be fun...
                    break;
                default:
                    WriteError("Expected Number or Identifier", Tokens[currentToken]);
                    break;
            }
            AdvanceToken();
        }

        private void AdvanceToken() {
            if (currentToken == Tokens.Count - 1) {
                if (!hitEndOfFile)
                    WriteErrorLine("Unexpected end of file", Tokens[currentToken]);
                hitEndOfFile = true;
            }
            else {
                currentToken++;
            }
        }

        private void WriteError(string error, Token token){
            Console.WriteLine("[Parser Error]: " + error + " but got " + token.Value + " at line " + token.LineNum);
            parseSuccessful=false;
        }

        private void WriteErrorLine(string error, Token token) {
            Console.WriteLine("[Parser Error]: " + error + " at line " + token.LineNum);
            parseSuccessful = false;
        }
    }
}
