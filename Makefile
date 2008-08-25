CSOURCES=wsp/ws_flow_cmd.o \
	wsp/ws_cmd.o \
	wsp/ws_stack_cmd.o \
	wsp/wsp.o
PARSER=lex-yacc/ws_tab.o lex-yacc/lex.yy.o
CFLAGS=-pedantic -Wall -fPIC
CSSOURCES=wsc/Program.cs \
	 wsc/wscode/ArithmeticCommand.cs \
	 wsc/wscode/CodeParser.cs \
	 wsc/wscode/Command.cs \
	 wsc/wscode/FlowCommand.cs \
	 wsc/wscode/HeapCommand.cs \
	 wsc/wscode/IOCommand.cs \
	 wsc/wscode/SourceCode.cs \
	 wsc/wscode/StackCommand.cs \
	 wsc/wscode/WSILGenerator.cs

MCS=gmcs

ifeq ($(shell uname -s),Darwin)
	MCS=/Library/Frameworks/Mono.framework/Commands/mcs
endif

all: dll csharp

csharp: ${CSSOURCES}
	@echo "[MCS]	"${CSSOURCES}
	@${MCS} -debug -out:wsc.exe ${CSSOURCES}
	
dll: parser ${CSOURCES} ${PARSER}
	@echo "[LINK]	libwsp.dll"
	@${CC} -shared -o libwsp.dll ${CSOURCES} ${PARSER}
	@ln -f -s libwsp.dll libwsp.dll.so
	
	
%.o: %.c
	@echo "[CC]	$@"
	@${CC} ${CFLAGS} -o $@ -c $<


parser: lex-yacc/ws.l lex-yacc/ws.y
	@echo "[BISON]	ws.y"
	@bison -d lex-yacc/ws.y -o lex-yacc/ws_tab.c
	@echo "[FLEX]	ws.l"
	@flex -olex-yacc/lex.yy.c lex-yacc/ws.l 

clean:
	@echo "[CLEAN]"
	@rm -f ${CSOURCES} wsc.exe libwsp.dll libwsp.dll.so
