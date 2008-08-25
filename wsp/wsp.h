#ifdef WIN32
#define WSP_API __declspec(dllexport)
#else 
#define WSP_API
#endif

#include "ws_cmd.h"


WSP_API ws_cmd* parseFile(char *filename);
WSP_API int getCommandType(ws_cmd* cmd);
WSP_API StackCmdType getStackCommandType(ws_cmd* cmd);
WSP_API int getStackCommandNumber(ws_cmd* cmd);
WSP_API int getArithmeticCommandType(ws_cmd* cmd);
WSP_API int getHeapCommandType(ws_cmd* cmd);
WSP_API int getFlowCommandType(ws_cmd* cmd);
WSP_API char* getFlowCommandLabelName(ws_cmd* cmd);
WSP_API int getIOCommandType(ws_cmd* cmd);

WSP_API ws_cmd* getNextCommand(ws_cmd* cmd);

WSP_API void cleanMemory(ws_cmd* cmd);

