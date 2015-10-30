using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalParser {
    class Program {
        static void Main(string[] args) {
            List<Token> tokens = null;
#if DEBUG
            if (args.Length != 1) {
                Console.Write("[DEBUG] Enter Filename: ");
                args = new string[1];
                args[0] = Console.ReadLine();
            }
#endif
            if (args.Length != 1) {
                Console.WriteLine("Run program with file as argument!");
            }
            else {
                try {
                    IOModule ioMod = new IOModule(args[0]);
                    tokens = ioMod.ParseFileTokens();
                }
                catch (LexicalException e) {
                    Console.WriteLine(e.Message);
                    return;
                }
                Console.WriteLine("{0,20}{1,20}", "TokenType", "Value");
                Console.WriteLine("{0,20}{1,20}", "----------", "----------");

                tokens.ForEach(delegate(Token token) {
                    Console.WriteLine("{0,20}{1,20}", token.Type, token.Value);
                });

                Parser.Parser parser = new Parser.Parser();
                if (parser.Parse(tokens)) {
                    Console.WriteLine("Parse Complete!");
                }
                else {
                    Console.WriteLine("Parsing Failed!");
                }
            }
            Console.WriteLine("Press Enter to continue");
            //add a pause 
            Console.ReadLine();
        }
    }
}
