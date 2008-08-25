#ifndef WS_FLOW_CMD_H
#define WS_FLOW_CMD_H

typedef enum {	FlowCmdMark, 
				FlowCmdCallSub, 
				FlowCmdJmp,
				FlowCmdJGZ, 
				FlowCmdJLZ, 
				FlowCmdEndSub, 
				FlowCmdEndProg
} FlowCmdType;

typedef struct {
	FlowCmdType type;
	char* name;
} ws_flow_cmd;

ws_flow_cmd* newFlowCmd(FlowCmdType type, char *name);
void delFlowCmd(ws_flow_cmd* cmd);

#endif /* WS_FLOW_CMD_H */
