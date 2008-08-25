using System;
using System.Text;
using System.Runtime.InteropServices;

namespace wsc.wscode
{
	public class CodeParser
	{
		const string DLL_File = "wsp.dll";

		[DllImport(DLL_File, EntryPoint = "parseFile")]
		static extern IntPtr parseFile(string filename);

		[DllImport(DLL_File, EntryPoint = "getCommandType")]
		static extern Command.Type getCommandType(IntPtr cmd);

		[DllImport(DLL_File, EntryPoint = "getStackCommandType")]
		static extern StackCommand.StackCommandType getStackCommandType(IntPtr cmd);

		[DllImport(DLL_File, EntryPoint = "getStackCommandNumber")]
		static extern int getStackCommandNumber(IntPtr cmd);

		[DllImport(DLL_File, EntryPoint = "getArithmeticCommandType")]
		static extern ArithmeticCommand.ArithmeticCommandType getArithmeticCommandType(IntPtr cmd);

		[DllImport(DLL_File, EntryPoint = "getHeapCommandType")]
		static extern HeapCommand.HeapCommandType getHeapCommandType(IntPtr cmd);

		[DllImport(DLL_File, EntryPoint = "getFlowCommandType")]
		static extern FlowCommand.FlowCommandType getFlowCommandType(IntPtr cmd);

		[DllImport(DLL_File, EntryPoint = "getFlowCommandLabelName")]
		static extern IntPtr getFlowCommandLabelName(IntPtr cmd);

		[DllImport(DLL_File, EntryPoint = "getIOCommandType")]
		static extern IOCommand.IOCommandType getIOCommandType(IntPtr cmd);


		
		[DllImport("wsp.dll", EntryPoint = "getNextCommand")]
		static extern IntPtr getNextCommand(IntPtr cmd);
		
		[DllImport("wsp.dll", EntryPoint = "cleanMemory")]
		static extern void cleanMemory(IntPtr code);

		private IntPtr m_CodePointer;
		

		public CodeParser()
		{
			m_CodePointer = IntPtr.Zero;
		}
		
		~CodeParser() {
			cleanMemory(m_CodePointer);
		}

		public bool loadFile(string fname)
		{
			m_CodePointer = parseFile(fname);
			

			return (m_CodePointer != IntPtr.Zero);
		}

		public SourceCode getSourceCode()
		{
			SourceCode lines = new SourceCode();
			IntPtr cur_cmd = m_CodePointer;
			while (cur_cmd != IntPtr.Zero) {
				Command.Type cmd_type = getCommandType(cur_cmd);
				if (cmd_type == Command.Type.StackCmd) {
					StackCommand cmd = new StackCommand();
					cmd.Number = getStackCommandNumber(cur_cmd);
					cmd.Type = getStackCommandType(cur_cmd);
					lines.Commands.Add(cmd);
				} else if (cmd_type == Command.Type.ArithmeticCmd) {
					ArithmeticCommand cmd = new ArithmeticCommand();
					cmd.Type = getArithmeticCommandType(cur_cmd);
					lines.Commands.Add(cmd);
				} else if (cmd_type == Command.Type.HeapCmd) {
					HeapCommand cmd = new HeapCommand();
					cmd.Type = getHeapCommandType(cur_cmd);
					lines.Commands.Add(cmd);
				} else if (cmd_type == Command.Type.FlowCmd) {
					FlowCommand cmd = new FlowCommand();
					cmd.Type = getFlowCommandType(cur_cmd);
					cmd.LabelName = Marshal.PtrToStringAnsi(getFlowCommandLabelName(cur_cmd));
					if (cmd.LabelName == null) cmd.LabelName = " ";
					lines.Commands.Add(cmd);
				} else if (cmd_type == Command.Type.IOCmd) {
					IOCommand cmd = new IOCommand();
					cmd.Type = getIOCommandType(cur_cmd);
					lines.Commands.Add(cmd);
				}


				cur_cmd = getNextCommand(cur_cmd);

			}

			return lines;

		}
	}
}
