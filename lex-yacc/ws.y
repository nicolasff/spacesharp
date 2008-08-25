%token SPACE TAB LF

%{ 
#include <stdlib.h>
#include <stdio.h>
#ifdef WIN32
#define alloca _alloca
#endif

#include "../wsp/ws_nb_label.h"
#include "yacc-definitions.h"

extern int yyerror(char *s);
extern int yylex();
extern ws_cmd *ws_program;

ws_cmd *last = NULL;
%}

%%

ws_code: cmd_list { ws_program = $1.cmd; }
;

cmd_list: cmd	{ $$ = $1;  last = $$.cmd; /*printf("\n----\n");*/}
	| cmd_list cmd	{ $$ = $1; last->next = $2.cmd; last = last->next; /*printf("\n----\n");*/}
;

cmd: stack_cmd			{ /*printf("stack_cmd as cmd\n");*/ $$ = $1; $$.cmd->type = StackCmd; }
	| arithmetic_cmd	{ /*printf("arithmetic_cmd as cmd\n");*/ $$ = $1; $$.cmd->type = ArithmeticCmd; }
	| heap_cmd			{ /*printf("heap_cmd as cmd\n");*/ $$ = $1; $$.cmd->type = HeapCmd; }
	| flow_cmd			{ /*printf("flow_cmd(%d) as cmd\n",$1.cmd->cmd.flw->n);*/ $$ = $1; $$.cmd->type = FlowCmd; }
	| io_cmd			{ /*printf("io_cmd as cmd\n");*/ $$ = $1; $$.cmd->type = IOCmd; }
; 

stack_cmd: SPACE stack_next					{ $$ = $2; }
arithmetic_cmd: TAB SPACE arithmetic_next	{ $$ = $3; }
heap_cmd: TAB TAB heap_next					{ $$ = $3; }
flow_cmd: LF flow_next						{ $$ = $2; }
io_cmd: TAB LF io_next						{ $$ = $3; }


Number: SPACE OptSimpleNumber LF	{ $$.n = $2.n; /*printf("got positive number %d\n", $$.n);*/ }
	| TAB OptSimpleNumber LF		{ $$.n = -($2.n);  /*printf("got negative number %d\n", $$.n);*/ }
;
OptSimpleNumber:	{ $$.n = 0; }
	| SimpleNumber	{ $$ = $1; }
;
SimpleNumber: SPACE			{ $$.n = 0; }
	| TAB					{ $$.n = 1; }
	| SimpleNumber SPACE	{ $$.n *= 2; }
	| SimpleNumber TAB		{ $$.n *= 2;  $$.n++; }
;

stack_next:	SPACE Number /* push */	{
		/*printf("Stack push %d\n", $2.n);*/
		$$.cmd = newWSCmd();
		$$.cmd->cmd.stk = newStackCmd(StackCmdPush, $2.n);
	}
	| LF SPACE /* duplicate top */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.stk = newStackCmd(StackCmdDuplicate, 0);
	}
	| TAB SPACE Number /* copy the nth elt on top */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.stk = newStackCmd(StackCmdCopy, $3.n);
	}
	| LF TAB /* swap two elts */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.stk = newStackCmd(StackCmdSwap, 0);
	}
	| LF LF /* discard top elt */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.stk = newStackCmd(StackCmdDiscard, 0);
	}
	| TAB LF Number /* Slide n elts off the stack, keeping the top */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.stk = newStackCmd(StackCmdSlide, 0);
	}
;

arithmetic_next: SPACE SPACE /* addition */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.art = ArithAddition;
	}
	| SPACE TAB /* substraction */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.art = ArithSubstraction;
	}
	| SPACE LF	/* Multiplication */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.art = ArithMultiplication;
	}
	| TAB SPACE	/* Integer Division */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.art = ArithDivision;
	}
	| TAB TAB	/* Modulo */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.art = ArithModulo;
	}
;

heap_next: SPACE /* Store */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.hep = HeapStore;
	}
	| TAB	/* Retrieve */ {
		$$.cmd = newWSCmd();
		$$.cmd->cmd.hep = HeapRetrieve;
	}
;

flow_next: SPACE SPACE Label /* Mark a location in the program */	{
		$$.cmd = newWSCmd();
		/*printf("Set label %s\n", $3.s);*/
		$$.cmd->cmd.flw = newFlowCmd(FlowCmdMark, $3.s);
	}
	| SPACE TAB Label	/* Call a subroutine */	{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.flw = newFlowCmd(FlowCmdCallSub, $3.s);
	}
	| SPACE LF Label	/* Jump unconditionally to a label */	{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.flw = newFlowCmd(FlowCmdJmp, $3.s);
	}
	| TAB SPACE Label	/* Jump to a label if the top of the stack is zero */	{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.flw = newFlowCmd(FlowCmdJGZ, $3.s);
	}
	| TAB TAB	Label	/* Jump to a label if the top of the stack is negative */	{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.flw = newFlowCmd(FlowCmdJLZ, $3.s);
	}
	| TAB LF			/* End a subroutine and transfer control back to the caller */	{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.flw = newFlowCmd(FlowCmdEndSub, NULL);
	}
	| LF LF				/* End the program */	{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.flw = newFlowCmd(FlowCmdEndProg, NULL);
	}

;

Label: OptSimpleLabel LF;

OptSimpleLabel:		{ $$.s = NULL; }
	| SimpleLabel	{ $$.s = $1.s; }
;
	
SimpleLabel: SPACE			{ $$.s = malloc(2); $$.s[0]='S'; $$.s[1]='\0'; $$.n = 2; }
	| TAB					{ $$.s = malloc(2); $$.s[0]='T'; $$.s[1]='\0'; $$.n = 2; }
	| SimpleLabel SPACE		{ $$.s = realloc($1.s, 1+$1.n); $$.s[$1.n-1]='S'; $$.s[$1.n]='\0'; $$.n=$1.n+1; }
	| SimpleLabel TAB		{ $$.s = realloc($1.s, 1+$1.n); $$.s[$1.n-1]='T'; $$.s[$1.n]='\0'; $$.n=$1.n+1; }
;

io_next: SPACE SPACE /* Output the character at the top of the stack */{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.io = IOPrintChar;
	}
	| SPACE TAB	/* Output the number at the top of the stack */{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.io = IOPrintNumber;
	}
	| TAB SPACE	/* Read a character and place it in the location given by the top of the stack */{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.io = IOReadChar;
	}
	| TAB TAB	/* Read a number and place it in the location given by the top of the stack */{
		$$.cmd = newWSCmd();
		$$.cmd->cmd.io = IOReadNumber;
	}
;

%%

