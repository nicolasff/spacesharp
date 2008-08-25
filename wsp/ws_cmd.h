#ifndef WS_CMD_H
#define WS_CMD_H

#include "ws_stack_cmd.h"
#include "ws_arith_cmd.h"
#include "ws_heap_cmd.h"
#include "ws_flow_cmd.h"
#include "ws_io_cmd.h"

typedef enum { StackCmd, ArithmeticCmd, HeapCmd, FlowCmd, IOCmd } CmdType;
typedef struct ws_cmd_ {
	union {
		ws_stack_cmd *stk;
		ws_arith_cmd art;	/* just an enum */
		ws_heap_cmd  hep;	/* just an enum */
		ws_flow_cmd *flw;
		ws_io_cmd  io;	/* just an enum */
	} cmd;

	CmdType type;
	struct ws_cmd_ *next;

} ws_cmd;

ws_cmd *newWSCmd();
void delWSCmd(ws_cmd *cmd);

#endif /* WS_CMD_H */

