#ifndef YACC_DEFINITIONS_H
#define YACC_DEFINITIONS_H


#include "../wsp/ws_cmd.h"

typedef struct {

	int n;
	char *s;
	ws_cmd *cmd;

} yystype;

#define YYSTYPE yystype

#endif /* YACC_DEFINITIONS_H */

