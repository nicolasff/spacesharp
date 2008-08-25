#include "ws_stack_cmd.h"
#include <stdlib.h>

ws_stack_cmd* newStackCmd(StackCmdType type, int n)
{
	ws_stack_cmd* ret = NULL;
	if((ret = malloc(sizeof(ws_stack_cmd)))==NULL) return NULL;

	ret->type = type;
	ret->number = n;

	return ret;
}

void delStackCmd(ws_stack_cmd* cmd)
{
	if(cmd) free(cmd);
}
