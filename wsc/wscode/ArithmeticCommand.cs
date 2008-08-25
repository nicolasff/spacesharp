using System;
using System.Text;

namespace wsc.wscode
{
	public class ArithmeticCommand : Command
	{
		public enum ArithmeticCommandType{	
			ArithAddition,
			ArithSubstraction,
			ArithMultiplication,
			ArithDivision,
			ArithModulo 
		};

		new public ArithmeticCommandType Type;
	}
}
