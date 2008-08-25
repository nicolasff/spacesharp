using System;
using System.Text;

namespace wsc.wscode
{
	public class StackCommand : Command
	{
		public enum StackCommandType
		{
			StackCmdPush, 
			StackCmdDuplicate,
			StackCmdCopy,
			StackCmdSwap,
			StackCmdDiscard,
			StackCmdSlide
		} ;

		public int Number = 0;
		new public StackCommandType Type;

	}
}
