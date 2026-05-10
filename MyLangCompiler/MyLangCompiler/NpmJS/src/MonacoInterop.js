import * as monaco from 'monaco-editor';


window.SetDotNetHomeRef = function (ref) {
	window._dotNetHomeRef = ref;
}

// =============================================
// Editor
// =============================================

let monacoEditor_SourceCode;
let monacoEditor_AssemblyCode;
let monacoEditor_MachineCode;
let monacoEditor_OpcodeMap;

//Symbol mapping: sourceLineNumber (1-based) -> array of assemblyLineNumbers (1-based)
let _symbolMapping = {};
let _symbolTrackingEnabled = false;
let _cursorChangeDisposable = null;
let _assemblyDecorations = [];
let _sourceDecorations = [];

//Editor Initialization.
window.InitializeMonaco = function (sourceCode, assemblyCode, machineCode, opcodeMap) {

	// =============================================
	// Define a unified theme (C#-like colors)
	// =============================================
	monaco.editor.defineTheme("MyLangTheme", {
		base: "vs",
		inherit: true,
		rules: [
			//MyLang source code tokens
			{ token: "keyword.mylang", foreground: "0000FF", fontStyle: "bold" },       // blue, like C# keywords
			{ token: "identifier.mylang", foreground: "000000" },
			{ token: "function.mylang", foreground: "795E26" },                          // brown, like C# methods
			{ token: "variable.mylang", foreground: "001080" },                          // dark blue, like C# locals
			{ token: "number.mylang", foreground: "098658" },                            // green, like C# numeric literals
			{ token: "number.float.mylang", foreground: "098658" },
			{ token: "operator.mylang", foreground: "000000" },
			{ token: "delimiter.mylang", foreground: "000000" },
			{ token: "label.mylang", foreground: "9400FF", fontStyle: "bold" },          // purple for labels
			{ token: "string.mylang", foreground: "A31515" },                            // red, like C# strings
			{ token: "comment.mylang", foreground: "008000", fontStyle: "italic" },      // green italic, like C# comments
			{ token: "delimiter.curly.mylang", foreground: "000000" },
			{ token: "delimiter.parenthesis.mylang", foreground: "000000" },

			//Assembly tokens
			{ token: "instruction.asm", foreground: "0000FF", fontStyle: "bold" },       // blue for instructions
			{ token: "number.asm", foreground: "098658" },
			{ token: "comment.asm", foreground: "008000", fontStyle: "italic" },
			{ token: "label.asm", foreground: "9400FF", fontStyle: "bold" },
			{ token: "directive.asm", foreground: "AF00DB" },

			//Machine code tokens
			{ token: "binary.mcode", foreground: "098658", fontStyle: "bold" },
			{ token: "hex.mcode", foreground: "0000FF" },
			{ token: "address.mcode", foreground: "795E26" },
			{ token: "comment.mcode", foreground: "008000", fontStyle: "italic" },
			{ token: "separator.mcode", foreground: "AF00DB" },

			//Opcode map tokens
			{ token: "opcode.omap", foreground: "0000FF", fontStyle: "bold" },
			{ token: "binary.omap", foreground: "098658" },
			{ token: "description.omap", foreground: "795E26" },
			{ token: "comment.omap", foreground: "008000", fontStyle: "italic" },
			{ token: "separator.omap", foreground: "AF00DB" },
			{ token: "number.omap", foreground: "098658" },
		],
		colors: {
			"editor.foreground": "#000000",
		},
	});



	// =============================================
	// MyLang - Source code language (C#-like highlighting)
	// =============================================
	monaco.languages.register({ id: "MyLang" });

	monaco.languages.setMonarchTokensProvider("MyLang", {
		keywords: ["if", "else", "goto", "print"],

		operators: ["=", ">", "<", ">=", "<=", "==", "!=", "+", "-", "*", "/"],

		tokenizer: {
			root: [
				//Comments
				[/\/\/.*$/, "comment.mylang"],

				//Labels (identifier followed by colon at start of line)
				[/^[a-zA-Z_]\w*(?=\s*:)/, "label.mylang"],
				[/:/, "label.mylang"],

				//Function calls like print(...)
				[/(print)(\s*\()/, ["function.mylang", "delimiter.parenthesis.mylang"]],

				//Keywords
				[/\b(if|else|goto|print)\b/, "keyword.mylang"],

				//Identifiers (variables)
				[/[a-zA-Z_]\w*/, "variable.mylang"],

				//Numbers
				[/\d*\.\d+([eE][\-+]?\d+)?/, "number.float.mylang"],
				[/\d+/, "number.mylang"],

				//Operators
				[/[><=!]+/, "operator.mylang"],
				[/[+\-*/]/, "operator.mylang"],
				[/=/, "operator.mylang"],

				//Delimiters
				[/[{}]/, "delimiter.curly.mylang"],
				[/[()]/, "delimiter.parenthesis.mylang"],
				[/;/, "delimiter.mylang"],
			],
		},
	});

	//Completions for MyLang.
	monaco.languages.registerCompletionItemProvider("MyLang", {
		provideCompletionItems: function (model, position) {
			var word = model.getWordUntilPosition(position);
			var range = {
				startLineNumber: position.lineNumber,
				endLineNumber: position.lineNumber,
				startColumn: word.startColumn,
				endColumn: word.endColumn,
			};
			return {
				suggestions: createSourceCodeDependencyProposals(range)
			};
		}
	});

	//Create the source code editor.
	monacoEditor_SourceCode = monaco.editor.create(document.getElementById('sourceCodeEditor'), {
		value: sourceCode,
		language: "MyLang",
		theme: "MyLangTheme",
		automaticLayout: true
	});



	// =============================================
	// MyAssembly - Assembly language
	// =============================================
	monaco.languages.register({ id: "MyAssembly" });

	monaco.languages.setMonarchTokensProvider("MyAssembly", {
		instructions: ["NO OP", "LDA", "ADD", "OUT", "HLT", "SUB", "LDAD", "STA", "JMP", "JMC", "JMZ"],

		tokenizer: {
			root: [
				//Comments
				[/\/\/.*$/, "comment.asm"],

				//Labels (word followed by colon)
				[/^[a-zA-Z_]\w*(?=\s*:)/, "label.asm"],

				//Instructions (must come before generic identifier match)
				[/\b(NO OP|LDA|LDAD|ADD|SUB|OUT|HLT|STA|JMP|JMC|JMZ)\b/, "instruction.asm"],

				//Numbers (operands and address literals)
				[/\b\d+\b/, "number.asm"],

				//Colon for labels
				[/:/, "label.asm"],
			],
		},
	});

	// Completions for assembly
	monaco.languages.registerCompletionItemProvider("MyAssembly", {
		provideCompletionItems: function (model, position) {
			var word = model.getWordUntilPosition(position);
			var range = {
				startLineNumber: position.lineNumber,
				endLineNumber: position.lineNumber,
				startColumn: word.startColumn,
				endColumn: word.endColumn,
			};
			return {
				suggestions: createAssemblyDependencyProposals(range)
			};
		}
	});

	monacoEditor_AssemblyCode = monaco.editor.create(document.getElementById('assemblyCodeEditor'), {
		value: assemblyCode,
		language: "MyAssembly",
		theme: "MyLangTheme",
		automaticLayout: true,
		lineNumbers: function (lineNumber) { return (lineNumber - 1).toString(); }
	});

	// =============================================
	// OpcodeMap - Opcode definitions language
	// =============================================
	monaco.languages.register({ id: "OpcodeMap" });

	monaco.languages.setMonarchTokensProvider("OpcodeMap", {
		tokenizer: {
			root: [
				//Comments
				[/\/\/.*$/, "comment.omap"],

				//Separator (equals sign)
				[/=/, "separator.omap"],

				//Binary values (sequences of 0s and 1s after =)
				[/\b[01]+\b/, "binary.omap"],

				//Numbers
				[/\b\d+\b/, "number.omap"],

				//Opcode names (alphabetic identifiers)
				[/[A-Z][A-Z ]*[A-Z]/, "opcode.omap"],
				[/[a-zA-Z_]\w*/, "opcode.omap"],
			],
		},
	});

	// =============================================
	// MachineCode - Machine code language
	// =============================================
	monaco.languages.register({ id: "MachineCode" });

	monaco.languages.setMonarchTokensProvider("MachineCode", {
		tokenizer: {
			root: [
				//Comments
				[/\/\/.*$/, "comment.mcode"],

				//Binary values (sequences of 0s and 1s)
				[/\b[01]+\b/, "binary.mcode"],

				//Hex values
				[/0x[0-9a-fA-F]+/, "hex.mcode"],

				//Addresses
				[/\b\d+\b/, "address.mcode"],
			],
		},
	});

	monacoEditor_OpcodeMap = monaco.editor.create(document.getElementById('opcodeMapEditor'), {
		value: opcodeMap,
		language: "OpcodeMap",
		theme: "MyLangTheme",
		automaticLayout: true
	});

	monacoEditor_MachineCode = monaco.editor.create(document.getElementById('machineCodeEditor'), {
		value: machineCode,
		language: "MachineCode",
		theme: "MyLangTheme",
		automaticLayout: true,
		lineNumbers: function (lineNumber) { return (lineNumber - 1).toString(); }
	});



	//Attach change listeners after ALL editors are created.
	monacoEditor_SourceCode.onDidChangeModelContent(function (event) {
		if (window._dotNetHomeRef) {
			window._dotNetHomeRef.invokeMethodAsync('SourceCodeEditorContentChanged', monacoEditor_SourceCode.getValue());
		}
	});

	monacoEditor_AssemblyCode.onDidChangeModelContent(function (event) {
		if (window._dotNetHomeRef) {
			window._dotNetHomeRef.invokeMethodAsync('OnAssemblyCodeChanged');
		}
	});

	monacoEditor_OpcodeMap.onDidChangeModelContent(function (event) {
		if (window._dotNetHomeRef) {
			window._dotNetHomeRef.invokeMethodAsync('OnAssemblyCodeChanged');
		}
	});
}


// =============================================
// Editor settings and helper functions
// =============================================
function createSourceCodeDependencyProposals(range) {
	//Returning a static list of proposals, not even looking at the prefix (filtering is done by the Monaco editor).
	//Let's just hard code it for now.
	return [
		{
			label: 'if',
			kind: monaco.languages.CompletionItemKind.Snippet,
			documentation: "If statement: if x > y { //code ... } else { //code ... }",
			insertText: 'if {\n} else {\n}',
			range: range,
		},
		{
			label: 'else',
			kind: monaco.languages.CompletionItemKind.Snippet,
			documentation: "else block of if statement",
			insertText: 'else {\n}',
			range: range,
		},
		{
			label: 'goto',
			kind: monaco.languages.CompletionItemKind.Keyword,
			documentation: "goto",
			insertText: 'goto ',
			range: range,
		},
		{
			label: 'print',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "print statement: print(x)",
			insertText: 'print();',
			range: range,
		},
	];
}

function getKeyWords() {
	//Let's just hard code it for now. 
	const keyWords = [
		"if",
		"else",
		"goto",
		"print"
	];

	return keyWords;
}

function getAssemblyKeyWords() {
	//Let's just hard code it for now. 
	const assemblyKeywords = [
		"NO OP",
		"LDA",
		"ADD",
		"OUT",
		"HLT",
		"SUB",
		"LDAD",
		"STA",
		"JMP",
		"JMC",
		"JMZ",
    ];

	return assemblyKeywords;
}

function createAssemblyDependencyProposals(range) {
	return [
		{
			label: 'NO OP',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "No operation is performed",
			insertText: 'NO OP',
			range: range,
		},
		{
			label: 'LDA',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Operand is loaded into the A register from the specified RAM location.",
			insertText: 'LDA ',
			range: range,
		},
		{
			label: 'ADD',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Operand is loaded into the B register from the specified RAM location. Then it is added together with the operand in the A register. Finally the value is output from the ALU and stored in the A register.",
			insertText: 'ADD ',
			range: range,
		},
		{
			label: 'OUT',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Value is taken from the A register and put in to the output register.",
			insertText: 'OUT',
			range: range,
		},
		{
			label: 'HLT',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Execution of all operations is stopped.",
			insertText: 'HLT',
			range: range,
		},
		{
			label: 'SUB',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Operand is loaded into the B register from the specified RAM location. Then it is subtracted from the operand in the A register. Finally the value is output from the ALU and stored in the A register.",
			insertText: 'SUB ',
			range: range,
		},
		{
			label: 'LDAD',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Operand is loaded directly into the A register.(4 bit value limit)",
			insertText: 'LDAD ',
			range: range,
		},
		{
			label: 'STA',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Value from the A register is stored at the specified memory location.",
			insertText: 'STA ',
			range: range,
		},
		{
			label: 'JMP',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Specifies the next \"line of code\" to be executed by pointing to its location in RAM.",
			insertText: 'JMP ',
			range: range,
		},
		{
			label: 'JMC',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Specifies the next \"line of code\" to be executed by pointing to its location in RAM. But only if the previous operation resulted in a carry.",
			insertText: 'JMC ',
			range: range,
		},
		{
			label: 'JMZ',
			kind: monaco.languages.CompletionItemKind.Function,
			documentation: "Specifies the next \"line of code\" to be executed by pointing to its location in RAM. But only if the previous operation resulted to zero.",
			insertText: 'JMZ ',
			range: range,
		},
	];
}



// =============================================
// Editor set/get content functions
// =============================================

window.SetSourceCodeMonacoContent = function (text) {
	monacoEditor_SourceCode.setValue(text);
}

window.GetSourceCodeMonacoContent = function () {
	return monacoEditor_SourceCode.getValue();
}


window.SetAssemblyCodeMonacoContent = function (text) {
	monacoEditor_AssemblyCode.setValue(text);
}

window.GetAssemblyCodeMonacoContent = function () {
	return monacoEditor_AssemblyCode.getValue();
}

window.SetMachineCodeMonacoContent = function (text) {
	monacoEditor_MachineCode.setValue(text);
}

window.GetMachineCodeMonacoContent = function () {
	return monacoEditor_MachineCode.getValue();
}

window.SetOpcodeMapMonacoContent = function (text) {
	monacoEditor_OpcodeMap.setValue(text);
}

window.GetOpcodeMapMonacoContent = function () {
	return monacoEditor_OpcodeMap.getValue();
}



// =============================================
// Symbol Tracking: Source Code <-> Assembly Mapping
// =============================================

window.SetSymbolMapping = function (mapping) {
	//Mapping is a Dictionary<int, List<int>> serialized as an object: { "1": [1,2,3], "3": [4,5] }
	_symbolMapping = mapping || {};
}

window.EnableSymbolTracking = function (enabled) {
	_symbolTrackingEnabled = enabled;

	if (enabled) {
		//Attach cursor position change listener on source code editor.
		if (_cursorChangeDisposable) {
			_cursorChangeDisposable.dispose();
		}

		_cursorChangeDisposable = monacoEditor_SourceCode.onDidChangeCursorPosition(function (e) {
			if (!_symbolTrackingEnabled)
				return;

			var sourceLineNumber = e.position.lineNumber;
			highlightAssemblyLinesForSourceLine(sourceLineNumber);
		});

		//Trigger immediately for current cursor position.
		var currentPos = monacoEditor_SourceCode.getPosition();
		if (currentPos) {
			highlightAssemblyLinesForSourceLine(currentPos.lineNumber);
		}
	} else {
		//Remove listener and clear highlights.
		if (_cursorChangeDisposable) {
			_cursorChangeDisposable.dispose();
			_cursorChangeDisposable = null;
		}

		clearAssemblyHighlights();
	}
}

function highlightAssemblyLinesForSourceLine(sourceLineNumber) {
	var key = sourceLineNumber.toString();
	var assemblyLines = _symbolMapping[key];

	if (!assemblyLines || assemblyLines.length === 0) {
		clearAssemblyHighlights();
		return;
	}

	// Highlight the active source line
	_sourceDecorations = monacoEditor_SourceCode.deltaDecorations(_sourceDecorations, [
		{
			range: new monaco.Range(sourceLineNumber, 1, sourceLineNumber, 1),
			options: {
				isWholeLine: true,
				className: 'symbol-highlight-source-line'
			}
		}
	]);

	// Build decoration ranges for the mapped assembly lines
	var decorations = assemblyLines.map(function (asmLine) {
		return {
			range: new monaco.Range(asmLine, 1, asmLine, 1),
			options: {
				isWholeLine: true,
				className: 'symbol-highlight-line',
				glyphMarginClassName: 'symbol-highlight-glyph'
			}
		};
	});

	// Apply decorations (deltaDecorations replaces old ones)
	_assemblyDecorations = monacoEditor_AssemblyCode.deltaDecorations(_assemblyDecorations, decorations);

	// Scroll to the first highlighted assembly line
	if (assemblyLines.length > 0) {
		monacoEditor_AssemblyCode.revealLineInCenter(assemblyLines[0]);
	}
}

function clearAssemblyHighlights() {
	if (_assemblyDecorations.length > 0) {
		_assemblyDecorations = monacoEditor_AssemblyCode.deltaDecorations(_assemblyDecorations, []);
	}
	if (_sourceDecorations.length > 0) {
		_sourceDecorations = monacoEditor_SourceCode.deltaDecorations(_sourceDecorations, []);
	}
}



// =============================================
// AST Pan & Zoom
// =============================================

window.InitAstPanZoom = function (containerId) {
    const container = document.getElementById(containerId);
	if (!container)
		return;

    const svg = container.querySelector('svg');
	if (!svg)
		return;

    //Remove any previous listeners attached to the container.
    if (container._astPanZoomAbort) {
        container._astPanZoomAbort.abort();
	}

    const ac = new AbortController();
    container._astPanZoomAbort = ac;
    const signal = ac.signal;

    //Make SVG fill the container instead of overflowing.
    svg.removeAttribute('width');
    svg.removeAttribute('height');
    svg.style.width = '100%';
    svg.style.height = '100%';
    svg.style.display = 'block';

    //Wrap all SVG children in a single <g> so we can apply a unified transform.
    let root = svg.querySelector('g.ast-pz-root');
    if (!root) {
        root = document.createElementNS('http://www.w3.org/2000/svg', 'g');
		root.classList.add('ast-pz-root');

		while (svg.firstChild)
			root.appendChild(svg.firstChild);

        svg.appendChild(root);
    }

    let scale = 1, tx = 0, ty = 0;
    let dragging = false, startX = 0, startY = 0;

    function applyTransform() {
        root.setAttribute('transform', `translate(${tx},${ty}) scale(${scale})`);
    }

    //Zoom on scroll wheel, centred on the mouse position.
    svg.addEventListener('wheel', function (e) {
		e.preventDefault();

		const rect = svg.getBoundingClientRect();

        const mx = e.clientX - rect.left;
		const my = e.clientY - rect.top;

		const factor = e.deltaY < 0 ? 1.1 : 0.9;

        tx = mx - factor * (mx - tx);
        ty = my - factor * (my - ty);
		scale *= factor;

        applyTransform();
    }, { passive: false, signal });

    //Pan on mouse drag.
    svg.addEventListener('mousedown', function (e) {
		if (e.button !== 0)
			return;

        dragging = true;
        startX = e.clientX - tx;
        startY = e.clientY - ty;
        svg.style.cursor = 'grabbing';
    }, { signal });

    window.addEventListener('mousemove', function (e) {
		if (!dragging)
			return;

        tx = e.clientX - startX;
		ty = e.clientY - startY;

        applyTransform();
    }, { signal });

    window.addEventListener('mouseup', function () {
		if (!dragging)
			return;

        dragging = false;
        svg.style.cursor = 'grab';
    }, { signal });

    applyTransform();
};