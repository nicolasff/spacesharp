using System;
using System.Text;

namespace wsc.wscode
{
	public class IOCommand : Command
	{
		public enum IOCommandType {
			IOPrintChar,
			IOPrintNumber,
			IOReadChar,
			IOReadNumber
		};
		new public IOCommandType Type;
	}
}
