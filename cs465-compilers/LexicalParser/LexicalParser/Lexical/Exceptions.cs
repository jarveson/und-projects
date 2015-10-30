using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalParser {
    class Exceptions {
    }

    [Serializable]
    public class LexicalException : Exception {
        readonly Int32 lineNumber;
        readonly String message;

        public LexicalException() { }
        public LexicalException(Int32 line)
            : base(FormatMessage(line)) {
                this.lineNumber = line;
        }

        public LexicalException(Int32 line, String Message)
            : base(FormatMessage(line, Message)) {
            this.lineNumber = line;
            this.message = Message;
        }

        public LexicalException(String Message, Exception inner)
            : base(Message, inner) {
        }

        protected LexicalException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
                if (info == null) {
                    throw new ArgumentNullException("info");
                }
                this.lineNumber = info.GetInt32("lineNumber");
                this.message = info.GetString("message");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }
            info.AddValue("lineNumber", this.lineNumber);
            info.AddValue("message", this.message);
            base.GetObjectData(info, context);
        }

        public Int32 LineNumber { get { return this.lineNumber; } }

        public String LexMessage { get { return this.message; } }

        static String FormatMessage(Int32 line) {
            return String.Format("[Lex Error]: At line {0}. ", line); 
        }

        static String FormatMessage(Int32 line, String message) {
            if (line == 0) {
                return String.Format("[Lex Error]: {0}.", message);
            }
            return String.Format("[Lex Error]: Line {0}. {1}", line, message);
        }
    }
}
