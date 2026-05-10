# Software Architecture: AsmIDE & 8-Bit Computer Compiler

## Overview

AsmIDE is a Blazor WebAssembly IDE and compiler for a custom 8-bit computer, built on .NET 9. The architecture is modular, separating the user interface, compiler logic, and hardware communication.

---

## Key Components

### 1. User Interface (UI) Layer
- **Blazor WebAssembly Frontend:** Modern, responsive web-based editor for writing and managing code.
- **File Management:** Open, edit, and save source/definition files.
- **Serial Communication UI:** Connects to hardware for code upload/testing.

### 2. Compiler Core

#### a. Lexer (Tokenizer)
- Converts source code into a sequence of tokens (keywords, identifiers, literals, operators).
- **Output:** List of tokens.

#### b. Parser
- Consumes tokens and builds an Abstract Syntax Tree (AST) representing program structure.
- **Output:** AST nodes for instructions, labels, expressions, etc.

#### c. Abstract Syntax Tree (AST)
- Tree structure modeling the logical structure of the code.
- Used for semantic analysis, code generation, and error reporting.

#### d. Semantic Analyzer
- Checks AST for semantic correctness (undefined labels, instruction validity).
- **Output:** Annotated AST or error messages.

#### e. Code Generator
- Traverses AST and emits target machine code or binary for the 8-bit architecture.
- **Output:** Binary code or memory image for hardware upload.

### 3. Hardware Communication Layer
- **Serial Port Interface:** Communicates with 8-bit computer hardware via UART for code upload and control.
- **State Management:** Tracks connection status, memory layout, and upload progress.

---

## Data Flow Diagram

flowchart TD
	A["User Edits Code in Browser"] --> B["Lexer (Tokenizer)"]
	B --> C["Parser"] 
	C --> D["AST (Abstract Syntax Tree)"] 
	D --> E["Semantic Analyzer"] 
	E --> F["Code Generator"] 
	F --> G["Binary Output"] 
	G --> H["Serial Port Upload"] 
	H --> I["8-bit Computer Hardware"]


---

## Example: Token, AST Node, and Code Generation (Pseudocode)

// Token definition public class Token { public TokenType Type { get; } public string Value { get; } }
// AST node example public abstract class AstNode { } public class InstructionNode : AstNode { public string Mnemonic { get; } public List<AstNode> Operands { get; } }
// Code generator example public class CodeGenerator { public byte[] Generate(AstNode ast) { // Traverse AST and emit binary } }

---

## Summary

- **AsmIDE** is a Blazor WebAssembly IDE for a custom 8-bit computer.
- **Compiler architecture** includes: Lexer, Parser, AST, Semantic Analyzer, Code Generator.
- **Hardware communication** is managed via serial port for code upload and testing.