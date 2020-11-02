using System;
using System.Collections.Generic;
using System.IO;

namespace Lang01
{
    class Program
    {
        enum KEYWORD
        {
            // Function
            KW_FUNC, KW_LOCAL,
            KW_PUSH, KW_NIL,
            KW_RETURN, KW_CALL,
            // Branching
            KW_JMP, KW_JMP_TRUE, KW_JMP_FALSE,
            // Operator
            KW_ADD, KW_SUB, KW_DIV, KW_MUL,
            KW_GT, KW_LT, KW_LTE, KW_GTE, KW_EQ, KW_NEQ
        };

        static readonly List<string> TOKENS = new List<string>(){
            "local","func",
            "push", "nil?",
            "jmp", "jmp?", "jmp!",
            "ret", "call",
            "(", ")", "[", "]",
            "+", "-", "/", "*",
            ">", "<", "<=", ">=", "==", "!="
        };

        static readonly char[] DELIMITER = {
            ' '
        };


        static readonly List<string> typenames = new List<string>(){
            "nil", "number", "symbol", "string"
        };

        public class MObject
        {
            public int line;
            public object value;
            public MObject(int line){ this.line = line; }
            public MObject(object value){this.value = value;}
            public MObject(int line, object value){ this.line = line; this.value = value; }
            
            public virtual MObject Eval(Dictionary<string, MObject> env) { throw new Exception("Undefined MObject."); }
        }

        class Local : MObject { public Local (int line, object value) : base(line, value){} }
        class Var : MObject { public Var (int line, object value) : base(line, value){} }
        class Boolean : MObject { public Boolean (int line, object value) : base(line, value){} }
        class Number : MObject { public Number (int line, object value) : base(line, value){} }
        class String : MObject { public String (int line, object value) : base(line, value){} }
        class Symbol : MObject { public Symbol (int line, object value) : base(line, value){} }
        class Array : MObject { public Array (int line, object value) : base(line, value){} }
        class Expr : MObject { public Expr (int line, object value) : base(line, value){} }
        class Func : MObject { public Func (int line, object value) : base(line, value){} }
        class End : MObject { public End (int line) : base(line){} }

        class Push : MObject { public Push (int line, object value) : base(line, value){} }
        class NilQ : MObject { public NilQ (int line, object value) : base(line, value){} }
        
        class Label : MObject { public Label (int line, object value): base(line, value){} }
        class Jmp : MObject { public Jmp (int line, object value) : base(line, value){} }
        class JmpTrue : MObject { public JmpTrue (int line, object value) : base(line, value){} }
        class JmpFalse : MObject { public JmpFalse (int line, object value) : base(line, value){} }

        class Ret : MObject { public Ret (int line, object value) : base(line, value){} }
        class Call : MObject { public Call (int line, object value) : base(line, value){} }

        class Operator : MObject { public Operator (int line, object value): base(line, value){} }
        class Add : MObject { public Add (int line, object value) : base(line, value){} }
        class Sub : MObject { public Sub (int line, object value) : base(line, value){} }
        class Div : MObject { public Div (int line, object value) : base(line, value){} }
        class Mul : MObject { public Mul (int line, object value) : base(line, value){} }

        class CompareGT : MObject { public CompareGT (int line, object value) : base(line, value){} }
        class CompareLT : MObject { public CompareLT (int line, object value) : base(line, value){} }
        class CompareLE : MObject { public CompareLE (int line, object value) : base(line, value){} }
        class CompareGE : MObject { public CompareGE (int line, object value) : base(line, value){} }
        class CompareEQ : MObject { public CompareEQ (int line, object value) : base(line, value){} }
        class CompareNQ : MObject { public CompareNQ (int line, object value) : base(line, value){} }



        static List<string> sourceLines = new List<string>();
        static LinkedList<MObject> memory = new LinkedList<MObject>();


        // interpreter state
        static Stack<object> executionStack = new Stack<object>();
        static Dictionary<string, object> envMap = new Dictionary<string, object>();
        static List<string> AccParamsOnly(string[] pargs){
            if(pargs.Length < 3) throw new Exception("AccParamsOnly -- not enough args for [] or ()");

            var exprList = new List<string>();
            for(int e = 1; e < pargs.Length-1; e++){
                exprList.Add(pargs[e]);
            }
            return exprList;
        }
        static List<MObject> MObjectFromString(int line, List<string> parameters)
        {
            var acc = new List<MObject>();
            var pargs = parameters.ToArray();
            for(int i = 0; i < pargs.Length;i++){
                var parameter = pargs[i];
                switch(parameter){
                                
                    // Braces
                    case "(": 
                        var expr = new Expr(line, MObjectFromString(line, AccParamsOnly(pargs)));
                        acc.Add(expr);
                        i = pargs.Length;
                        break;
                    case ")": break;

                    case "[":
                        var array = new Array(line, MObjectFromString(line, AccParamsOnly(pargs)));
                        acc.Add(array);
                        i = pargs.Length;
                        break;
                    case "]": break;

                    // Numberic Operator
                    case "+": 
                    case "-":
                    case "/":
                    case "*":
                    
                    // Boolean Operator
                    case ">":
                    case "<":
                    case "<=":
                    case ">=":
                    case "==":
                    case "!=": acc.Add(new Operator(line, parameter)); break;

                    default: 
                    // check number and string 
                    double numberTest;
                    if(double.TryParse(parameter, out numberTest))
                    {
                        acc.Add(new Number(line, numberTest));
                    }
                    else if(parameter == "true" || parameter == "false")
                    {
                        acc.Add(new Boolean(line, parameter == "true"));    
                    }
                    else if(parameter.Contains("\""))
                    {
                        acc.Add(new String(line, parameter));
                    }
                    else 
                    {
                        acc.Add(new Symbol(line, parameter));
                    }
                    break;
                }
            }
            return acc;
        }
        static MObject Mapping(int line, string word, List<string> parameters){
            
            switch (word)
            {
                // Declare
                case "local": return new Local(line, parameters);
                case "func": return new Func(line, parameters);

                // Value
                case "push": return new Func(line, parameters);
                case "nil?": return new NilQ(line, parameters);

                // Function
                case "ret": return new Ret(line, parameters);                                
                case "call": return new Call(line, parameters);

                // Branching 
                case "jmp": return new Jmp(line, parameters);                                
                case "jmp?": return new JmpTrue(line, parameters);
                case "jmp!": return new JmpFalse(line, parameters);

                case "label": return new Label(line, parameters);

                default: throw new Exception(string.Format("Unknown MObject -- {0}", word));
            }
        }

        static void Main(string[] args)
        {
            string text = File.ReadAllText("fib.txt");

            // split by lines
            foreach (var line in text.Split('\n'))
            {
                sourceLines.Add(line);

                var i = 0;
                var currentKeyWord = "";
                var currentParams = new List<string>();
                foreach (var word in line.Split(DELIMITER))
                {
                    if (word == " " || word == "" || word.Length == 0) continue;
                    i++;

                    if (i == 1)
                    {
                        if (TOKENS.Contains(word))
                        {
                            Console.WriteLine("[Keyword]: {0}", word);
                            currentKeyWord = word;
                        }
                        else
                        {
                            Console.WriteLine("[Label]: {0}", word);
                            currentKeyWord = "label";
                        }
                    }
                    else
                    {
                        currentParams.Add(word);
                        Console.WriteLine("[Params]: {0}", word);
                    }
                }
                memory.AddLast(Mapping(i, currentKeyWord, currentParams));
                Console.WriteLine();
                Console.WriteLine(line);
                Console.WriteLine();
            }


        }
    }
}
