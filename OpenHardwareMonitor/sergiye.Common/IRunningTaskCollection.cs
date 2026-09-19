using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[Guid("6A67614B-6828-4FEC-AA54-6D52E8F1F2DB")]
[TypeLibType(4288)]
public interface IRunningTaskCollection : IEnumerable
{
	[DispId(1)]
	int Count
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
	}

	[DispId(0)]
	IRunningTask this[object index]
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(-4)]
	[return: MarshalAs(UnmanagedType.Interface)]
	new IEnumerator GetEnumerator();
}
