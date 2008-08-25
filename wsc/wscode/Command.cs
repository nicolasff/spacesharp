using System;
using System.Text;

namespace wsc.wscode
{
	abstract public class Command
	{
		public enum Type { StackCmd, ArithmeticCmd, HeapCmd, FlowCmd, IOCmd } ;
	}
}
