using System.Text;

using TokenType = MyLangCompiler.Compiler.Token.TokenType;

namespace MyLangCompiler
{
    class Compiler
    {
        // ==========================================
        // 0. Compiler Entry Point
        // ==========================================

        public static CompileResult Compile(string sourceCode, int bits, CompilerOutput outputType, string opcodeDefinition, int memAddressLength, int memWordLength, int memSize)  
        {
            //1. Lexing
            var lexer = new Lexer(sourceCode);
            var tokens = lexer.Tokenize();

            //2. Parsing
            var parser = new Parser(tokens, sourceCode);
            var program = parser.Parse();

            //3. Code Generation
            var codeGen = new CodeGenerator(bits, opcodeDefinition);
            return codeGen.Generate(program, outputType, memAddressLength, memWordLength, memSize);
        }

        public static CompileResult Compile(string sourceCode, string opcodeDefinition, int bits, CompilerOutput outputType, int memAddressLength, int memWordLength, int memSize, out ProgramNode programNodeOut)
        {
            //1. Lexing
            var lexer = new Lexer(sourceCode);
            var tokens = lexer.Tokenize();

            //2. Parsing
            var parser = new Parser(tokens, sourceCode);
            ProgramNode program = parser.Parse();
            programNodeOut = program;

            //3. Code Generation
            var codeGen = new CodeGenerator(bits, opcodeDefinition);
            return codeGen.Generate(program, outputType, memAddressLength, memWordLength, memSize);
        }

        public enum CompilerOutput 
        {
            Assembly,
            MachineCode,
            Symbols
        }
        
        public class CompileResult
        {
            public List<string> Output { get; set; } = new List<string>();
            public List<string> Messages { get; set; } = new List<string>();
        }



        // ==========================================
        // 1. TOKENS & LEXER
        // ==========================================
        internal class Token
        {
            public Token(TokenType type, string value, int line, int characterPosition)
            {
                Type = type;
                Value = value;
                Line = line;
                CharacterPosition = characterPosition;
            }


            internal enum TokenType
            {
                Identifier, Number, Equals, Plus, Minus,
                Asterisk, Slash,
                GreaterThan, LessThan, DoubleEquals,
                If, Else, Goto, Print, LParen, RParen, Colon,
                LBrace, RBrace, Semicolon,
                EOF //end of file
            }


            public TokenType Type { get; }
            public string Value { get; }
            public int Line { get; }
            public int CharacterPosition { get; }



            public override string ToString()
            {
                return $"{Type}: {Value}";
            }
        }

        private class Lexer
        {
            public Lexer(string source)
            {
                this.source = source;
            }


            private readonly string source;
            private int pos = 0;
            private int line = 1;


            internal List<Token> Tokenize()
            {
                List<Token> tokens = new List<Token>();
                while (pos < source.Length)
                {
                    //Get the current character.
                    char current = source[pos];


                    //Skip whitespaces.
                    if (char.IsWhiteSpace(current)) 
                    {
                        //If a newline, increment line count.
                        if (current == '\n') 
                            line++;

                        pos++;

                        continue;
                    }


                    //Skip comments.
                    if (current == '/' && Peek() == '/')
                    {
                        //Go to the next character untill we reach end of the line or end of file.
                        while (pos < source.Length && source[pos] != '\n')
                        { 
                            pos++;
                        }

                        if(pos >= source.Length)
                            break;

                        continue;
                    }


                    //Operators and punctuation
                    switch (current)
                    {
                        case '+':
                            tokens.Add(new Token(TokenType.Plus, "+", line, pos));
                            pos++;
                            break;
                        case '-':
                            tokens.Add(new Token(TokenType.Minus, "-", line, pos));
                            pos++;
                            break;
                        case '*':
                            tokens.Add(new Token(TokenType.Asterisk, "*", line, pos));
                            pos++;
                            break;
                        case '/':
                            if (Peek() == '/') //Skip comments.
                            {
                                //Go to the next character until we reach end of the line or end of file.
                                while (pos < source.Length && source[pos] != '\n')
                                { 
                                    pos++;
                                }

                                if(pos >= source.Length)
                                    break;
                            }
                            else
                            {
                                tokens.Add(new Token(TokenType.Slash, "/", line, pos));
                                pos++;
                            }
                            break;
                        case '(':
                            tokens.Add(new Token(TokenType.LParen, "(", line, pos));
                            pos++;
                            break;
                        case ')':
                            tokens.Add(new Token(TokenType.RParen, ")", line, pos));
                            pos++;
                            break;
                        case ':':
                            tokens.Add(new Token(TokenType.Colon, ":", line, pos));
                            pos++;
                            break;
                        case '=':
                            if (Peek() == '=')
                            {
                                tokens.Add(new Token(TokenType.DoubleEquals, "==", line, pos));
                                pos += 2;
                            }
                            else
                            {
                                tokens.Add(new Token(TokenType.Equals, "=", line, pos));
                                pos++;
                            }
                            break;
                        case '>':
                            tokens.Add(new Token(TokenType.GreaterThan, ">", line, pos));
                            pos++;
                            break;
                        case '<':
                            tokens.Add(new Token(TokenType.LessThan, "<", line, pos));
                            pos++;
                            break;
                        case '{':
                            tokens.Add(new Token(TokenType.LBrace, "{", line, pos));
                            pos++;
                            break;
                        case '}':
                            tokens.Add(new Token(TokenType.RBrace, "}", line, pos));
                            pos++;
                            break;
                        case ';':
                            tokens.Add(new Token(TokenType.Semicolon, ";", line, pos));
                            pos++;
                            break;
                        default:
                            if (char.IsDigit(current)) //Numbers
                            {
                                string digitStr = string.Empty;
                                while (pos < source.Length && char.IsDigit(source[pos])) //Charter is a digit and we are not at the end of file.
                                    digitStr += source[pos++];

                                tokens.Add(new Token(TokenType.Number, digitStr, line, pos));
                            }
                            else if (char.IsLetter(current) || current == '_') //Identifiers & Keywords
                            {
                                string keywordStr = string.Empty;
                                while (pos < source.Length && (char.IsLetterOrDigit(source[pos]) || source[pos] == '_'))
                                    keywordStr += source[pos++];

                                TokenType type = keywordStr switch
                                {
                                    "if" => TokenType.If,
                                    "else" => TokenType.Else,
                                    "goto" => TokenType.Goto,
                                    "print" => TokenType.Print,
                                    _ => TokenType.Identifier
                                };

                                tokens.Add(new Token(type, keywordStr, line, pos));
                            }
                            else
                            {
                                throw new Exception($"Lexer Error: Unknown character '{current}' at line {line} at position {pos}");
                            }
                            break;
                    }
                }

                //After all tokens processed, add EOF token.
                tokens.Add(new Token(TokenType.EOF, "", line, pos));

                return tokens;



                #region Tokenize() method helpers. ////////////////////////////////////////////////////

                char Peek()
                {
                    if (pos + 1 < source.Length)
                        return source[pos + 1];
                    else
                        return '\0';
                }

                #endregion ////////////////////////////////////////////////////////////////////////////
            }
        }



        // ==========================================
        // 2. ABSTRACT SYNTAX TREE (AST) 
        // ==========================================
        public abstract class Node
        {
            public string SourceLine { get; set; }
        }

        public class ProgramNode : Node
        {
            public List<Node> Statements { get; } = new List<Node>();
        }

        public class LabelNode : Node
        {
            public LabelNode(string name, string sourceLine = "")
            {
                Name = name;
                SourceLine = sourceLine;
            }
                
            
            public string Name { get; }
        }

        public class GotoNode : Node
        {
            public GotoNode(string targetLabel, string sourceLine = "")
            {
                TargetLabel = targetLabel;
                SourceLine = sourceLine;
            }
            

            public string TargetLabel { get; }
        }

        public class PrintNode : Node
        {
            public PrintNode(string target, bool isImmediate, string sourceLine = "")
            {
                Target = target;
                IsImmediate = isImmediate;
                SourceLine = sourceLine;
            }


            public string Target { get; } // Variable or Number
            public bool IsImmediate { get; }
        }

        public class AssignmentNode : Node
        {
            public AssignmentNode(string target, ExpressionNode expr, string sourceLine = "")
            {
                TargetVariable = target;
                Expression = expr;
                SourceLine = sourceLine;
            }
            public string TargetVariable { get; }
            public ExpressionNode Expression { get; }
        }

        public class IfBlockNode : Node
        {
            public IfBlockNode(string left, string op, string right, List<Node> statements, List<Node> elseStatements = null, string sourceLine = "")
            {
                Left = left;
                Operator = op;
                Right = right;
                Statements = statements;
                ElseStatements = elseStatements;
                SourceLine = sourceLine;
            }

            public string Left { get; }
            public string Operator { get; }
            public string Right { get; }
            public List<Node> Statements { get; }
            public List<Node> ElseStatements { get; }
        }

        public abstract class ExpressionNode : Node { }

        public class LiteralNode : ExpressionNode
        {
            public LiteralNode(string value) { Value = value; }
            public string Value { get; }
        }

        public class VariableNode : ExpressionNode
        {
            public VariableNode(string name) { Name = name; }
            public string Name { get; }
        }

        public class BinaryOpNode : ExpressionNode
        {
            public BinaryOpNode(string op, ExpressionNode left, ExpressionNode right)
            {
                Operator = op;
                Left = left;
                Right = right;
            }
            public string Operator { get; }
            public ExpressionNode Left { get; }
            public ExpressionNode Right { get; }
        }



        // ==========================================
        // 3. PARSER
        // ==========================================
        class Parser
        {
            public Parser(List<Token> tokens, string sourceCode)
            {
                if (tokens is null || tokens.Count == 0)
                    throw new ArgumentException("Tokens cannot be null or empty.", nameof(tokens));

                this.tokens = tokens;
                this.sourceCode = sourceCode;

                //Initialize position to 0 and use it to set first token as current.
                pos = 0;
                current = tokens[pos];
            }


            private readonly List<Token> tokens;
            private readonly string sourceCode;
            private int pos;
            private Token current;


            public ProgramNode Parse()
            {
                var program = new ProgramNode();

                while (current.Type != TokenType.EOF)
                {
                    program.Statements.Add(parseStatement());
                }

                return program;
            }

            private Node parseStatement()
            {
                //Get the source line number at the start of each statement.
                int statementLineNumber = current.Line;
                string statementSourceLine = getSourceLine(statementLineNumber);

                //Label: "label:"
                if (current.Type == TokenType.Identifier && peek(1).Type == TokenType.Colon)
                {
                    var labelName = match(TokenType.Identifier).Value;
                    match(TokenType.Colon);

                    return new LabelNode(labelName, statementSourceLine);
                }

                //Print: "print(x)"
                if (current.Type == TokenType.Print)
                {
                    match(TokenType.Print);
                    match(TokenType.LParen);
                    Token val = matchAny(TokenType.Identifier, TokenType.Number);
                    match(TokenType.RParen);
                    match(TokenType.Semicolon);

                    return new PrintNode(val.Value, val.Type == TokenType.Number, statementSourceLine);
                }

                //Goto: "goto label"
                if (current.Type == TokenType.Goto)
                {
                    match(TokenType.Goto);
                    var label = match(TokenType.Identifier).Value;
                    match(TokenType.Semicolon);

                    return new GotoNode(label, statementSourceLine);
                }

                //If: "if x > y goto label"
                if (current.Type == TokenType.If)
                {
                    match(TokenType.If);
                    string left = match(TokenType.Identifier).Value;
                    Token opToken = matchAny(TokenType.DoubleEquals, TokenType.GreaterThan, TokenType.LessThan);
                    string right = matchAny(TokenType.Identifier, TokenType.Number).Value;

                    match(TokenType.LBrace);
                    var blockStatements = new List<Node>();
                    while (current.Type != TokenType.RBrace && current.Type != TokenType.EOF)
                    {
                        blockStatements.Add(parseStatement());
                    }
                    match(TokenType.RBrace);

                    List<Node> elseStatements = null;
                    if (current.Type == TokenType.Else)
                    {
                        match(TokenType.Else);
                        match(TokenType.LBrace);
                        elseStatements = new List<Node>();
                        while (current.Type != TokenType.RBrace && current.Type != TokenType.EOF)
                        {
                            elseStatements.Add(parseStatement());
                        }
                        match(TokenType.RBrace);
                    }

                    return new IfBlockNode(left, opToken.Value, right, blockStatements, elseStatements, statementSourceLine);
                }

                //Assignment: x = expr
                if (current.Type == TokenType.Identifier)
                {
                    string target = match(TokenType.Identifier).Value;
                    match(TokenType.Equals);

                    ExpressionNode expr = parseExpression();
                    match(TokenType.Semicolon);

                    return new AssignmentNode(target, expr, statementSourceLine);
                }

                throw new Exception($"Parser Error: Unexpected token {current} at line {current.Line}");
            }

            //Expression parsing with precedence: +,- (lowest), *,/ (higher)
            private ExpressionNode parseExpression(int precedence = 0)
            {
                //Parse the leftmost primary expression (either a variable or a literal)
                ExpressionNode left;
                if (current.Type == TokenType.Number)
                    left = new LiteralNode(match(TokenType.Number).Value);
                else if (current.Type == TokenType.Identifier)
                    left = new VariableNode(match(TokenType.Identifier).Value);
                else
                    throw new Exception($"Expected primary expression at line {current.Line}");

                //If the next token is not an operator, return the primary as the whole expression
                while (true)
                {
                    //Get the precedence of the current operator.
                    int currPrecedence = current.Type switch
                    {
                        TokenType.Asterisk => 2,
                        TokenType.Slash => 2,
                        TokenType.Plus => 1,
                        TokenType.Minus => 1,
                        _ => 0
                    };

                    //If the next token is not an operator or has lower precedence, stop parsing further
                    if (currPrecedence == 0 || currPrecedence < precedence)
                        break;

                    //Otherwise, parse the operator and the right-hand side
                    string op = current.Value;
                    matchAny(TokenType.Plus, TokenType.Minus, TokenType.Asterisk, TokenType.Slash);
                    ExpressionNode right = parseExpression(currPrecedence + 1);
                    left = new BinaryOpNode(op, left, right);
                }

                return left;
            }

            #region Helper methods /////////////////////////////////////////////////////////////////////

            private Token peek(int offset)
            {
                if (pos + offset < tokens.Count)
                    return tokens[pos + offset];
                else
                    return tokens[tokens.Count - 1];
            }

            private void nextToken()
            {
                //Advance position.
                pos++;
                //Update current token.
                current = tokens[pos];
            }

            private Token match(TokenType type)
            {
                if (current.Type == type)
                {
                    //Store the current token to return later.
                    Token currentReturn = current;
                    //Advance to next token.
                    nextToken();
                    //Return the matched token.
                    return currentReturn;
                }

                throw new Exception($"Expected {type} but found {current.Type} at line {current.Line}");
            }

            private Token matchAny(params TokenType[] types)
            {
                //Check if current token matches any of the provided types.
                foreach (var type in types)
                {
                    if (current.Type == type)
                    {
                        //Store the current token to return later.
                        Token currentReturn = current;
                        //Advance to next token.
                        nextToken();
                        //Return the matched token.
                        return currentReturn;
                    }
                }

                throw new Exception($"Unexpected token {current.Type} at line {current.Line}");
            }

            //Extracts the text of a given source line (1-based line number).
            private string getSourceLine(int lineNumber)
            {
                var lines = sourceCode.Split('\n');
                if (lineNumber >= 1 && lineNumber <= lines.Length)
                    return lines[lineNumber - 1].Trim();

                return "";
            }

            #endregion //////////////////////////////////////////////////////////////////////////////////
        }



        // ==========================================
        // 4. CODE GENERATOR
        // ==========================================
        public class CodeGenerator
        {
            #region Properties and Variables

            private List<string> _instructions = new List<string>();
            private Dictionary<string, int> _variables = new Dictionary<string, int>();
            private Dictionary<string, int> _variableInitialValues = new Dictionary<string, int>(); // Variable Name -> Initial Value
            private Dictionary<string, int> _labels = new Dictionary<string, int>(); // Label Name -> Instruction Index
            private Dictionary<int, string> _jumpFixups = new Dictionary<int, string>(); // Instruction Index -> Target Label

            private int _instructionPointer = 0;
            private int _immediateLimit;
            private int _memWordLength;

            private Dictionary<string, string> asmToOpcodeMap;
            public List<string> Messages { get; } = new List<string>();

            #endregion



            #region Constructor

            public CodeGenerator(int bitDepth, string opcodeDefinition = "")
            {
                _immediateLimit = (1 << bitDepth) - 1;

                if (string.IsNullOrEmpty(opcodeDefinition))
                {
                    asmToOpcodeMap = new Dictionary<string, string>()
                    {
                        { "NO OP", "0000" },
                        { "LDA",   "1000" },
                        { "ADD",   "0100" },
                        { "OUT",   "1100" },
                        { "HLT",   "0010" },
                        { "SUB",   "1010" },
                        { "LDAD",  "0110" },
                        { "STA",   "1110" },
                        { "JMP",   "0001" },
                        { "JMC",   "1001" },
                        { "JMZ",   "0101" }
                        // Unused: 1101, 0011, 1011, 0111, 1111
                    };
                }
                else 
                {
                    asmToOpcodeMap = mapAssemblyToOpcode(opcodeDefinition);
                }
            }

            #endregion



            #region Methods

            public CompileResult Generate(ProgramNode program, CompilerOutput outputType, int memAddressLength, int memWordLength, int memSize)
            {
                Messages.Clear();
                _memWordLength = memWordLength;

                if (asmToOpcodeMap.Count == 0)
                    return new CompileResult { Output = new List<string>(), Messages = new List<string> { "No opcode definitions available." } };

                var sourceLines = new List<string>();
                var variableNames = new List<string>();

                //1. Walk the AST
                foreach (var node in program.Statements)
                {
                    string sourceLine = GetSourceLineForNode(node); //Implement this to extract the source line.
                    string variable = GetVariableForNode(node);     //Implement this to extract the variable name if present.

                    VisitWithSymbols(node, sourceLine, variable, sourceLines, variableNames);
                }

                //2. Ensure Halt
                EmitWithSymbols("HLT", "", "", sourceLines, variableNames);

                //3. Resolve Addresses and Format Output
                var finalOutput = new List<string>();
                int varBaseAddr = _instructionPointer;

                //First pass: resolve all instructions to assembly text
                var resolvedLines = new List<string>();
                for (int i = 0; i < _instructions.Count; i++)
                {
                    string line = _instructions[i];

                    //Fix Jump Targets (Labels).
                    if (_jumpFixups.ContainsKey(i))
                    {
                        string labelName = _jumpFixups[i];
                        if (!_labels.ContainsKey(labelName)) throw new Exception($"Undefined label: {labelName}");
                        line = line.Replace("{JMP}", _labels[labelName].ToString());
                    }

                    //Fix Variable Addresses.
                    foreach (var kvp in _variables)
                    {
                        line = line.Replace($"{{VAR:{kvp.Key}}}", (varBaseAddr + kvp.Value).ToString());
                    }

                    resolvedLines.Add(line);
                }

                if (outputType == CompilerOutput.MachineCode)
                {
                    //Build a single assembly text block (instructions + data section), then convert once.
                    var asmBlock = new StringBuilder();
                    foreach (var line in resolvedLines)
                        asmBlock.AppendLine(line);

                    //Data Section - append initial values for variables.
                    //Build a reverse lookup: variable name -> source line text for pre-initialized variables.
                    var varSourceLines = new Dictionary<string, string>();
                    foreach (var node in program.Statements)
                    {
                        if (node is AssignmentNode a && a.Expression is LiteralNode && !string.IsNullOrEmpty(a.SourceLine))
                            varSourceLines[a.TargetVariable] = a.SourceLine;
                    }

                    foreach (var kvp in _variables)
                    {
                        string varName = kvp.Key;
                        int initialValue = _variableInitialValues.ContainsKey(varName) ? _variableInitialValues[varName] : 0;
                        asmBlock.AppendLine(initialValue.ToString());
                    }

                    string machineCodeText = assemblyToMachineCode(asmBlock.ToString(), asmToOpcodeMap, memAddressLength, memWordLength, memSize);
                    finalOutput.AddRange(machineCodeText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
                }
                else
                {
                    for (int i = 0; i < resolvedLines.Count; i++)
                    {
                        if (outputType == CompilerOutput.Symbols)
                        {
                            string src = sourceLines.Count > i ? sourceLines[i] : "";
                            string var = variableNames.Count > i ? variableNames[i] : "";
                            if (!string.IsNullOrEmpty(var))
                                finalOutput.Add($"{resolvedLines[i]}    :{src}:[{var}]");
                            else
                                finalOutput.Add($"{resolvedLines[i]}    :{src}");
                        }
                        else
                        {
                            finalOutput.Add(resolvedLines[i]);
                        }
                    }

                    //4. Data Section - output initial values for variables
                    var varSourceLines = new Dictionary<string, string>();
                    foreach (var node in program.Statements)
                    {
                        if (node is AssignmentNode a && a.Expression is LiteralNode && !string.IsNullOrEmpty(a.SourceLine))
                            varSourceLines[a.TargetVariable] = a.SourceLine;
                    }

                    foreach (var kvp in _variables)
                    {
                        string varName = kvp.Key;
                        int initialValue = _variableInitialValues.ContainsKey(varName) ? _variableInitialValues[varName] : 0;
                        if (outputType == CompilerOutput.Symbols)
                        {
                            string srcLine = varSourceLines.ContainsKey(varName) ? varSourceLines[varName] : "";
                            if (!string.IsNullOrEmpty(srcLine))
                                finalOutput.Add($"{initialValue}    :{srcLine}:[{varName}]");
                            else
                                finalOutput.Add($"{initialValue}    ::[{varName}]");
                        }
                        else
                        {
                            finalOutput.Add(initialValue.ToString());
                        }
                    }
                }

                Messages.Add($"Compiled {_instructionPointer} instructions.");
                Messages.Add($"Allocated {_variables.Count} variables.");

                return new CompileResult { Output = finalOutput, Messages = Messages };
            }

            private void LoadIntoA(string value)
            {
                if (IsNumber(value))
                {
                    int val = int.Parse(value);
                    if (val > _immediateLimit) 
                        throw new Exception($"Literal {val} exceeds hardware limit {_immediateLimit}");

                    Emit($"LDAD {val}");
                }
                else
                {
                    RegisterVar(value);

                    Emit($"LDA {{VAR:{value}}}");
                }
            }

            private void LoadIntoAWithSymbols(string value, string sourceLine, string variable, List<string> sourceLines, List<string> variableNames)
            {
                if (IsNumber(value))
                {
                    int val = int.Parse(value);
                    if (val > _immediateLimit)
                        throw new Exception($"Literal {val} exceeds hardware limit {_immediateLimit}");

                    EmitWithSymbols($"LDAD {val}", sourceLine, variable, sourceLines, variableNames);
                }
                else
                {
                    RegisterVar(value);

                    EmitWithSymbols($"LDA {{VAR:{value}}}", sourceLine, variable, sourceLines, variableNames);
                }
            }

            private void Emit(string asm)
            {
                _instructions.Add(asm);

                if (!asm.StartsWith("//")) 
                    _instructionPointer++;
            }

            private void RegisterVar(string name)
            {
                if (!_variables.ContainsKey(name)) 
                    _variables[name] = _variables.Count;
            }

            private bool IsNumber(string s) => int.TryParse(s, out _);

            private void VisitWithSymbols(Node node, string sourceLine, string variable, List<string> sourceLines, List<string> variableNames)
            {
                switch (node)
                {
                    case LabelNode l:
                        _labels[l.Name] = _instructionPointer;
                        break;

                    case PrintNode p:
                        LoadIntoAWithSymbols(p.Target, sourceLine, p.IsImmediate ? "" : p.Target, sourceLines, variableNames);
                        EmitWithSymbols("OUT", sourceLine, p.Target, sourceLines, variableNames);
                        break;

                    case GotoNode g:
                        EmitJumpWithSymbols("JMP", g.TargetLabel, sourceLine, "", sourceLines, variableNames);
                        break;

                    case AssignmentNode a:
                        //Optimization: if assigning a simple literal to a brand-new variable, pre-initialize it in the data section instead of emitting LDAD + STA.
                        if (a.Expression is LiteralNode assignLit && !_variables.ContainsKey(a.TargetVariable))
                        {
                            int val = int.Parse(assignLit.Value);
                            if (val > (1 << _memWordLength) - 1)
                                throw new Exception($"Literal {val} exceeds memory word size limit {(1 << _memWordLength) - 1}");

                            RegisterVar(a.TargetVariable);
                            _variableInitialValues[a.TargetVariable] = val;
                        }
                        else
                        {
                            GenerateExpressionWithSymbols(a.Expression, sourceLine, a.TargetVariable, sourceLines, variableNames);
                            RegisterVar(a.TargetVariable);
                            EmitWithSymbols($"STA {{VAR:{a.TargetVariable}}}", sourceLine, a.TargetVariable, sourceLines, variableNames);
                        }
                        break;

                    case IfBlockNode ib:
                        LoadIntoAWithSymbols(ib.Left, sourceLine, ib.Left, sourceLines, variableNames);
                        EmitWithSymbols($"SUB {{VAR:{ib.Right}}}", sourceLine, ib.Right, sourceLines, variableNames);

                        //Generate unique label for end of block.
                        string elseLabel = $"__else_{_instructionPointer}";
                        string endLabel = $"__end_if_{_instructionPointer}";

                        if (ib.Operator == "==")
                            EmitJumpWithSymbols("JMZ", elseLabel, sourceLine, "", sourceLines, variableNames, invert: true);
                        else if (ib.Operator == ">")
                            EmitJumpWithSymbols("JMC", elseLabel, sourceLine, "", sourceLines, variableNames, invert: true);
                        else if (ib.Operator == "<")
                            EmitWithSymbols("// WARNING: '<' not natively supported by JMC/JMZ, check ISA", sourceLine, "", sourceLines, variableNames);

                        //Emit "then" block.
                        foreach (var stmt in ib.Statements)
                        {
                            string blockSrc = GetSourceLineForNode(stmt);
                            string blockVar = GetVariableForNode(stmt);
                            VisitWithSymbols(stmt, blockSrc, blockVar, sourceLines, variableNames);
                        }

                        //Jump over else block after "then".
                        EmitJumpWithSymbols("JMP", endLabel, sourceLine, "", sourceLines, variableNames);

                        //Place else label.
                        _labels[elseLabel] = _instructionPointer;

                        //Emit "else" block if present.
                        if (ib.ElseStatements != null)
                        {
                            foreach (var stmt in ib.ElseStatements)
                            {
                                string blockSrc = GetSourceLineForNode(stmt);
                                string blockVar = GetVariableForNode(stmt);
                                VisitWithSymbols(stmt, blockSrc, blockVar, sourceLines, variableNames);
                            }
                        }

                        //Place end label.
                        _labels[endLabel] = _instructionPointer;
                        break;
                }
            }

            private void EmitJumpWithSymbols(string op, string label, string sourceLine, string variable, List<string> sourceLines, List<string> variableNames, bool invert = false)
            {
                if (invert)
                {
                    //JMZ: jump if zero, so invert means jump if NOT zero (i.e., if condition is false).
                    //JMC: jump if carry, so invert means jump if NOT carry.
                    //For simplicity, use unconditional JMP if invert is true after the block.
                    _instructions.Add($"{op} {{JMP}}");
                    sourceLines.Add(sourceLine);
                    variableNames.Add(variable);
                    _jumpFixups[_instructionPointer] = label;
                    _instructionPointer++;
                }
                else
                {
                    _instructions.Add($"{op} {{JMP}}");
                    sourceLines.Add(sourceLine);
                    variableNames.Add(variable);
                    _jumpFixups[_instructionPointer] = label;
                    _instructionPointer++;
                }
            }

            private void EmitWithSymbols(string asm, string sourceLine, string variable, List<string> sourceLines, List<string> variableNames)
            {
                _instructions.Add(asm);
                sourceLines.Add(sourceLine);
                variableNames.Add(variable);

                if (!asm.StartsWith("//")) 
                    _instructionPointer++;
            }

            private string GetSourceLineForNode(Node node)
            {
                return node?.SourceLine ?? "";
            }

            private string GetVariableForNode(Node node)
            {
                switch (node)
                {
                    case AssignmentNode a:
                        return a.TargetVariable;
                    case PrintNode p:
                        return !p.IsImmediate ? p.Target : "";
                    case IfBlockNode ib:
                        return ib.Left;
                    default:
                        return "";
                }
            }

            private void GenerateExpression(ExpressionNode expr)
            {
                switch (expr)
                {
                    case LiteralNode lit:
                        LoadIntoA(lit.Value);
                        break;
                    case VariableNode var:
                        LoadIntoA(var.Name);
                        break;
                    case BinaryOpNode bin:
                        //Evaluate left side.
                        GenerateExpression(bin.Left);

                        //.Save result to temp variable.
                        string tempVar = "__tmp_" + _instructionPointer;
                        RegisterVar(tempVar);
                        Emit($"STA {{VAR:{tempVar}}}");

                        //Evaluate right side.
                        GenerateExpression(bin.Right);

                        //Perform operation with left (in temp) and right (in A).
                        string asmOp;
                        if (bin.Operator == "+")
                        {
                            asmOp = "ADD";
                        }
                        else if (bin.Operator == "-")
                        {
                            asmOp = "SUB";
                        }
                        else if (bin.Operator == "*")
                        {
                            asmOp = "// WARNING: '*' (multiplication) not supported in target assembly";
                        }
                        else if (bin.Operator == "/")
                        {
                            asmOp = "// WARNING: '/' (division) not supported in target assembly";
                        }
                        else
                        {
                            throw new Exception($"Unknown operator {bin.Operator}");
                        }

                        Emit($"{asmOp}{(asmOp.StartsWith("//") ? "" : $" {{VAR:{tempVar}}}")}");

                        break;
                    default:
                        throw new Exception("Unsupported expression type in assignment.");
                }
            }

            private void GenerateExpressionWithSymbols(ExpressionNode expr, string sourceLine, string variable, List<string> sourceLines, List<string> variableNames)
            {
                switch (expr)
                {
                    case LiteralNode lit:
                        LoadIntoAWithSymbols(lit.Value, sourceLine, variable, sourceLines, variableNames);
                        break;
                    case VariableNode var:
                        LoadIntoAWithSymbols(var.Name, sourceLine, var.Name, sourceLines, variableNames);
                        break;
                    case BinaryOpNode bin:
                        //Evaluate left side.
                        GenerateExpressionWithSymbols(bin.Left, sourceLine, variable, sourceLines, variableNames);
                        //Save result to temp variable.
                        string tempVar = "__tmp_" + _instructionPointer;
                        RegisterVar(tempVar);
                        EmitWithSymbols($"STA {{VAR:{tempVar}}}", sourceLine, tempVar, sourceLines, variableNames);

                        //Evaluate right side.
                        GenerateExpressionWithSymbols(bin.Right, sourceLine, variable, sourceLines, variableNames);

                        //Perform operation with left (in temp) and right (in A).
                        string asmOp;
                        if (bin.Operator == "+")
                        {
                            asmOp = "ADD";
                            EmitWithSymbols($"{asmOp}{(asmOp.StartsWith("//") ? "" : $" {{VAR:{tempVar}}}")}", sourceLine, tempVar, sourceLines, variableNames);
                        }
                        else if (bin.Operator == "-")
                        {
                            asmOp = "SUB";
                            EmitWithSymbols($"{asmOp}{(asmOp.StartsWith("//") ? "" : $" {{VAR:{tempVar}}}")}", sourceLine, tempVar, sourceLines, variableNames);
                        }
                        else if (bin.Operator == "*")
                        {
                            //Multiplication: result = left * right
                            //Pseudocode:
                            //result = 0
                            //counter = right
                            //while (counter > 0) { result += left; counter--; }

                            string leftVar = "__mul_left_" + _instructionPointer;
                            string rightVar = "__mul_right_" + _instructionPointer;
                            string resultVar = "__mul_result_" + _instructionPointer;
                            string counterVar = "__mul_counter_" + _instructionPointer;
                            string loopLabel = "__mul_loop_" + _instructionPointer;
                            string mulEndLabel = "__mul_end_" + _instructionPointer;

                            //Save left operand.
                            GenerateExpressionWithSymbols(bin.Left, sourceLine, variable, sourceLines, variableNames);
                            RegisterVar(leftVar);
                            EmitWithSymbols($"STA {{VAR:{leftVar}}}", sourceLine, leftVar, sourceLines, variableNames);

                            //Save right operand (counter).
                            GenerateExpressionWithSymbols(bin.Right, sourceLine, variable, sourceLines, variableNames);
                            RegisterVar(rightVar);
                            EmitWithSymbols($"STA {{VAR:{rightVar}}}", sourceLine, rightVar, sourceLines, variableNames);

                            //result = 0
                            RegisterVar(resultVar);
                            _variableInitialValues[resultVar] = 0;

                            //counter = right
                            RegisterVar(counterVar);

                            EmitWithSymbols($"LDA {{VAR:{rightVar}}}", sourceLine, rightVar, sourceLines, variableNames);
                            EmitWithSymbols($"STA {{VAR:{counterVar}}}", sourceLine, counterVar, sourceLines, variableNames);

                            //loop:
                            _labels[loopLabel] = _instructionPointer;

                            //if counter == 0, jump to end
                            EmitWithSymbols($"LDA {{VAR:{counterVar}}}", sourceLine, counterVar, sourceLines, variableNames);
                            EmitJumpWithSymbols("JMZ", mulEndLabel, sourceLine, "", sourceLines, variableNames);

                            //result += left
                            EmitWithSymbols($"LDA {{VAR:{resultVar}}}", sourceLine, resultVar, sourceLines, variableNames);
                            EmitWithSymbols($"ADD {{VAR:{leftVar}}}", sourceLine, leftVar, sourceLines, variableNames);
                            EmitWithSymbols($"STA {{VAR:{resultVar}}}", sourceLine, resultVar, sourceLines, variableNames);

                            //counter--
                            EmitWithSymbols($"LDA {{VAR:{counterVar}}}", sourceLine, counterVar, sourceLines, variableNames);
                            EmitWithSymbols("LDAD 1", sourceLine, "", sourceLines, variableNames);
                            EmitWithSymbols($"SUB {{VAR:{counterVar}}}", sourceLine, counterVar, sourceLines, variableNames);
                            EmitWithSymbols($"STA {{VAR:{counterVar}}}", sourceLine, counterVar, sourceLines, variableNames);

                            //Jump back to loop.
                            EmitJumpWithSymbols("JMP", loopLabel, sourceLine, "", sourceLines, variableNames);

                            //end:
                            _labels[mulEndLabel] = _instructionPointer;

                            // Load result into A
                            EmitWithSymbols($"LDA {{VAR:{resultVar}}}", sourceLine, resultVar, sourceLines, variableNames);

                            break;
                        }
                        else if (bin.Operator == "/")
                        {
                            //Division: result = left / right
                            //Pseudocode:
                            //result = 0
                            //dividend = left
                            //divisor = right
                            //while (dividend >= divisor) { dividend -= divisor; result++; }

                            string leftVar = "__div_left_" + _instructionPointer;
                            string rightVar = "__div_right_" + _instructionPointer;
                            string resultVar = "__div_result_" + _instructionPointer;
                            string loopLabel = "__div_loop_" + _instructionPointer;
                            string endLabel = "__div_end_" + _instructionPointer;

                            //Save left operand (dividend).
                            GenerateExpressionWithSymbols(bin.Left, sourceLine, variable, sourceLines, variableNames);
                            RegisterVar(leftVar);
                            EmitWithSymbols($"STA {{VAR:{leftVar}}}", sourceLine, leftVar, sourceLines, variableNames);

                            //Save right operand (divisor).
                            GenerateExpressionWithSymbols(bin.Right, sourceLine, variable, sourceLines, variableNames);
                            RegisterVar(rightVar);
                            EmitWithSymbols($"STA {{VAR:{rightVar}}}", sourceLine, rightVar, sourceLines, variableNames);

                            //result = 0
                            RegisterVar(resultVar);
                            EmitWithSymbols($"LDAD 0", sourceLine, resultVar, sourceLines, variableNames);
                            EmitWithSymbols($"STA {{VAR:{resultVar}}}", sourceLine, resultVar, sourceLines, variableNames);

                            //loopLabel:
                            _labels[loopLabel] = _instructionPointer;

                            //if dividend < divisor goto endLabel
                            EmitWithSymbols($"LDA {{VAR:{leftVar}}}", sourceLine, leftVar, sourceLines, variableNames);
                            EmitWithSymbols($"SUB {{VAR:{rightVar}}}", sourceLine, rightVar, sourceLines, variableNames);
                            EmitJumpWithSymbols("JMC", endLabel, sourceLine, "", sourceLines, variableNames);

                            //dividend -= divisor
                            EmitWithSymbols($"LDA {{VAR:{leftVar}}}", sourceLine, leftVar, sourceLines, variableNames);
                            EmitWithSymbols($"SUB {{VAR:{rightVar}}}", sourceLine, rightVar, sourceLines, variableNames);
                            EmitWithSymbols($"STA {{VAR:{leftVar}}}", sourceLine, leftVar, sourceLines, variableNames);

                            //result++
                            EmitWithSymbols($"LDA {{VAR:{resultVar}}}", sourceLine, resultVar, sourceLines, variableNames);
                            EmitWithSymbols($"LDAD 1", sourceLine, "", sourceLines, variableNames);
                            EmitWithSymbols($"ADD {{VAR:{resultVar}}}", sourceLine, resultVar, sourceLines, variableNames);
                            EmitWithSymbols($"STA {{VAR:{resultVar}}}", sourceLine, resultVar, sourceLines, variableNames);

                            //goto loopLabel
                            EmitJumpWithSymbols("JMP", loopLabel, sourceLine, "", sourceLines, variableNames);

                            //endLabel:
                            _labels[endLabel] = _instructionPointer;

                            //Load result into A.
                            EmitWithSymbols($"LDA {{VAR:{resultVar}}}", sourceLine, resultVar, sourceLines, variableNames);

                            break;
                        }
                        else
                        {
                            throw new Exception($"Unknown operator {bin.Operator}");
                        }
                        break;
                    default:
                        throw new Exception("Unsupported expression type in assignment.");
                }
            }



            // Map assembly to machine code using opcode definitions. //////////////////////////////////////////////////////////////////////////////////////////////

            public static string AssemblyToMachineCode(string asmText, string opcodeDefinition, int memAddressLength, int memWordLength, int memSize)
            {
                var asmToOpcodeMap = mapAssemblyToOpcode(opcodeDefinition);

                return assemblyToMachineCode(asmText, asmToOpcodeMap, memAddressLength, memWordLength, memSize);
            }

            //Ported from AsmIDE.
            private static string assemblyToMachineCode(string asmText, Dictionary<string, string> opcodeMap, int memAddressLength, int memWordLength, int memSize)
            {
                int operandBits = memWordLength - memAddressLength;
                var asmLines = asmText.Split('\n');
                var machineCodeLines = new List<string>();

                foreach (var rawLine in asmLines)
                {
                    var line = rawLine.Split(new[] { "//" }, StringSplitOptions.None)[0].Trim();

                    if (string.IsNullOrEmpty(line))
                    {
                        string filler = new string('0', memWordLength);
                        machineCodeLines.Add(filler.Insert(memAddressLength, " "));
                        continue;
                    }

                    //Raw numeric data (e.g., variable storage locations).
                    if (int.TryParse(line, out int rawValue))
                    {
                        string numericValue = Convert.ToString(rawValue, 2);
                        char[] reversed = numericValue.ToCharArray();
                        Array.Reverse(reversed);
                        string padded = new string(reversed).PadRight(memWordLength, '0');
                        machineCodeLines.Add(padded.Insert(memAddressLength, " "));
                        continue;
                    }

                    //Parse mnemonic and operand.
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string mnemonic = parts[0].ToUpperInvariant();

                    //Handle two-word instructions like "NO OP".
                    if (parts.Length >= 2 && opcodeMap.ContainsKey(mnemonic + " " + parts[1].ToUpperInvariant()))
                    {
                        mnemonic = mnemonic + " " + parts[1].ToUpperInvariant();
                        parts = parts.Length > 2 ? new[] { mnemonic, parts[2] } : new[] { mnemonic };
                    }

                    if (opcodeMap.TryGetValue(mnemonic, out string? opcode))
                    {
                        string valueString = new string('0', operandBits);

                        if (parts.Length > 1 && int.TryParse(parts[^1], out int value))
                        {
                            // Convert to binary, reverse for LSB-first (matching AsmIDE behavior)
                            string numericValue = Convert.ToString(value, 2);
                            char[] reversed = numericValue.ToCharArray();
                            Array.Reverse(reversed);
                            valueString = new string(reversed).PadRight(operandBits, '0');
                        }

                        machineCodeLines.Add($"{opcode} {valueString}");
                    }
                    else
                    {
                        //Unknown instruction -> zeros.
                        string filler = new string('0', memWordLength);
                        machineCodeLines.Add(filler.Insert(memAddressLength, " "));
                    }
                }

                //Pad remaining memory locations with zeros up to memSize.
                while (machineCodeLines.Count < memSize)
                {
                    string filler = new string('0', memWordLength);
                    machineCodeLines.Add(filler.Insert(memAddressLength, " "));
                }

                //Add memory address column.
                for (int i = 0; i < machineCodeLines.Count; i++)
                {
                    string memAddress = Convert.ToString(i, 2);
                    char[] revAddr = memAddress.ToCharArray();
                    Array.Reverse(revAddr);
                    string addr = new string(revAddr).PadRight(memAddressLength, '0');
                    machineCodeLines[i] = machineCodeLines[i] + "   " + addr;
                }

                return string.Join(Environment.NewLine, machineCodeLines);
            }

            private static Dictionary<string, string> mapAssemblyToOpcode(string opcodeDefinition)
            {
                Dictionary<string, string> asmToOpcodeMap = new Dictionary<string, string>();

                //Parse opcode definitions (format: INSTRUCTION=BINARY per line, e.g. LDA=0001)
                foreach (var defLine in opcodeDefinition.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = defLine.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                        continue;

                    var parts = trimmed.Split('=', 2);
                    if (parts.Length == 2)
                        asmToOpcodeMap[parts[0].Trim().ToUpper()] = parts[1].Trim();
                }

                return asmToOpcodeMap;
            }

            #endregion
        }
    }
}
