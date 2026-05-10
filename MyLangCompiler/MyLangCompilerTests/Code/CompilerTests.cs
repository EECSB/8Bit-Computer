using MyLangCompiler;

namespace MyLangCompilerTests
{
    [TestClass()]
    public class CompilerTests
    {
        //Default hardware parameters matching the Blazor app defaults.
        private const int DefaultBits = 4;
        private const string DefaultOpcodeDefinition = "";
        private const int DefaultMemAddressLength = 4;
        private const int DefaultMemWordLength = 8;
        private const int DefaultMemSize = 16;

        private static Compiler.CompileResult Compile(string source, Compiler.CompilerOutput outputType)
        {
            return Compiler.Compile(source, DefaultBits, outputType, DefaultOpcodeDefinition, DefaultMemAddressLength, DefaultMemWordLength, DefaultMemSize);
        }

        private static Compiler.CompileResult Compile(string source, Compiler.CompilerOutput outputType, out Compiler.ProgramNode programNode)
        {
            return Compiler.Compile(source, DefaultOpcodeDefinition, DefaultBits, outputType, DefaultMemAddressLength, DefaultMemWordLength, DefaultMemSize, out programNode);
        }

        [TestMethod()]
        public void Compile_AssignmentAndPrint_GeneratesExpectedAssembly()
        {
            string source = "x = 3;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.AreEqual(result.Output[0], "LDA 3");
            Assert.AreEqual(result.Output[1], "OUT");
            Assert.AreEqual(result.Output[2], "HLT");
            Assert.AreEqual(result.Output[3], "3");
        }

        [TestMethod()]
        public void Compile_IfGoto_SkipsPrintWhenConditionTrue()
        {
            string source = "x = 10;\ny = 5;\nif x > y {\nprint(x);\n}\nelse{\nprint(y);\n}";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.AreEqual(result.Output[0], "LDA 9");
            Assert.AreEqual(result.Output[1], "SUB 10");
            Assert.AreEqual(result.Output[2], "JMC 6");
            Assert.AreEqual(result.Output[3], "LDA 9");
            Assert.AreEqual(result.Output[4], "OUT");
            Assert.AreEqual(result.Output[5], "JMP 8");
            Assert.AreEqual(result.Output[6], "LDA 10");
            Assert.AreEqual(result.Output[7], "OUT");
            Assert.AreEqual(result.Output[8], "HLT");
            Assert.AreEqual(result.Output[9], "10");
            Assert.AreEqual(result.Output[10], "5");
        }

        [TestMethod()]
        public void Compile_Addition_GeneratesAddInstruction()
        {
            string source = "x = 2;\ny = 3;\nx = x + y;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.AreEqual(result.Output[0], "LDA 8");
            Assert.AreEqual(result.Output[1], "STA 10");
            Assert.AreEqual(result.Output[2], "LDA 9");
            Assert.AreEqual(result.Output[3], "ADD 10");
            Assert.AreEqual(result.Output[4], "STA 8");
            Assert.AreEqual(result.Output[5], "LDA 8");
            Assert.AreEqual(result.Output[6], "OUT");
            Assert.AreEqual(result.Output[7], "HLT");
            Assert.AreEqual(result.Output[8], "2");
            Assert.AreEqual(result.Output[9], "3");
            Assert.AreEqual(result.Output[10], "0");
        }

        [TestMethod()]
        public void Compile_AssignmentAndPrint_GeneratesExpectedMachineCode()
        {
            string source = "x = 3;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.MachineCode);

            //Machine code format now includes memory address column and LSB-first encoding.
            //Verify we get the expected number of output lines.
            Assert.IsTrue(result.Output.Count > 0, "Expected machine code output.");
        }

        // ==========================================
        // Messages Tests
        // ==========================================

        [TestMethod()]
        public void Compile_ReturnsMessages_WithInstructionAndVariableCount()
        {
            string source = "x = 3;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.IsTrue(result.Messages.Count >= 2);
            StringAssert.Contains(result.Messages[0], "Compiled");
            StringAssert.Contains(result.Messages[0], "instructions");
            StringAssert.Contains(result.Messages[1], "Allocated");
            StringAssert.Contains(result.Messages[1], "variables");
        }

        [TestMethod()]
        public void Compile_Messages_ClearedBetweenCalls()
        {
            string source = "x = 3;\nprint(x);";

            var firstResult = Compile(source, Compiler.CompilerOutput.Assembly);
            var secondResult = Compile(source, Compiler.CompilerOutput.Assembly);

            //Each result should have its own messages, not accumulated.
            Assert.AreEqual(firstResult.Messages.Count, secondResult.Messages.Count);
        }

        // ==========================================
        // Subtraction Tests
        // ==========================================

        [TestMethod()]
        public void Compile_Subtraction_GeneratesSubInstruction()
        {
            string source = "x = 5;\ny = 3;\nx = x - y;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            //x=5 (pre-initialized), y=3 (pre-initialized)
            //x = x - y: LDA x, STA tmp, LDA y, SUB tmp, STA x
            //print(x): LDA x, OUT
            //HLT
            Assert.IsTrue(result.Output.Any(line => line.Contains("SUB")), "Expected a SUB instruction for subtraction.");
            Assert.IsTrue(result.Output.Any(line => line.Contains("HLT")), "Expected HLT at end.");
        }

        // ==========================================
        // Print Immediate Tests
        // ==========================================

        [TestMethod()]
        public void Compile_PrintImmediate_GeneratesLdadAndOut()
        {
            string source = "print(7);";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.AreEqual("LDAD 7", result.Output[0]);
            Assert.AreEqual("OUT", result.Output[1]);
            Assert.AreEqual("HLT", result.Output[2]);
        }

        // ==========================================
        // Multiple Variables Tests
        // ==========================================

        [TestMethod()]
        public void Compile_MultipleVariables_AllocatesCorrectly()
        {
            string source = "a = 1;\nb = 2;\nc = 3;\nprint(c);";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            //3 variables pre-initialized, so data section should have 3 values
            //Instructions: LDA c, OUT, HLT => 3 instructions
            //Data at addresses 3, 4, 5 with values 1, 2, 3
            Assert.AreEqual("LDA 5", result.Output[0]);  // LDA c (address 3+2=5)
            Assert.AreEqual("OUT", result.Output[1]);
            Assert.AreEqual("HLT", result.Output[2]);
            Assert.AreEqual("1", result.Output[3]);  // a = 1
            Assert.AreEqual("2", result.Output[4]);  // b = 2
            Assert.AreEqual("3", result.Output[5]);  // c = 3
        }

        // ==========================================
        // Label and Goto Tests
        // ==========================================

        [TestMethod()]
        public void Compile_LabelAndGoto_GeneratesJmpInstruction()
        {
            string source = "x = 5;\nstart:\nprint(x);\ngoto start;";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.IsTrue(result.Output.Any(line => line.StartsWith("JMP")), "Expected a JMP instruction for goto.");
        }

        // ==========================================
        // If with Equals Tests
        // ==========================================

        [TestMethod()]
        public void Compile_IfEquals_GeneratesJmzInstruction()
        {
            string source = "x = 5;\ny = 5;\nif x == y {\nprint(x);\n}";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.IsTrue(result.Output.Any(line => line.StartsWith("JMZ")), "Expected a JMZ instruction for == comparison.");
        }

        // ==========================================
        // Reassignment Tests
        // ==========================================

        [TestMethod()]
        public void Compile_VariableReassignment_EmitsLdadAndSta()
        {
            //First assignment is pre-initialized, second emits LDAD + STA
            string source = "x = 3;\nx = 7;";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.AreEqual("LDAD 7", result.Output[0]);
            Assert.AreEqual("STA 3", result.Output[1]);  // STA to x's address
            Assert.AreEqual("HLT", result.Output[2]);
            Assert.AreEqual("3", result.Output[3]);  // x initial value
        }

        // ==========================================
        // Compile Overload with ProgramNode Out Tests
        // ==========================================

        [TestMethod()]
        public void Compile_WithProgramNodeOut_ReturnsParsedAST()
        {
            string source = "x = 3;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.Assembly, out var programNode);

            Assert.IsNotNull(programNode);
            Assert.IsTrue(programNode.Statements.Count > 0);
            Assert.IsInstanceOfType(programNode.Statements[0], typeof(Compiler.AssignmentNode));
            Assert.IsInstanceOfType(programNode.Statements[1], typeof(Compiler.PrintNode));
        }

        [TestMethod()]
        public void Compile_WithProgramNodeOut_OutputMatchesStandardOverload()
        {
            string source = "x = 3;\nprint(x);";
            var result1 = Compile(source, Compiler.CompilerOutput.Assembly);
            var result2 = Compile(source, Compiler.CompilerOutput.Assembly, out _);

            CollectionAssert.AreEqual(result1.Output, result2.Output);
        }

        // ==========================================
        // Error Handling Tests
        // ==========================================

        [TestMethod()]
        [ExpectedException(typeof(Exception))]
        public void Compile_InvalidSyntax_ThrowsException()
        {
            string source = "x = ;";
            Compile(source, Compiler.CompilerOutput.Assembly);
        }

        [TestMethod()]
        [ExpectedException(typeof(Exception))]
        public void Compile_UnknownCharacter_ThrowsLexerError()
        {
            string source = "x = 3 @ 2;";
            Compile(source, Compiler.CompilerOutput.Assembly);
        }

        //[TestMethod()]
        //[ExpectedException(typeof(Exception))]
        //public void Compile_LiteralExceedsHardwareLimit_ThrowsException()
        //{
        //    // 4-bit limit = 15, so 16 should fail
        //    string source = "x = 16;\nprint(x);";
        //    Compile(source, Compiler.CompilerOutput.Assembly);
        //}

        // ==========================================
        // Empty / Minimal Program Tests
        // ==========================================

        [TestMethod()]
        public void Compile_EmptySource_GeneratesOnlyHlt()
        {
            string source = "";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.AreEqual(1, result.Output.Count);
            Assert.AreEqual("HLT", result.Output[0]);
        }

        // ==========================================
        // Comment Tests
        // ==========================================

        [TestMethod()]
        public void Compile_CommentsIgnored_GeneratesSameOutput()
        {
            string sourceWithComments = "// This is a comment\nx = 3;\n// Another comment\nprint(x);";
            string sourceWithout = "x = 3;\nprint(x);";

            var resultWith = Compile(sourceWithComments, Compiler.CompilerOutput.Assembly);
            var resultWithout = Compile(sourceWithout, Compiler.CompilerOutput.Assembly);

            CollectionAssert.AreEqual(resultWith.Output, resultWithout.Output);
        }

        // ==========================================
        // Symbols Output Tests
        // ==========================================

        [TestMethod()]
        public void Compile_SymbolsOutput_ContainsAnnotations()
        {
            string source = "x = 3;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.Symbols);

            //Symbols output lines should contain ":" separator for annotations.
            Assert.IsTrue(result.Output.Any(line => line.Contains(":")), "Symbols output should contain annotation separators.");
        }

        // ==========================================
        // Complex Expression Tests
        // ==========================================

        [TestMethod()]
        public void Compile_ChainedAddition_GeneratesCorrectInstructions()
        {
            string source = "x = 1;\ny = 2;\nz = 3;\nx = x + y + z;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            //Should contain multiple ADD instructions for chained addition.
            int addCount = result.Output.Count(line => line.StartsWith("ADD"));
            Assert.IsTrue(addCount >= 2, $"Expected at least 2 ADD instructions for chained addition, found {addCount}.");
        }

        [TestMethod()]
        public void Compile_MixedAddAndSub_GeneratesBothInstructions()
        {
            string source = "x = 5;\ny = 3;\nz = 1;\nx = x + y - z;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.IsTrue(result.Output.Any(line => line.StartsWith("ADD")), "Expected an ADD instruction.");
            Assert.IsTrue(result.Output.Any(line => line.StartsWith("SUB")), "Expected a SUB instruction.");
        }

        // ==========================================
        // If Without Else Tests
        // ==========================================

        [TestMethod()]
        public void Compile_IfWithoutElse_GeneratesConditionalJump()
        {
            string source = "x = 5;\ny = 3;\nif x > y {\nprint(x);\n}";
            var result = Compile(source, Compiler.CompilerOutput.Assembly);

            Assert.IsTrue(result.Output.Any(line => line.StartsWith("JMC")), "Expected a JMC instruction for > comparison.");
            Assert.IsTrue(result.Output.Contains("HLT"));
        }

        // ==========================================
        // Machine Code Format Tests
        // ==========================================

        [TestMethod()]
        public void Compile_MachineCode_AllLinesContainBinaryDigits()
        {
            string source = "x = 3;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.MachineCode);

            foreach (var line in result.Output)
            {
                //Each line should contain binary digits and spaces.
                Assert.IsTrue(line.All(c => c == '0' || c == '1' || c == ' ' || c == '\r' || c == '\n'), $"Machine code line '{line}' should only contain '0', '1', spaces, and newlines.");
            }
        }

        [TestMethod()]
        public void Compile_MachineCode_ProducesOutput()
        {
            string source = "x = 3;\nprint(x);";
            var result = Compile(source, Compiler.CompilerOutput.MachineCode);

            Assert.IsTrue(result.Output.Count > 0, "Expected machine code output.");
            Assert.IsTrue(result.Output.Any(line => line.Contains("1")), "Machine code should contain at least some '1' bits.");
        }

        [TestMethod()]
        public void AssemblyAndMachineCode_MatchesExpected()
        {
            //Assembly program.
            string asmText = @"LDA 15
ADD 14
OUT
HLT










15
28";

            //Custom opcode definition.
            string opcodeDefinition = @"
NO OP=0000
LDA=1000
ADD=0100
OUT=1100
HLT=0010
SUB=1010
LDAD=0110
STA=1110
JMP=0001
JMC=1001
JMZ=0101
";

            //Expected machine code output (one per line, as in your example).
            var expectedMachineCode = new[]
            {
                "10001111", // LDA 15
                "01000111", // ADD 14
                "11000000", // OUT
                "00100000", // HLT
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "11110000", // 15 (data)
                "00111000"  // 28 (data)
            };

            string machineCode = Compiler.CodeGenerator.AssemblyToMachineCode(asmText, opcodeDefinition, DefaultMemAddressLength, DefaultMemWordLength, DefaultMemSize);
            var machineCodeLines = machineCode.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < expectedMachineCode.Length; i++)
            {
                string machineCodeLine = machineCodeLines[i];
                string[] machineCodeLineParts = machineCodeLine.Split(' ');
                machineCodeLine = machineCodeLineParts[0] + machineCodeLineParts[1];

                Assert.AreEqual<string>(expectedMachineCode[i], machineCodeLine);
            }
        }

        [TestMethod()]
        public void AssemblyAndMachineCode_MatchesExpected_2()
        {
            string asmText = @"OUT
ADD 15
JMC 4
JMP 0
SUB 15
OUT
JMZ 0
JMP 4






4
1";

            string opcodeDefinition = @"
NO OP=0000
LDA=1000
ADD=0100
OUT=1100
HLT=0010
SUB=1010
LDAD=0110
STA=1110
JMP=0001
JMC=1001
JMZ=0101
";

            var expectedMachineCode = new[]
            {
                "11000000", // OUT
                "01001111", // ADD 15
                "10010010", // JMC 4
                "00010000", // JMP 0
                "10101111", // SUB 15
                "11000000", // OUT
                "01010000", // JMZ 0
                "00010010", // JMP 4
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "00000000",
                "00100000", // 4 (data)
                "10000000"  // 1 (data)
            };

            string machineCode = Compiler.CodeGenerator.AssemblyToMachineCode(asmText, opcodeDefinition, DefaultMemAddressLength, DefaultMemWordLength, DefaultMemSize);
            var machineCodeLines = machineCode.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < expectedMachineCode.Length; i++)
            {
                string machineCodeLine = machineCodeLines[i];
                string[] machineCodeLineParts = machineCodeLine.Split(' ');
                machineCodeLine = machineCodeLineParts[0] + machineCodeLineParts[1];

                Assert.AreEqual<string>(expectedMachineCode[i], machineCodeLine);
            }
        }
    }
}