using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[ClassInterface(ClassInterfaceType.None)]
[Guid("9F15266D-D7BA-48F0-93C1-E6895F6FE5AC")]
[TypeLibType(2)]
public class TaskHandlerStatusPSClass : ITaskHandlerStatus, TaskHandlerStatusPS, ITaskVariables
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[return: MarshalAs(UnmanagedType.BStr)]
	public virtual extern string GetContext();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[return: MarshalAs(UnmanagedType.BStr)]
	public virtual extern string GetInput();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void SetOutput([In][MarshalAs(UnmanagedType.BStr)] string input);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void TaskCompleted([In][MarshalAs(UnmanagedType.Error)] int taskErrCode);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void UpdateStatus([In] short percentComplete, [In][MarshalAs(UnmanagedType.BStr)] string statusMessage);
}
