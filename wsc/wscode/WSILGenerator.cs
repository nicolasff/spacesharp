using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace wsc.wscode
{
	public class WSILGenerator
	{
		/*
		public void YY()
		{
			System.Collections.Stack s = new System.Collections.Stack();
			s.Push(0);
			goto thamer;

			suite:
				Console.Write('X');
				Console.Write('\n');
				return;
			thamer:
			Console.Write('S');
			Console.Write('\n');
			int i = (int)s.Pop();
			switch (i) {
				case 0: goto suite;
			}
		}
		*/
		public void XX()
		{
			System.Collections.Stack s = new System.Collections.Stack();
			s.Push(42);
			Console.Write((int)s.Pop());
			
		}
		
		public WSILGenerator()
		{
		
		}

		public void generate(SourceCode code, string outfile)
		{
#if __MonoCS__
			int pos = outfile.LastIndexOf("/");
#else
			int pos = outfile.LastIndexOf("\\");
#endif
			string outbin = outfile.Substring(1 + pos);
//			Console.WriteLine("outbin={0}", outbin);

            MethodInfo PushMethod = typeof (System.Collections.Stack).GetMethod ("Push");
            MethodInfo PopMethod = typeof (System.Collections.Stack).GetMethod ("Pop");
            MethodInfo PeekMethod = typeof (System.Collections.Stack).GetMethod ("Peek");
            Type Int32Type = typeof (System.Int32);

			#region Pre-definitions
			AssemblyName objAsmName = new AssemblyName();
			objAsmName.Name = "whitespace";
			objAsmName.Version = new Version("1.0.0.0");

			AssemblyBuilder objAsm = AppDomain.CurrentDomain.DefineDynamicAssembly(objAsmName,
												AssemblyBuilderAccess.Save);

			ModuleBuilder objModule = objAsm.DefineDynamicModule(objAsmName.Name, outbin, true);
			TypeBuilder objClass = objModule.DefineType(objAsmName.Name, TypeAttributes.Public);
			MethodBuilder objMethod = objClass.DefineMethod("Main",
												 MethodAttributes.Static | MethodAttributes.Public,
												 Type.GetType("void"),
				/*Type.GetTypeArray(new object[] {"string" })*/
												 null
												 );
			ILGenerator objILgenerator = objMethod.GetILGenerator();

			/// contains all the labels
			System.Collections.Hashtable hash_Labels = new System.Collections.Hashtable();

			/// contains the list of callers, for each label.
			/// sub_callers[sub_name] = (List<Label>)[lbl_after1, lbl_after2, ...]
			Dictionary<string, List<int>> sub_callers = new Dictionary<string, List<int>>();

			

			//Type[] wlParams = new Type[] {typeof(string),
            //         typeof(object)};

			
			/* declare locals */
			LocalBuilder local_heap = objILgenerator.DeclareLocal(typeof(System.Collections.Hashtable));
			LocalBuilder local_one = objILgenerator.DeclareLocal(typeof(Int32));
			LocalBuilder local_two = objILgenerator.DeclareLocal(typeof(Int32));
			LocalBuilder local_jmptable = objILgenerator.DeclareLocal(typeof(System.Collections.Stack));
			LocalBuilder local_stack = objILgenerator.DeclareLocal(typeof(System.Collections.Stack));
			LocalBuilder local_TMP_stack = objILgenerator.DeclareLocal(typeof(System.Collections.Stack));


			// Define the heap, to be stored in locals[0] :
			objILgenerator.Emit(OpCodes.Newobj, typeof(System.Collections.Hashtable).GetConstructor(Type.EmptyTypes));
			objILgenerator.Emit(OpCodes.Stloc, local_heap);	// store in [0]
			objILgenerator.Emit(OpCodes.Newobj, typeof(System.Collections.Stack).GetConstructor(Type.EmptyTypes));
			objILgenerator.Emit(OpCodes.Stloc, local_jmptable);	// store in [3]
			objILgenerator.Emit(OpCodes.Newobj, typeof(System.Collections.Stack).GetConstructor(Type.EmptyTypes));
			objILgenerator.Emit(OpCodes.Stloc, local_stack);	// store in [4]
			objILgenerator.Emit(OpCodes.Newobj, typeof(System.Collections.Stack).GetConstructor(Type.EmptyTypes));
			objILgenerator.Emit(OpCodes.Stloc, local_TMP_stack);	// store in [5]

			
#if TEST
			/* begin test */

			objILgenerator.Emit(OpCodes.Ldc_I4, 77);	// push 77
			objILgenerator.Emit(OpCodes.Ldc_I4, 42);	// push 77

				// stack = [value,addr]
			objILgenerator.Emit(OpCodes.Stloc, local_one);	// value in [1], stack = [addr]
			objILgenerator.Emit(OpCodes.Stloc, local_two);	// address in [2], stack = []
			objILgenerator.Emit(OpCodes.Ldloc, local_heap);	// heap in [0], stack = [heap]
			objILgenerator.Emit(OpCodes.Ldloc, local_one);	// stack = [addr,heap]
			objILgenerator.Emit(OpCodes.Box, Int32Type);	// stack = [int(addr),heap]
			objILgenerator.Emit(OpCodes.Ldloc, local_two);	// stack = [value, int(addr),heap]
			objILgenerator.Emit(OpCodes.Box, Int32Type);	// stack = [int(value),int(addr),heap]
			objILgenerator.EmitCall(OpCodes.Call, typeof(System.Collections.Hashtable).GetMethod("set_Item"), new Type[] { typeof(object), typeof(object) });

			objILgenerator.Emit(OpCodes.Ret);	// push 77

			objClass.CreateType();
			objAsm.SetEntryPoint(objMethod, PEFileKinds.ConsoleApplication);
			objAsm.Save("out.exe");
			return;

			/* end test */
#endif
			#endregion

			int i = 0;
			Dictionary<int, Label> label_Numbers = new Dictionary<int, Label>();

			foreach (Command base_cmd in code.Commands) {
				if (base_cmd is FlowCommand){
					FlowCommand flow = base_cmd as FlowCommand;
					if(flow.Type == FlowCommand.FlowCommandType.FlowCmdMark) {
						Label lbl = objILgenerator.DefineLabel();
						hash_Labels[flow.LabelName] = lbl;
					} else if(flow.Type == FlowCommand.FlowCommandType.FlowCmdCallSub){
						Label lbl_after_call = objILgenerator.DefineLabel();	// define labels for "after-call"
						// then remember all callers for this label
						if (sub_callers.ContainsKey(flow.LabelName) == false){ // create it
							sub_callers[flow.LabelName] = new List<int>();
						}
					//	Console.WriteLine("[{2}] : caller of '{0}' : {1}", flow.LabelName, i, outfile);
						sub_callers[flow.LabelName].Add(i);

						label_Numbers[i] = lbl_after_call;	
						i++;
					}
				}
			}

			//int nb_labels = tmp_calls_list.Count;
	
			int nb_calls_emitted = 0;
			//string lastLabelName = null;
			foreach (Command base_cmd in code.Commands) {
				#region Stack Commands
				if (base_cmd is StackCommand) {
					StackCommand cmd = base_cmd as StackCommand;
					switch(cmd.Type){
						case StackCommand.StackCommandType.StackCmdPush:
							//objILgenerator.Emit(OpCodes.Ldc_I4, cmd.Number);
							/**/
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							objILgenerator.Emit(OpCodes.Ldc_I4, cmd.Number);
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							// push on stack
							//MethodInfo m = PushMethod;

							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
							
							
							break;
						case StackCommand.StackCommandType.StackCmdDuplicate:
							//objILgenerator.Emit(OpCodes.Dup);
							/**/
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							// push top on evaluation stack
							objILgenerator.EmitCall(OpCodes.Call, PeekMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
							
							break;
						case StackCommand.StackCommandType.StackCmdCopy:
							// copy the nth item to top
							for (i = 0; i < cmd.Number; i++) {
								objILgenerator.Emit(OpCodes.Ldloc, local_stack);
								objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
								objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
								objILgenerator.Emit(OpCodes.Ldind_I4);
                                objILgenerator.Emit (OpCodes.Stloc, local_one);	// store in [1]
								
								// evaluation stack contains element i
								objILgenerator.Emit(OpCodes.Ldloc, local_TMP_stack);
                                objILgenerator.Emit (OpCodes.Ldloc, local_one);	// get [1]
								objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
								objILgenerator.EmitCall(OpCodes.Call, PushMethod, null); // push to temp
								// ok, stored elt i on TMP stack
							}
                            
                            // save [1] in [2]. this is the copied value.
                            objILgenerator.Emit (OpCodes.Ldloc, local_one);	// get from [1]
                            objILgenerator.Emit (OpCodes.Stloc, local_two);	// store in [2]

                            
							//put all of the others back
							for (i = 0; i < cmd.Number; i++) {
								objILgenerator.Emit(OpCodes.Ldloc, local_TMP_stack);
								objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
								objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
								objILgenerator.Emit(OpCodes.Ldind_I4);
                                objILgenerator.Emit (OpCodes.Stloc, local_one); // store in [1]
								
								// evaluation stack contains element i
								objILgenerator.Emit(OpCodes.Ldloc, local_stack);
                                objILgenerator.Emit (OpCodes.Ldloc, local_one); // take [1]
								objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
								objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
								// ok, stored elt i on real stack
							}
							// put nth back dupped on top
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							objILgenerator.Emit(OpCodes.Ldloc, local_two);
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
							break;

						case StackCommand.StackCommandType.StackCmdSwap:
							// stack = [X,Y]
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store X in [1]

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Stloc, local_two);	// store Y in [2]

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.Emit(OpCodes.Ldloc, local_one);		// push X
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.Emit(OpCodes.Ldloc, local_two);		// push Y
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
							// stack is [Y,X] now
							break;
						case StackCommand.StackCommandType.StackCmdDiscard:
							//objILgenerator.Emit(OpCodes.Pop);
							/**/
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Pop);
							break;
						case StackCommand.StackCommandType.StackCmdSlide:
							//objILgenerator.Emit(OpCodes.Stloc, local_one);	/* save top */
							//for (int item_nb = 0; item_nb < cmd.Number; item_nb++) {	/* remove n items */
							//	objILgenerator.Emit(OpCodes.Pop);
							//}
							//objILgenerator.Emit(OpCodes.Ldloc, local_one);		/* put back the top */
							/**/
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store top in [1]
							for (int item_nb = 0; item_nb < cmd.Number; item_nb++) {	/* remove n items */
								objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
								objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
								objILgenerator.Emit(OpCodes.Pop);
							}
							// and put back the top
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.Emit(OpCodes.Ldloc, local_one);		// push top
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);

							break;


					}
				}
				#endregion
				#region Arithmetic Commands
				else if (base_cmd is ArithmeticCommand) {
					ArithmeticCommand cmd = base_cmd as ArithmeticCommand;
					switch (cmd.Type) {
						case ArithmeticCommand.ArithmeticCommandType.ArithAddition:
							//objILgenerator.Emit(OpCodes.Add);
							//Console.WriteLine("Add");
							/**/
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Add);	// add
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store a+b in [1]

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// load a+b
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);

							break;
						case ArithmeticCommand.ArithmeticCommandType.ArithSubstraction:
							//objILgenerator.Emit(OpCodes.Sub);
							//Console.WriteLine("Sub");
							/**/
							// stack = [Y,X] (we want X-Y)
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store Y in [1]
							
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// get Y from [1]
							
							objILgenerator.Emit(OpCodes.Sub);	// substract
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store X-Y in [1]
							
							// evaluation stack is empty now
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// get X-Y from [1]
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
							// evaluation stack is empty now, mem stack top is X-Y	
							
							break;
						case ArithmeticCommand.ArithmeticCommandType.ArithMultiplication:
							//objILgenerator.Emit(OpCodes.Mul);
							//Console.WriteLine("Mul");
							/**/
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Mul);	// Multiply
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store X*Y in [1]
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// get X*Y from [1]
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);

							break;
						case ArithmeticCommand.ArithmeticCommandType.ArithDivision:
							//objILgenerator.Emit(OpCodes.Div);
							//objILgenerator.Emit(OpCodes.Conv_I4);	// convert to int
							//Console.WriteLine("Div");
							/**/
							// stack = [Y,X] (we want X/Y)
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store Y in [1]

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// get Y from [1]

							objILgenerator.Emit(OpCodes.Div);	// divide
							objILgenerator.Emit(OpCodes.Conv_I4);	// convert to int
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store X/Y in [1]

							// evaluation stack is empty now
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// get X/Y from [1]
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
							// evaluation stack is empty now, mem stack top is X/Y
							break;

						case ArithmeticCommand.ArithmeticCommandType.ArithModulo:
							// calculating a % b, stack = [b,a]
							//objILgenerator.Emit(OpCodes.Stloc, local_two);	// b in [2], stack = [a]
							//objILgenerator.Emit(OpCodes.Stloc, local_one);	// a in [1], stack = []
							//objILgenerator.Emit(OpCodes.Ldloc, local_one);	// stack = [a]
							//objILgenerator.Emit(OpCodes.Ldloc, local_two);	// stack = [b,a]
							//objILgenerator.Emit(OpCodes.Div);		// stack = [a/b]
							//objILgenerator.Emit(OpCodes.Conv_I4);	// stack = [int(a/b)]
							//objILgenerator.Emit(OpCodes.Ldloc, local_two);	// stack = [b,int(a/b)]
							//objILgenerator.Emit(OpCodes.Mul);		// stack = [b*int(a/b)]
							//objILgenerator.Emit(OpCodes.Neg);		// stack = [-b*int(a/b)]
							//objILgenerator.Emit(OpCodes.Ldloc, local_one);	// stack = [a,-b*int(a/b)]
							//objILgenerator.Emit(OpCodes.Add);		// stack = [a%b]
							/**/
							// stack = [Y,X] (we want X%Y)
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store Y in [1]

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// get Y from [1]

							objILgenerator.Emit(OpCodes.Rem);	// remainder
							objILgenerator.Emit(OpCodes.Conv_I4);	// convert to int
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// store X%Y in [1]

							// evaluation stack is empty now
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// get X%Y from [1]
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
							// evaluation stack is empty now, mem stack top is X%Y
							break;

					}

				}
				#endregion
				#region IO Commands
				else if (base_cmd is IOCommand) {
					IOCommand cmd = base_cmd as IOCommand;
					switch(cmd.Type){
						case IOCommand.IOCommandType.IOPrintNumber:
							//Type[] wiParams = new Type[] { Int32Type };
							//MethodInfo writeiMI = typeof(Console).GetMethod("Write", wiParams);
							//objILgenerator.EmitCall(OpCodes.Call, writeiMI, null);
							/**/
							Type[] wiParams = new Type[] { Int32Type };
							MethodInfo writeiMI = typeof(Console).GetMethod("Write", wiParams);
							
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.EmitCall(OpCodes.Call, writeiMI, null);	// write
							
							break;

						case IOCommand.IOCommandType.IOPrintChar:
							//Type[] wcParams = new Type[] { typeof(char) };
							//MethodInfo writecMI = typeof(Console).GetMethod("Write", wcParams);
							//objILgenerator.EmitCall(OpCodes.Call, writecMI, null);
							/**/
							Type[] wcParams = new Type[] { typeof(char) };
							MethodInfo writecMI = typeof(Console).GetMethod("Write", wcParams);

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.EmitCall(OpCodes.Call, writecMI, null);	// write
							break;

						case IOCommand.IOCommandType.IOReadNumber:

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);

							MethodInfo readlineMI = typeof(Console).GetMethod("ReadLine", Type.EmptyTypes);
							objILgenerator.EmitCall(OpCodes.Call, readlineMI, null);
							MethodInfo parseMI = typeof(Int32).GetMethod("Parse", new Type[] { typeof(string) });
							objILgenerator.EmitCall(OpCodes.Call, parseMI, null);

							//value = stack.pop
							//address = stack.pop

							objILgenerator.Emit(OpCodes.Stloc, local_two);	// value in [2], stack = [addr]
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// address in [1], stack = []
							objILgenerator.Emit(OpCodes.Ldloc, local_heap);	// heap in [0], stack = [heap]
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// stack = [addr,heap]
							objILgenerator.Emit(OpCodes.Box, Int32Type);	// stack = [int(addr),heap]
							objILgenerator.Emit(OpCodes.Ldloc, local_two);	// stack = [value, int(addr),heap]
							objILgenerator.Emit(OpCodes.Box, Int32Type);	// stack = [int(value),int(addr),heap]
//							objILgenerator.EmitCall(OpCodes.Call, typeof(System.Collections.Hashtable).GetMethod("set_Item"), new Type[] { typeof(object), typeof(object) });
							objILgenerator.EmitCall(OpCodes.Call, typeof(System.Collections.Hashtable).GetMethod("set_Item"), null);
							

							break;							

						case IOCommand.IOCommandType.IOReadChar:

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);	// load the stack
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);

							MethodInfo readcMI = typeof(Console).GetMethod("Read", Type.EmptyTypes);
							objILgenerator.EmitCall(OpCodes.Call, readcMI, null);

							//value = stack.pop
							//address = stack.pop

							objILgenerator.Emit(OpCodes.Stloc, local_two);	// value in [2], stack = [addr]
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// address in [1], stack = []
							objILgenerator.Emit(OpCodes.Ldloc, local_heap);	// heap in [0], stack = [heap]
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// stack = [addr,heap]
							objILgenerator.Emit(OpCodes.Box, Int32Type);	// stack = [int(addr),heap]
							objILgenerator.Emit(OpCodes.Ldloc, local_two);	// stack = [value, int(addr),heap]
							objILgenerator.Emit(OpCodes.Box, Int32Type);	// stack = [int(value),int(addr),heap]
//							objILgenerator.EmitCall(OpCodes.Call, typeof(System.Collections.Hashtable).GetMethod("set_Item"), new Type[] { typeof(object), typeof(object) });
							objILgenerator.EmitCall(OpCodes.Call, typeof(System.Collections.Hashtable).GetMethod("set_Item"), null);

							break;
						default:
							break;
					}

				}
				#endregion
				#region Flow Commands
				else if (base_cmd is FlowCommand) {
					FlowCommand cmd = base_cmd as FlowCommand;
					switch (cmd.Type) {
						case FlowCommand.FlowCommandType.FlowCmdEndProg:
							objILgenerator.Emit(OpCodes.Ret);
							break;

						case FlowCommand.FlowCommandType.FlowCmdMark:
							//lastLabelName = cmd.LabelName;
							objILgenerator.MarkLabel((Label)hash_Labels[cmd.LabelName]);
							break;

						case FlowCommand.FlowCommandType.FlowCmdJmp:
							objILgenerator.Emit(OpCodes.Br, (Label)hash_Labels[cmd.LabelName]);
							break;
						case FlowCommand.FlowCommandType.FlowCmdJZ:
							//objILgenerator.Emit(OpCodes.Dup);	// DO NOT duplicate number on top
							objILgenerator.Emit(OpCodes.Ldc_I4, 0);	// push 0
							// get top of stack
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							
							objILgenerator.Emit(OpCodes.Beq, (Label)hash_Labels[cmd.LabelName]); // jmp if eq
							break;

						case FlowCommand.FlowCommandType.FlowCmdJLZ:
							//objILgenerator.Emit(OpCodes.Dup);	// DO NOT duplicate number on top

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							

							objILgenerator.Emit(OpCodes.Ldc_I4, 0);	// push 0
							objILgenerator.Emit(OpCodes.Blt, (Label)hash_Labels[cmd.LabelName]); // jmp if lt
							break;

						case FlowCommand.FlowCommandType.FlowCmdCallSub:
							Label lbl_after_call = label_Numbers[nb_calls_emitted];
							
							objILgenerator.Emit(OpCodes.Ldloc, local_jmptable);		// push jmptable
							objILgenerator.Emit(OpCodes.Ldc_I4, nb_calls_emitted);	// push label number
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							// push on stack
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
							objILgenerator.Emit(OpCodes.Br, (Label)hash_Labels[cmd.LabelName]); // emit call 

							objILgenerator.MarkLabel(lbl_after_call);	// HERE is the label-after-call

							nb_calls_emitted++; // finally, increment number
							break;

						case FlowCommand.FlowCommandType.FlowCmdEndSub:
							// pop label-after-call number
							
							objILgenerator.Emit(OpCodes.Ldloc, local_jmptable);
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);						// convert to Int

							objILgenerator.Emit(OpCodes.Stloc, local_one);

							//Console.WriteLine("sub '{0}' : ret", lastLabelName);
							/*
							foreach (int lbl_caller in sub_callers[lastLabelName]) {
								objILgenerator.Emit(OpCodes.Ldloc, local_one);
								objILgenerator.Emit(OpCodes.Ldc_I4, lbl_caller);
								objILgenerator.Emit(OpCodes.Beq, label_Numbers[lbl_caller]);
							}
							*/
							
							for (i = 0; i < label_Numbers.Count; i++) {
								objILgenerator.Emit(OpCodes.Ldloc, local_one);
								objILgenerator.Emit(OpCodes.Ldc_I4, i);
								objILgenerator.Emit(OpCodes.Beq, label_Numbers[i]);
							}

							objILgenerator.Emit(OpCodes.Ret);
							//objILgenerator.Emit(OpCodes.Nop);

							break;
					}
				}
				#endregion
				#region Heap Commands
				else if (base_cmd is HeapCommand) {
					HeapCommand cmd = base_cmd as HeapCommand;
					switch (cmd.Type) {
						case HeapCommand.HeapCommandType.HeapStore:
							//value = stack.pop
							//address = stack.pop

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							objILgenerator.Emit(OpCodes.Stloc, local_two);	// value in [2], stack = [addr]

							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// address in [1], stack = []

							objILgenerator.Emit(OpCodes.Ldloc, local_heap);	// heap in [0], stack = [heap]
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// stack = [addr,heap]
							objILgenerator.Emit(OpCodes.Box, Int32Type);	// stack = [int(addr),heap]
							objILgenerator.Emit(OpCodes.Ldloc, local_two);	// stack = [value, int(addr),heap]
							objILgenerator.Emit(OpCodes.Box, Int32Type);	// stack = [int(value),int(addr),heap]
							objILgenerator.EmitCall(OpCodes.Call, typeof(System.Collections.Hashtable).GetMethod("set_Item"), null);
							// evaluation stack = []
							break;
						case HeapCommand.HeapCommandType.HeapRetrieve:
							//address = stack.pop
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							objILgenerator.EmitCall(OpCodes.Call, PopMethod, null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);	// unbox 
							objILgenerator.Emit(OpCodes.Ldind_I4);
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// value in [1], stack = []
							
							objILgenerator.Emit(OpCodes.Ldloc, local_heap);	// heap in [0], stack = [heap]
							objILgenerator.Emit(OpCodes.Ldloc, local_one);	// stack = [addr,heap]
							objILgenerator.Emit(OpCodes.Box, Int32Type);	// stack = [int(addr),heap]
							objILgenerator.EmitCall(OpCodes.Call, typeof(System.Collections.Hashtable).GetMethod("get_Item"), null);
							objILgenerator.Emit(OpCodes.Unbox, Int32Type);
							objILgenerator.Emit(OpCodes.Ldind_I4);
							// stack = [value]
							objILgenerator.Emit(OpCodes.Stloc, local_one);	// value in [1], stack = []
							objILgenerator.Emit(OpCodes.Ldloc, local_stack);
							objILgenerator.Emit(OpCodes.Ldloc, local_one);
							objILgenerator.Emit(OpCodes.Box, Int32Type); // convert to int
							// push on stack
							objILgenerator.EmitCall(OpCodes.Call, PushMethod, null);
							// evaluation stack = []

							break;

						default:
						break;
					}
				}
				#endregion
			}

			Console.WriteLine("saving to '{0}'", outbin);

			objClass.CreateType();
			objAsm.SetEntryPoint(objMethod, PEFileKinds.ConsoleApplication);
			//objAsm.Save(outfile);

			string outdir = ".";
			try { outdir = outfile.Substring(0, pos); } catch (Exception) { }
			objAsm.Save(outbin);
			try { if(outdir != ".")System.IO.File.Delete(outfile); } catch(Exception){}
			try { System.IO.File.Move(outbin, outfile); } catch (Exception) { }
			
		}
	}
}
// this file is too long. TODO: refactor.
