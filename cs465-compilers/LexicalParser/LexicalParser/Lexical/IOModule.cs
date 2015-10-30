using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalParser {
    public class IOModule {

        private String filename;
        private List<Token> tokens = new List<Token>();

        public IOModule(String file) {
            filename = file;
            if (File.Exists(filename) == false) {
                //idiot...probly should make a new exception but..meh
                throw new LexicalException(0,"File Doesn't Exist");
            }
        }

        public List<Token> ParseFileTokens() {
            String line;
            int lineNum = 0;
            //Tokenizer tokenize = new Tokenizer();
            //well, here we go, wont stop till the files empty!
            using (StreamReader reader = new StreamReader(filename)) {
                while ((line = reader.ReadLine()) != null) {
                    lineNum++;
                    //process current line of toks
                    GetLineTokens(lineNum,line);
                }
            }
            //nothing blew up...i guess return the list
            //throw up an end of file token as well
            VerifyAddToken(lineNum,Tokenizer.EndOfFile());
            return tokens;
        }

        private void GetLineTokens(int lineNum, String line) {
            Tokenizer tokenize = new Tokenizer();
            //bombard tokenizer with chars from the string until we get a valid token
            foreach (char c in line) {
                if (tokenize.ProcessChar(c)) {
                    //hey a token, lets hold onto it if its valid
                    VerifyAddToken(lineNum, tokenize.GetToken());
                }
            }
            //signal endline, make sure all is well
            VerifyAddToken(lineNum, tokenize.EndOfLine());
        }

        private void VerifyAddToken(int lineNum, Token token) {
            if (token != null) {
                if (token.Type == TokenType.Illegal) {
                    throw new LexicalException(lineNum, token.ErrorMsg);
                }
                else {
                    token.LineNum = lineNum;
                    tokens.Add(token);
                }
            }
        }
    }
}
