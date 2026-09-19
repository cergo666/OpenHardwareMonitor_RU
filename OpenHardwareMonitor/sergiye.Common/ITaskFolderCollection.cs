using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[TypeLibType(4288)]
[Guid("79184A66-8664-423F-97F1-637356A5D812")]
public interface ITaskFolderCollection : IEnumerable
{
	[DispId(1610743808)]
	int Count
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743808)]
		get;
	}

	[DispId(0)]
	ITaskFolder this[object index]
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
