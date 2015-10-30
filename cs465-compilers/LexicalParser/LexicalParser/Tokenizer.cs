using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalParser {
    public class Tokenizer {

        #region Private Variables

        //holds on to what we have processed so far
        private Token lastFoundToken;
        private String currentStream;

        //quick n dirty state machine enum
        private enum State {
            InitialState,
            Character,
            Operator,
            SingleQuote,
            Number,
            Comment,
        }

        private State currentState = State.InitialState;

        #region TokenExpectedValueDictionaries

        //dictionary to hold expected values for values of reserved words
        private Dictionary<String, TokenType> ExpectedReserveTokens = new Dictionary<String, TokenType>() {
            #region ReservedDefines
            {"and", TokenType.AndSym},
            {"array", TokenType.ArraySym},
            {"begin", TokenType.BeginSym},
            {"char", TokenType.CharSym},
            {"chrsym", TokenType.ChrSym},
            {"div", TokenType.DivSym},
            {"do", TokenType.DoSym},
            {"else", TokenType.ElseSym},
            {"end",TokenType.EndSym},
            {"if",TokenType.IfSym},
            {"integer",TokenType.IntegerSym},
            {"mod",TokenType.ModSym},
            {"not",TokenType.NotSym},
            {"of", TokenType.OfSym},
            {"or", TokenType.OrSym},
            {"ord", TokenType.OrdSym},
            {"procedure", TokenType.ProcedureSym},
            {"program", TokenType.ProgramSym},
            {"read", TokenType.ReadSym},
            {"readln", TokenType.ReadLnSym},
            {"then", TokenType.ThenSym},
            {"var", TokenType.VarSym},
            {"while", TokenType.WhileSym},
            {"write", TokenType.WriteSym},
            {"writeln", TokenType.WriteLnSym},
            #endregion
        };

        //dictionary to hold expected values for operators
        private Dictionary<String, TokenType> ExpectedOperatorTokens = new Dictionary<String, TokenType>() {
            #region OperatorDefines
            {"+", TokenType.Plus},
            {"-", TokenType.Minus},
            {"*", TokenType.Times},
            {"<", TokenType.Less},
            {"<=", TokenType.LessEqual},
            {"=<", TokenType.LessEqual},
            {"<>", TokenType.NotEqual},
            {">", TokenType.Greater},
            {">=", TokenType.GreaterEqual},
            {"=>", TokenType.GreaterEqual},
            {"=", TokenType.Equal},
            {":=", TokenType.Assign},
            {":", TokenType.Colon},
            {";", TokenType.Semicolon},
            {",", TokenType.Comma},
            {"(", TokenType.LeftParen},
            {"(.", TokenType.LeftBracket},
            {")", TokenType.RightParen},
            {".)",TokenType.RightBracket},
            {".", TokenType.Period},
            #endregion
        };

        #endregion

        #endregion

        #region Process State Functions

        //initialstate function
        //true if invalid token, false for no token to report
        private bool ProcessInitialState(Char character) {
            currentStream = "";
            //lets do this one first, not sure if any of the other cheatchecks will steal this from us
            if (character == '\'') {
                //eat the quote, go to state
                currentState = State.SingleQuote;
            }
            else if (Char.IsLetter(character)) {
                //parse this 'string'
                currentStream += character;
                currentState = State.Character;
            }

            else if (Char.IsWhiteSpace(character)) {
                //eat dat whitespace, nom nom
            }

            else if (Char.IsNumber(character)) {
                //k we got a number
                currentStream += character;
                currentState = State.Number;
            }
            //this one gets hairy, IsSymbol wont catch all our cases,
            //the idea is the piggyback off a single char in dict, then figure out if theres more in the state
            else if (ExpectedOperatorTokens.ContainsKey(character.ToString())) {
                currentStream += character;
                currentState = State.Operator;
            }

            else { //well, i got nothin, illegal token it is
                lastFoundToken = new Token(TokenType.Illegal, character.ToString());
                return true;
            }
            return false; //only return true for an error
        }

        private bool ProcessCharacterState(Char character) {
            bool updateToken = false;
            string tempStream = "";
            if (Char.IsWhiteSpace(character)) {
                //easy, eat the whitespace and check the token, hold in our buffer
                updateToken = true;
                currentState = State.InitialState;
                tempStream = currentStream;
            }
            else if (ExpectedOperatorTokens.ContainsKey(character.ToString())) {
                //little bit more tough, check token but also have to
                //spin up initialstate otherwise we will lose a char
                updateToken = true;
                tempStream = currentStream;
                currentStream += character;
                if (ProcessInitialState(character))
                    return true; //ugh..invalid next token, kill me now
            }
            else if (Char.IsLetterOrDigit(character)) {
                //easy, lets keep gettin more characters
                currentStream += character;
                return false;
            }
            else {
                //bad symbol
                lastFoundToken = new Token(TokenType.Illegal, character.ToString());
                lastFoundToken.ErrorMsg = "Expected Letter or Digit";
                return true;
            }
            if (updateToken) {
                //We have a token! Lets find a type for it,
                //dont touch state here
                if (ExpectedReserveTokens.ContainsKey(tempStream)) {
                    lastFoundToken = new Token(ExpectedReserveTokens[tempStream], tempStream);
                }
                else {
                    //must just be an identifier
                    lastFoundToken = new Token(TokenType.Identifier, tempStream);
                }
                return true;
            }
            else return false; // no token found so idk

            //this shouldnt happen, i guess make it known
            lastFoundToken = new Token(TokenType.Illegal, character.ToString());
            lastFoundToken.ErrorMsg = "Hit unknown condition for Character State! Token: " + character.ToString();
            return true;
        }

        private bool ProcessOperatorState(Char character) {
            //lets figure out what symbol we should report back!
            if (Char.IsWhiteSpace(character)) {
                //lets just verify what we have so far
                if (ExpectedOperatorTokens.ContainsKey(currentStream)) {
                    lastFoundToken = new Token(ExpectedOperatorTokens[currentStream], currentStream);
                    //not much more to see here
                    currentState = State.InitialState;
                    return true;
                }
                else {
                    //ok, u want to give me an invalid symbol
                    lastFoundToken = new Token(TokenType.Illegal, currentStream);
                    lastFoundToken.ErrorMsg = "Invalid Symbol/Operator!";
                    return true;
                }
            }
            else if (ExpectedOperatorTokens.ContainsKey(character.ToString())){
                //your not a comment are you? i dont want those
                if (currentStream+character == "(*"){
                    currentState = State.Comment;
                    return false;
                }
                if (currentStream + character == "*)") {
                    //ugh...end comment without a beginning? thats a paddlin
                    lastFoundToken = new Token(TokenType.Illegal, currentStream);
                    lastFoundToken.ErrorMsg = "Unmatched end of comment";
                    return true;
                }
                //another symbol eh? check if still valid
                if (ExpectedOperatorTokens.ContainsKey(currentStream+character)){
                    lastFoundToken = new Token(ExpectedOperatorTokens[currentStream+character], currentStream+character);
                    currentState = State.InitialState;
                    return true;
                }
                else {
                    if (currentStream.Length == 2){
                        //somethings fubared
                        lastFoundToken = new Token(TokenType.Illegal, currentStream);
                        lastFoundToken.ErrorMsg = "Illegal Symbol/Operator";
                        return true;
                    }
                    // just return the last token and spin up initial to delay parse of next symbol
                    // on paper this should always be found
                    lastFoundToken = new Token(ExpectedOperatorTokens[currentStream], currentStream);
                    if ( ProcessInitialState(character))
                        return true; //ugh, invalid
                    else return true;
                }
            }
            else if (character == '\'') {
                //verify what we have, then send the char to initial state to take care of it all
                if (ExpectedOperatorTokens.ContainsKey(currentStream)) {
                    lastFoundToken = new Token(ExpectedOperatorTokens[currentStream], currentStream);
                    if (ProcessInitialState(character))
                        return true; //invalid yey
                    else return true;
                }
                else {
                    //invalid? report it
                    lastFoundToken = new Token(TokenType.Illegal, currentStream);
                    lastFoundToken.ErrorMsg = "Invalid Symbol/Operator";
                    return true;
                }
            }

            else if (Char.IsLetterOrDigit(character)) {
                //no longer a symbol, on paper, what we have should always be valid
                lastFoundToken = new Token(ExpectedOperatorTokens[currentStream], currentStream);
                //spin up initial to check next char
                if (ProcessInitialState(character))
                    return true;
                else return true;
            }
            else {
                //ugh what?
                lastFoundToken = new Token(TokenType.Illegal, currentStream);
                lastFoundToken.ErrorMsg = "State Machine broke processing symbol";
                return true;
            }
        }

        private bool ProcessSingleQuoteState(Char character) {
            //this state is easy
            if (character == '\'') {
                //cool end of our 'string'
                lastFoundToken = new Token(TokenType.QuoteString, currentStream);
                currentState = State.InitialState;
                return true;
            }
            else {
                //keep taking chars for our string
                currentStream += character;
                return false;
            }
        }

        private bool ProcessNumberState(Char character) {
            //simple state, only take in more numbers until space or symbol
            if (Char.IsNumber(character)) {
                currentStream += character;
                return false;
            }
            else if (Char.IsWhiteSpace(character)) {
                //got a token!
                lastFoundToken = new Token(TokenType.Number, currentStream);
                currentState = State.InitialState;
                return true;
            }
            else if (ExpectedOperatorTokens.ContainsKey(character.ToString())) {
                //still a token
                lastFoundToken = new Token(TokenType.Number, currentStream);
                //spin up initial state
                if (ProcessInitialState(character))
                    return true; //ugh, invalid character is next up
                else return true;
            }
            else {
                //should be everything else
                lastFoundToken = new Token(TokenType.Illegal, currentStream);
                lastFoundToken.ErrorMsg = "Invalid Number expression";
                return true;
            }
        }

        private bool ProcessCommentState(Char character) {
            //pretty easy again
            if (currentStream.Length <= 1) {
                //not done
                currentStream += character;
                return false;
            }
            else if ((currentStream.Substring(currentStream.Length - 1) + character) == "*)") {
                //free!
                currentState = State.InitialState;
                return false;
            }
            else {
                currentStream += character;
                return false;
            }
        }

        #endregion

        public Tokenizer() { }

        //this will return false if no token can be reported, otherwise true, use gettoken to grab it
        public bool ProcessChar(Char character) {
            //lets make a simple state machine to figure out what we should be doing with things
            switch (currentState) {
                case State.InitialState:
                    //the world is our oyster!
                    return ProcessInitialState(character);
                case State.Character:
                    return ProcessCharacterState(character);
                case State.Operator:
                    return ProcessOperatorState(character);
                case State.SingleQuote:
                    return ProcessSingleQuoteState(character);
                case State.Number:
                    return ProcessNumberState(character);
                case State.Comment:
                    return ProcessCommentState(character);
            }
            //this shouldnt ever happen,just making compiler of our compiler happy
            return true;
        }

        //call at end of line, will return last token, or null if theres none to report
        //or illegal token if theres a token error
        public Token EndOfLine() {
            switch (currentState) {
                case State.InitialState:
                    //nothing to report, all whitespace
                    return null;
                case State.Character:
                    //cheat and call the function with a 'space' character;
                    ProcessCharacterState(' ');
                    break;
                case State.Operator:
                    ProcessOperatorState(' ');
                    break;
                case State.Number:
                    ProcessNumberState(' ');
                    break;
                case State.SingleQuote:
                    // rut rho!
                    lastFoundToken = new Token(TokenType.Illegal, currentStream);
                    lastFoundToken.ErrorMsg = "Missing Matching Single Quote";
                    break;
                case State.Comment:
                    //rut rho!
                    lastFoundToken = new Token(TokenType.Illegal, currentStream);
                    lastFoundToken.ErrorMsg = "Missing Matching Comment Symbol!";
                    break;
            }
            return lastFoundToken;
        }

        //this should only be called when processchar comes out to true
        //should always check to make sure its not an illegal token
        public Token GetToken() {
            return lastFoundToken;
        }

        public static Token EndOfFile() {
            return new Token(TokenType.EOfSym, "");
        }

    }
}
