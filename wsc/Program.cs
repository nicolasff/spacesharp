using System;
using System.Text;

namespace wsc
{
	class Program
	{

		static void Main(string[] args)
		{
			string input=null, output=null;
			if (args.Length != 1 && args.Length != 3) {
				foreach (string s in args) {
					Console.WriteLine("\t{0}", s);
				}
				Console.WriteLine("usage: wsc source.ws [-o bin.exe]");
				return;
			} else if (args.Length == 1) {
				input = args[0];
				output = " .exe";
			} else if (args.Length == 3) {
				if (args[0] == "-o") {
					input = args[2];
					output = args[1];
				} else if (args[1] == "-o") {
					input = args[0];
					output = args[2];
				} else {
					Console.WriteLine("usage: wsc source.ws [-o bin.exe]");
					return;
				}
			} 
		
			//output = "out.exe";
			//			input = @"C:\Documents and Settings\Nicolas\Mes documents\Visual Studio 2005\Projects\wsc\tests\ws\count.ws";
//			input = "test-subprocs.ws";
//			input = "quine.ws";
			//input = "quine-copy.ws";

			wscode.CodeParser cp = new wscode.CodeParser();
//			cp.loadFile("while-true-do-read-number-add-1-print result.ws");
//			cp.loadFile("while-true-do-read-char-add-1-print result.ws");
//			cp.loadFile("rot13.ws");
//			cp.loadFile("hw.ws");
//			cp.loadFile("life.ws");
//			cp.loadFile("heap-test.ws");
//			cp.loadFile("test-17-modulo-6=5.ws");
//			cp.loadFile("test-arith.ws");

			cp.loadFile(input);
			//cp.loadFile("hw.ws");
			wscode.SourceCode code = cp.getSourceCode();

			wscode.WSILGenerator ilg = new wscode.WSILGenerator();
			ilg.generate(code, output);


		}
	}
}
