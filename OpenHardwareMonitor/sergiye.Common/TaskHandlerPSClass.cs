using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[TypeLibType(2)]
[ClassInterface(ClassInterfaceType.None)]
[Guid("F2A69DB7-DA2C-4352-9066-86FEE6DACAC9")]
public class TaskHandlerPSClass : ITaskHandler, TaskHandlerPS
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void Pause();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void Resume();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void Start([In][MarshalAs(UnmanagedType.IUnknown)] object pHandlerServices, [In][MarshalAs(UnmanagedType.BStr)] string Data);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void Stop([MarshalAs(UnmanagedType.Error)] out int pRetCode);
}
