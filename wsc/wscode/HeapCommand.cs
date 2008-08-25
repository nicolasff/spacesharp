using System;
using System.Text;

namespace wsc.wscode
{
	public class HeapCommand : Command
	{
		public enum HeapCommandType {
			HeapStore,
			HeapRetrieve
		};
		new public HeapCommandType Type;
	}
}
