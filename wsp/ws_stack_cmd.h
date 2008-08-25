#ifndef WS_STACK_CMD_H
#define WS_STACK_CMD_H

typedef enum {	StackCmdPush, 
				StackCmdDuplicate,
				StackCmdCopy,
				StackCmdSwap,
				StackCmdDiscard,
				StackCmdSlide
} StackCmdType;

typedef struct {
	StackCmdType type;
	int number;
} ws_stack_cmd;

ws_stack_cmd* newStackCmd(StackCmdType type, int n);
void delStackCmd(ws_stack_cmd* cmd);

#endif /* WS_STACK_CMD_H */
