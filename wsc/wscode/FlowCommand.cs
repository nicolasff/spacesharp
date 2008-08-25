using System;
using System.Text;

namespace wsc.wscode
{
	public class FlowCommand : Command
	{
		public enum FlowCommandType {
			FlowCmdMark,
			FlowCmdCallSub,
			FlowCmdJmp,
			FlowCmdJZ,
			FlowCmdJLZ,
			FlowCmdEndSub,
			FlowCmdEndProg,
		};

		new public FlowCommandType Type;
		public string LabelName;
	}
}
