grammar Tiger;

options {
	language = CSharp3;
	k = 2;
	output = AST;
}

// -------------------------- Tokens Section --------------------------
tokens {
	NEGATIVE;
	LETINEND;
	FIELD_TYPE;
	FIELD_VALUE;
	
	/* function declaration */
	CALL;
	
	/* type declarations */
	TYPE_DECLARATION;
	ALIAS_TYPE;
	ARRAY_TYPE;
	RECORD_TYPE;
	
	/* variable declaration */
	VARIABLE_DECLARATION;

	/* function declaration */
	FUNCTION_DECLARATION;
	PARAMETERS;	
	
	/* expression block */
	EXPRESSIONS_BLOCK;

	RECORD;
	
	BLOCK_DECLARATIONS;
	
	/* block declarations */
	FUNCTIONS_BLOCK;
	VARIABLES_BLOCK;
	TYPES_BLOCK;
	
	PROGRAM;
	
	/* conditions */
	IF_THEN;
	IF_THEN_ELSE;
}

// -------------------------- Lexer Options --------------------------
@lexer::header
{
	using System;
	using System.Text;
}

@lexer::namespace { TigerCompiler.Parsing }

@lexer::members
{
    public override void ReportError(RecognitionException exc)
    {
        throw new ParsingException(GetErrorMessage(exc, TokenNames), exc);
    }
}
// -------------------------- Parser Options --------------------------
@parser::header
{
	using System;
	using System.Collections;
}

@parser::namespace { TigerCompiler.Parsing }

@parser::members 
{
    public override void ReportError(RecognitionException exc) 
    {
        throw new ParsingException(GetErrorMessage(exc, TokenNames), exc);
    }
}
// -------------------------- Lexer Rules --------------------------

/* punctuation symbols */
ASSIGN		: ':=';
COMMA			: ',';
COLON		  : ':';
SEMI			: ';'; /* Semicolon */
LP				:	'('; /* Left Parenthisis*/
RP				:	')'; /* Right Parenthisis*/
LSB				: '['; /* Left Square Brackets */
RSB				: ']'; /* Right Square Brackets */
LCB				:	'{'; /* Left Curly Bracket */
RCB				:	'}'; /* Right Curly Bracket */
PERIOD		:	'.';
PLUS			:	'+';
MINUS			: '-';
STAR			: '*';
DIV		    :	'/';
EQ				:	'='; /* Equal Sing */
NOTEQ			: '<>'; /* Not Equal Sing */
LETHAN		: '<='; /* Less Or Equal Than Sign */
GETHAN		: '>='; /* Great Or Equal Than Sign */
LTHAN			: '<'; /* Less Than Sign */
GTHAN			: '>'; /* Great Than Sign */
AND				: '&';
OR				:	'|';
USCORE 		:	'_'; /* Under Score */

/* reserved words */
FUNCTION	: 'function';
ARRAY			: 'array';
WHILE			: 'while';
BREAK     :	'break';
TYPE			: 'type';
ELSE 			:	'else';
THEN			:	'then';
NIL				:	'nil';
FOR				: 'for';
LET				:	'let';
END       :	'end';
VAR				: 'var';
IF				: 'if';
DO				:	'do';
TO				:	'to';
OF				:	'of';
IN				:	'in';

fragment LETTER		:	('a' .. 'z' | 'A' .. 'Z');

fragment DIGIT		:	'0' .. '9';

INT								:	( DIGIT )+;

ID								: LETTER (LETTER | DIGIT | USCORE)*;

/* ASCII Printable Characters */
fragment PCHAR
	:	' ' | '!' | '#' .. '[' | ']' .. '~';

fragment ASCII
	:	('0' DIGIT DIGIT | ('1' ('0'..'1' DIGIT | '2' ('0'..'7'))));

STRING					  
	@init{ StringBuilder SB = new StringBuilder(); }
	@after{ $text = SB.ToString(); }
	:
	'"'
	(
		'\\' (
						'n'  { SB.Append("\n"); } |
						'r'  { SB.Append("\r"); } |
						't'  { SB.Append("\t"); } |
						'"'  { SB.Append("\""); } |
						'\\' { SB.Append("\\"); } |
						x = ASCII  { SB.Append((char)(int.Parse($x.Text))); } |
						(' ' | '\t' | '\r' | '\n')+ '\\'
					)
		|
		x = PCHAR { SB.Append($x.Text); }
	)*
	'"';

/* Multiline Comments */
ML_COMMENTS				: '/*' (options { greedy = false; } : ML_COMMENTS | . )* '*/' { $channel = Hidden; };

/* Whitespace */
WS								:	(' ' | '\t' | '\r' | '\n')+ { $channel = Hidden; };

//-------------------------------------------------------
public program
	:	x = expror EOF -> ^(PROGRAM $x);

expression_sequence
	:	expror (SEMI! expror)*;

expror
	:	(x = exprand -> $x) (options{ greedy=true; } : OR y = exprand -> ^(OR $expror $y))*;

exprand
	:	(x = exprrel -> $x) (options{ greedy=true; } : AND y = exprrel -> ^(AND $exprand $y))*;

exprrel
	:	(x = arith -> $x) (options{ greedy=true; } :
	  (EQ     y = arith -> ^(EQ     $x $y)
	  |NOTEQ  y = arith -> ^(NOTEQ  $x $y)
	  |LETHAN y = arith -> ^(LETHAN $x $y)
	  |GETHAN y = arith -> ^(GETHAN $x $y)
	  |LTHAN  y = arith -> ^(LTHAN  $x $y)
	  |GTHAN  y = arith -> ^(GTHAN  $x $y))
	   )?
	;
	
arith
	:	(x = term -> $x) (options{ greedy=true; } :
	  ( MINUS y = term -> ^(MINUS $arith $y)
	  | PLUS  z = term -> ^(PLUS  $arith $z))
	  )*
	;

term
	:	(x = fact -> $x) (options{ greedy=true; } :
	   ( STAR y = fact -> ^(STAR $term $y)
	   | DIV  z = fact -> ^(DIV  $term $z))
	   )*
	;

fact
	:	atom
	| MINUS x = fact -> ^(NEGATIVE $x)
	;

atom
	:	literals
	| call
	| loop
	| if_then_else
	| record
	| array_lvalue_assign
	| LP exprs = expression_sequence? RP -> ^(EXPRESSIONS_BLOCK $exprs?)
	| let_in_end
	| BREAK;

literals
	:	INT | STRING | NIL;

expression_list
	:	expror (COMMA! expror)*;

call
	:	ID LP fparams = expression_list? RP -> ^(CALL ID $fparams?);

loop
	:	for | while;

for
	:	FOR^ ID ASSIGN! expror TO! expror DO! expror;

while
	:	WHILE^ expror DO! expror;

let_in_end
	: LET decs = declaration_list IN exps = expression_sequence? END -> ^(LETINEND ^(BLOCK_DECLARATIONS $decs) ^(EXPRESSIONS_BLOCK $exps?));

if_then_else
	:	IF x = expror THEN y = expror (options { greedy = true; } : ELSE z = expror -> ^(IF_THEN_ELSE $x $y $z) | -> ^(IF_THEN $x $y));

declaration_list
	: (types | variables | functions)+;

types
	:	(options{ greedy = true; }: type_declaration)+ -> ^(TYPES_BLOCK type_declaration+);
	
variables
	:	(options{ greedy = true; }: variable_declaration)+ -> ^(VARIABLES_BLOCK variable_declaration+);
	
functions
	:	(options{ greedy = true; }: function_declaration)+ -> ^(FUNCTIONS_BLOCK function_declaration+);

type_declaration
	:	TYPE id = ID EQ
	( type_id = ID                  -> ^(TYPE_DECLARATION $id ^(ALIAS_TYPE $type_id))
	| ARRAY OF type_id = ID         -> ^(TYPE_DECLARATION $id ^(ARRAY_TYPE $type_id))
	| LCB fields = type_fields? RCB -> ^(TYPE_DECLARATION $id ^(RECORD_TYPE $fields?))
	);

field_type
	:	pname = ID COLON ptype = ID -> ^(FIELD_TYPE $pname $ptype);

field_value
	:	name = ID EQ value = expror -> ^(FIELD_VALUE $name $value);

type_fields
	:	field_type (COMMA! field_type) *;

value_fields
	:	field_value (COMMA! value_fields)?;

variable_declaration
	: VAR id = ID
	( ASSIGN value = expror                    -> ^(VARIABLE_DECLARATION $id $value)
	| COLON type_id = ID ASSIGN value = expror -> ^(VARIABLE_DECLARATION $id $value $type_id)
	);
	
function_declaration
	:	FUNCTION fname = ID LP fparams = type_fields? RP
	( EQ fbody = expror 								 -> ^(FUNCTION_DECLARATION $fname ^(PARAMETERS $fparams?) $fbody)
	| COLON ftype = ID EQ fbody = expror -> ^(FUNCTION_DECLARATION $fname ^(PARAMETERS $fparams?) $fbody $ftype)
	);
	
array_lvalue_assign
	:	(ID LSB expror RSB OF) => (id = ID LSB n = expror RSB OF exprs = expror -> ^(ARRAY $id $n $exprs))
	  | lvalue ( ASSIGN^ expror)? ;

lvalue
	:	ID ( (PERIOD^ ID) | (LSB^ expror RSB!) )*;

record
	:	type_id = ID LCB fields = value_fields ? RCB -> ^(RECORD $type_id $fields?);
