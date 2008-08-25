#include "wsp.h"
#include <stdio.h>
#include <stdlib.h>

#include "ws_cmd.h"


extern FILE *yyin;
extern int yyparse();

ws_cmd *ws_program;

WSP_API ws_cmd* parseFile(char *filename)
{
	yyin = fopen(filename, "r");
	if(yyin == NULL) return NULL;
	yyparse();
	fclose(yyin);
	return ws_program;
}

WSP_API int getCommandType(ws_cmd* cmd)
{
	if(!cmd) return -1;
	return cmd->type;
}

WSP_API StackCmdType getStackCommandType(ws_cmd* cmd)
{
	if(!cmd) return -1;
	return cmd->cmd.stk->type;
}

WSP_API int getStackCommandNumber(ws_cmd* cmd)
{
	if(!cmd) return -1;
	return cmd->cmd.stk->number;
}

WSP_API int getArithmeticCommandType(ws_cmd* cmd)
{
	if(!cmd) return -1;
	return cmd->cmd.art;
}

WSP_API int getHeapCommandType(ws_cmd* cmd)
{
	if(!cmd) return -1;
	return cmd->cmd.hep;
}

WSP_API int getFlowCommandType(ws_cmd* cmd)
{
	if(!cmd) return -1;
	return cmd->cmd.flw->type;
}

WSP_API char* getFlowCommandLabelName(ws_cmd* cmd)
{
	if(!cmd) return NULL;
	return cmd->cmd.flw->name;
}

WSP_API int getIOCommandType(ws_cmd* cmd)
{
	if(!cmd) return -1;
	return cmd->cmd.io;
}

WSP_API ws_cmd* getNextCommand(ws_cmd* cmd)
{
	if(!cmd) return NULL;
	return cmd->next;
}

WSP_API void cleanMemory(ws_cmd* cmd)
{
	if(cmd != NULL){
		cleanMemory(cmd->next);
		if (cmd->type == StackCmd){
			free(cmd->cmd.stk);
		} else if(cmd->type == FlowCmd) {
			free (cmd->cmd.flw);
		}
		free(cmd);
	}

}
