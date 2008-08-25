#include "ws_flow_cmd.h"
#include <stdlib.h>

ws_flow_cmd* newFlowCmd(FlowCmdType type, char* name)
{
	ws_flow_cmd* ret = NULL;
	if((ret = malloc(sizeof(ws_flow_cmd)))==NULL) return NULL;

	ret->type = type;
	ret->name = name;

	return ret;
}

void delFlowCmd(ws_flow_cmd* cmd)
{
	if(cmd) free(cmd);
}

