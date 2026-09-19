using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[TypeLibType(4288)]
[Guid("86627EB4-42A7-41E4-A4D9-AC33A72F2D52")]
public interface IRegisteredTaskCollection : IEnumerable
{
	[DispId(1610743808)]
	int Count
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743808)]
		get;
	}

	[DispId(0)]
	IRegisteredTask this[object index]
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
