#include "ws_cmd.h"
#include <stdlib.h>

ws_cmd *newWSCmd()
{
	ws_cmd * ret = malloc(sizeof(ws_cmd));
	if(!ret) return NULL;
	ret->next = NULL;
	return ret;
}
void delWSCmd(ws_cmd *cmd)
{
	if(cmd) free(cmd);
}

