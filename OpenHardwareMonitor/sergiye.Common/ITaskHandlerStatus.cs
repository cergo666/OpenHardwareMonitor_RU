using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[Guid("EAEC7A8F-27A0-4DDC-8675-14726A01A38A")]
[InterfaceType(1)]
public interface ITaskHandlerStatus
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void UpdateStatus([In] short percentComplete, [In][MarshalAs(UnmanagedType.BStr)] string statusMessage);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void TaskCompleted([In][MarshalAs(UnmanagedType.Error)] int taskErrCode);
}
