using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalParser.Parser {
    public enum VarType {
        Invalid,
        Int,
        Bool,
        Char,
        String,
        Array,
        Number,
    }
    public class SymbolTable {

        Dictionary<string, int> Symbols = new Dictionary<string, int>();
        //holds types for variables
        Dictionary<string, VarType> VarTypes = new Dictionary<string, VarType>();
        //holds types for arrays
        Dictionary<string, VarType> ArrayTypes = new Dictionary<string, VarType>();

        Dictionary<string, string> Labels = new Dictionary<string, string>();

        int currentLabelNum = 0;

        String currentLabel = "A";

        //Insert returns true if successful, false if already in table
        public bool Insert(string identifier, int addr){
            if (Symbols.ContainsKey(identifier)) {
                return false;
            }
            else {
                Symbols.Add(identifier, addr);
                return true;
            }
        }

        //inserts array, true if successful, false if already in table
        public bool InsertArray(string identifier, int addr, VarType type) {
            if (Symbols.ContainsKey(identifier)) {
                return false;
            }
            else {
                Symbols.Add(identifier, addr);
                ArrayTypes.Add(identifier, type);
                SetType(identifier, VarType.Array);
                return true;
            }

        }

        //returns address of symbol if found, -1 if not found
        public int Lookup(string identifier) {
            if (Symbols.ContainsKey(identifier)) {
                return Symbols[identifier];
            }
            else return -1;
        }

        public bool SetType(string id, VarType varType) {
            if (VarTypes.ContainsKey(id)) {
                return false;
            }
            else {
                VarTypes.Add(id, varType);
                return true;
            }
        }


        public VarType GetArrayType(string identifier) {
            return ArrayTypes[identifier];
        }

        public bool SetArrayType(string id, VarType type) {
            if (ArrayTypes.ContainsKey(id)) {
                return false;
            }
            else {
                ArrayTypes.Add(id, type);
                return true;
            }
        }

        public VarType GetType(string id) {
            return VarTypes[id];
        }

        // here we return or generate a label for a program/function
        //currently assuming we wont have more than about 10000 labels
        public String GetLabel(string id) {
            if (Labels.ContainsKey(id)) {
                return Labels[id];
            }

            return null;
        }

        public String GenerateLabel() {
            string temp = currentLabel + currentLabelNum;
            currentLabelNum++;
            return temp;
        }
    }
}
