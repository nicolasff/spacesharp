%{ 

int already_started = 0;

int my_yyinput(char *tampon, int taille);

#define YY_INPUT(tampon,resultat,taille_max) resultat = my_yyinput(tampon, taille_max)


#include <stdlib.h>
#include <stdio.h>
#ifdef WIN32
#define alloca _alloca
#endif

#include "ws_tab.h"



%}
%option nounput

%%

\t		{ /*printf("TAB "); */	return TAB;		}
" "		{ /*printf("SPACE ");	*/ return SPACE;	}
\n		{ /*printf("LF ");*/	return LF;		}

.		{}


%%

int yywrap(){ return 1; }

int my_yyinput(char *tampon, int taille) {
	int c;
/*	printf("yyinput(taille=%d) !\n", taille); */
	if (!feof(yyin)) {
/*		printf("!feof\n"); */
		c = fgetc(yyin);
/*		printf("c = %d\n", c); */
		if (0 != c){
			*tampon = c;
			return 1;
		}
	} 
	/*printf("feof\n"); */
	return YY_NULL;	/* 0 chars */
}

int yyerror(char *s){ printf("Error: %s\n", s); return 0; }
