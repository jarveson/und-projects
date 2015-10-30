using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalParser.Parser {
    public class Assembler {
        string filename;
        int variableCounter = 0;
        int staticCounter = 0;
        int labelCounter = 0;
        /* Dont need for x86 masm
        const int startAddressVariables = 72;
        int variableAllocAddress = -1; // R12
        int stackPointer = 0; //r13
         */
        //string progName;

        //holds our data until we get everything
        List<string> dataBuffer = new List<String>();
        List<string> codeBuffer = new List<string>();
        List<string> staticBuffer = new List<string>();

        /* not needed for x86 masm
        //keep format in its own variable,
        // we got <label> <operation> <statement> 
        string lineFormat = "{0,-9} {1,-8} {2,-40}";
        */
          
        //constructor, with default filename to output to
        public Assembler(string progname, string FileName = "output.asm") {
            //progName = progname.Substring(0, 6); dont care about progname for x86
            filename = FileName;
            //throw up some boiler plate stuff
            //add in buffer data declaration, also holder variables for read/write handles
            dataBuffer.Add("inbuf db 10 dup (?)");
            using (StreamWriter sr = new StreamWriter(filename)) {
                /*sr.WriteLine(lineFormat, progName, "CSECT", " ");
                sr.WriteLine(lineFormat, " ", "STM", "14,12,12(13)");
                sr.WriteLine(lineFormat, " ", "LR", "12,15");
                sr.WriteLine(lineFormat, " ", "LA", "15,SAVE");
                sr.WriteLine(lineFormat, " ", "ST", "15,8(13)");
                sr.WriteLine(lineFormat, " ", "ST", "13,4(15)");
                sr.WriteLine(lineFormat, " ", "LR", "13,15");
                sr.WriteLine(lineFormat, "*", " ", " ");*/
                sr.WriteLine(".386");
                sr.WriteLine(".model flat,stdcall");
                sr.WriteLine("option casemap:none");
                sr.WriteLine("include windows.inc");
                sr.WriteLine("include masm32.inc");
                sr.WriteLine("include kernel32.inc");
                sr.WriteLine("includelib kernel32.lib");
                sr.WriteLine("includelib masm32.lib");
                sr.WriteLine("Main PROTO");
                sr.WriteLine("");

            }
        }

        public void FinalizeCode() {
            //finish writing our code
            using (StreamWriter sr = new StreamWriter(filename, true)) {
                //write out our .data 
                sr.WriteLine(".data");
                foreach (string line in staticBuffer) {
                    sr.WriteLine(line);
                }
                sr.WriteLine("");

                sr.WriteLine(".data?");
                foreach (string line in dataBuffer) {
                    sr.WriteLine(line);
                }
                sr.WriteLine("");

                //write out code
                sr.WriteLine(".code");
                sr.WriteLine("start:");

                sr.WriteLine("invoke Main");
                sr.WriteLine("invoke ExitProcess, 0");

                sr.WriteLine("Main Proc");

                foreach (string line in codeBuffer) {
                    sr.WriteLine(line);
                }
                sr.WriteLine("RET");
                sr.WriteLine("Main EndP");


                // boilerplate code to end the assembly code
                /*sr.WriteLine(lineFormat, "*", " ", " ");
                sr.WriteLine(lineFormat, " ", "SR", "15,15");
                sr.WriteLine(lineFormat, " ", "BR", "14");
                sr.WriteLine(lineFormat, "SAVE", "DS", "18F");
                sr.WriteLine(lineFormat, "END", progName, " ");*/
                sr.WriteLine("end start");
            }
        }

        public void RemoveFile() {
            File.Delete(filename);
        }

        //allocates variable memory, returns address /numeric identifier
        //size gives us declaration size for arrays,
        //type gives us dword or byte
        public int AllocateVariableMemory(VarType type, int size=1) {
            //cheating and just putting a 'v' for variable and taking on the counter
            variableCounter++;
            if (type == VarType.Int){
                dataBuffer.Add("v"+variableCounter +" dd " + ((size>1) ? size + "dup (?)" : "?"));
            }
            else {
                dataBuffer.Add("v"+variableCounter +" db " + ((size>1) ? size + "dup (?)" : "?"));
            }
            return variableCounter;
        }

        public int AllocateStaticMemory(string quote) {
            staticCounter++;
            staticBuffer.Add("s" + staticCounter + " db \"" + quote + "\",0h");
            return staticCounter;

        }

        public void EmitStore(int address) {
            codeBuffer.Add("pop ECX"); // should be value
            codeBuffer.Add("mov v" + address + ",ECX");
        }

        public void EmitLoad(int register, int value) {
            using (StreamWriter sr = new StreamWriter(filename, true)) {
                //sr.WriteLine(lineFormat, " ", "LA", register + "," + value + "(," + register + ")");
            }
        }

        public void EmitLoadRegAddr(int register, int address) {
            using (StreamWriter sr = new StreamWriter(filename, true)) {
                //sr.WriteLine(lineFormat, " ", "S", register + "," + register);
                //sr.WriteLine(lineFormat, " ", "L", register + "," + address + "(0," + register + ")");
            }
        }

        public void EmitRegisterRegister(string Operation, int reg1, int reg2) {
            using (StreamWriter sr = new StreamWriter(filename, true)) {
                //sr.WriteLine(lineFormat, " ", Operation, reg1 + "," + reg2);
            }
        }

        public void EmitShiftRightDoubleArith(int register, int bits) {
            using (StreamWriter sr = new StreamWriter(filename, true)) {
                //sr.WriteLine(lineFormat, " ", "SRDA", register + "," + bits);
            }
        }

        public void EmitCompare(TokenType type) {
            //do stuff depending on the comparison operator
        }

        public void EmitAdd() {
            codeBuffer.Add("pop ECX");
            codeBuffer.Add("pop EAX");
            codeBuffer.Add("add EAX,ECX");
            codeBuffer.Add("push EAX");
        }

        public void EmitMinus() {
            codeBuffer.Add("pop ECX");
            codeBuffer.Add("pop EAX");
            codeBuffer.Add("sub EAX,ECX");
            codeBuffer.Add("push EAX");
        }

        public void EmitLoc(string label) {
        }

        public void EmitJumpFalse(string label) {
        }

        public void EmitJump(string label) {
        }

        public void EmitMultiply() {
            codeBuffer.Add("pop ECX");
            codeBuffer.Add("pop EBX");
            codeBuffer.Add("invoke IntMul, ECX, EBX");
            codeBuffer.Add("push EAX");
        }

        public void EmitDivision() {
            codeBuffer.Add("pop ECX");
            codeBuffer.Add("pop EBX");
            codeBuffer.Add("invoke IntDiv, EBX, ECX");
            codeBuffer.Add("push EAX");
        }

        public void PushInt(int intToPush) {
            codeBuffer.Add("push " + intToPush);
        }

        public void PushChar(char charToPush) {
            codeBuffer.Add("push '" + charToPush+"'");
        }

        public void PushVariable(int address) {
            codeBuffer.Add("mov EAX, v" + address);
            codeBuffer.Add("push EAX");
        }

        public void EmitRead(int address, bool isChar) {
            //read char into variable address name
            codeBuffer.Add("invoke StdIn, addr inbuf, sizeof inbuf");
            if (isChar) {
                codeBuffer.Add("xor EBX,EBX");
                codeBuffer.Add("mov BL, BYTE PTR[inbuf]");
                codeBuffer.Add("mov v"+address+", BL");
                // codeBuffer.Add("mov [ECX], BL");
            }
            else {
                codeBuffer.Add("invoke atodw, addr inbuf");
                codeBuffer.Add("mov v"+address+", EAX");
            }
        }

        public void EmitWriteChr(int address, bool endline = false) {
            codeBuffer.Add("invoke StdOut, addr v" + address);
            if (endline) {
                EmitNewLine();
            }

        }
        public void EmitWriteInt(int address, bool endline = false) {
            codeBuffer.Add("invoke dwtoa, v"+address+", addr inbuf");
            codeBuffer.Add("invoke StdOut, addr inbuf");
            if (endline) {
                EmitNewLine();
            }
        }

        public void EmitWriteString(int address, bool endline = false) {
            codeBuffer.Add("invoke StdOut, addr s" + address);
            if (endline) {
                EmitNewLine();
            }
        }

        public void EmitWriteRawInt(int value, bool endline = false) {
            codeBuffer.Add("invoke dwtoa, " + value + ", addr inbuf");
            codeBuffer.Add("invoke StdOut, addr inbuf");
            if (endline) {
                EmitNewLine();
            }
        }

        private void EmitNewLine() {
            codeBuffer.Add("mov ECX, Offset inbuf");
            codeBuffer.Add("mov BYTE PTR[ECX], 0ah");
            codeBuffer.Add("mov BYTE PTR[ECX+1], 0h");
            codeBuffer.Add("invoke StdOut, addr inbuf");
        }
    }
}
