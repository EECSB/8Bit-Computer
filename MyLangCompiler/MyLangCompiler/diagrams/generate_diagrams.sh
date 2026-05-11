#!/bin/bash

# Install Mermaid CLI if needed:
# npm install -g @mermaid-js/mermaid-cli

mkdir -p docs/images
mkdir -p docs/diagrams

# System Architecture
cat > docs/diagrams/system-architecture.mmd <<'EOF'
flowchart TD
    A[Blazor WebAssembly UI] --> B[Monaco Editor Integration]
    A --> C[Compiler Core]
    A --> D[Serial Communication Layer]

    C --> E[Lexer]
    E --> F[Parser]
    F --> G[AST Generation]
    G --> H[Code Generator]
    H --> I[Assembly Output]
    H --> J[Machine Code Output]
    H --> K[Symbol Output]

    D --> L[UART / Serial Upload]
    J --> L
    L --> M[Custom 8-bit Computer]
EOF

# Compiler Pipeline
cat > docs/diagrams/compiler-pipeline.mmd <<'EOF'
flowchart LR
    A[Source Code] --> B[Lexer / Tokenizer]
    B --> C[Token Stream]
    C --> D[Parser]
    D --> E[Abstract Syntax Tree]
    E --> F[Code Generator]
    F --> G[Assembly]
    F --> H[Machine Code]
    F --> I[Symbol Table]
EOF

# Lexer Workflow
cat > docs/diagrams/lexer-workflow.mmd <<'EOF'
flowchart TD
    A[Read Character] --> B{Whitespace?}
    B -- Yes --> A
    B -- No --> C{Comment?}
    C -- Yes --> A
    C -- No --> D{Digit?}
    D -- Yes --> E[Parse Number]
    D -- No --> F{Identifier?}
    F -- Yes --> G[Parse Identifier / Keyword]
    F -- No --> H[Parse Operator]
EOF

# AST Structure
cat > docs/diagrams/ast-structure.mmd <<'EOF'
classDiagram
    class ProgramNode {
        Statements
    }
    class AssignmentNode {
        Variable
        Expression
    }
    class PrintNode {
        Value
    }
    class GotoNode {
        Label
    }
    class IfBlockNode {
        Condition
        Statements
        ElseStatements
    }
    class BinaryOpNode {
        Left
        Operator
        Right
    }
EOF

# AST Example
cat > docs/diagrams/ast-example.mmd <<'EOF'
graph TD
    A[Program]
    A --> B[Assignment x=5]
    A --> C[Assignment y=x+2]
    A --> D[Print y]
    C --> E[BinaryOp +]
    E --> F[Variable x]
    E --> G[Literal 2]
EOF

# Codegen Flow
cat > docs/diagrams/codegen-flow.mmd <<'EOF'
flowchart TD
    A[AST Node] --> B{Type}
    B --> C[Assignment]
    B --> D[Print]
    B --> E[Goto]
    B --> F[If Block]
    C --> G[Expression Evaluation]
    G --> H[Store Variable]
    D --> I[Load Value]
    I --> J[OUT]
    E --> K[JMP]
    F --> L[Conditional Jumps]
EOF

# Memory Layout
cat > docs/diagrams/memory-layout.mmd <<'EOF'
flowchart TD
    A[Instruction Section] --> B[Program Instructions]
    B --> C[HLT]
    C --> D[Variable Section]
    D --> E[Temporary Variables]
EOF

# UI Architecture
cat > docs/diagrams/ui-architecture.mmd <<'EOF'
flowchart TD
    A[Home.razor] --> B[Monaco Editor]
    A --> C[Compiler Controls]
    A --> D[Output Views]
    A --> E[Serial Upload]
    B --> F[MonacoInterop.js]
    E --> G[SerialInterop.js]
EOF

# Compilation Sequence
cat > docs/diagrams/compilation-sequence.mmd <<'EOF'
sequenceDiagram
    participant User
    participant Lexer
    participant Parser
    participant AST
    participant CodeGen
    participant Hardware
    User->>Lexer: Source Code
    Lexer->>Parser: Tokens
    Parser->>AST: AST
    AST->>CodeGen: Nodes
    CodeGen->>Hardware: Machine Code
EOF

# Render all diagrams
for file in docs/diagrams/*.mmd; do
    name=$(basename "$file" .mmd)
    mmdc -i "$file" -o "docs/images/${name}.svg"
    echo "Generated docs/images/${name}.svg"
done

echo "All diagrams generated successfully."
