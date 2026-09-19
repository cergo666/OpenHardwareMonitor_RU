using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[Guid("839D7762-5121-4009-9234-4F0D19394F04")]
[InterfaceType(1)]
public interface ITaskHandler
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Start([In][MarshalAs(UnmanagedType.IUnknown)] object pHandlerServices, [In][MarshalAs(UnmanagedType.BStr)] string Data);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Stop([MarshalAs(UnmanagedType.Error)] out int pRetCode);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Pause();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Resume();
}
