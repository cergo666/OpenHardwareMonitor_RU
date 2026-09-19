using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[DefaultMember("Path")]
[Guid("8CFAC062-A080-4C15-9A88-AA7C2AF80DFC")]
[TypeLibType(4288)]
public interface ITaskFolder
{
	[DispId(1)]
	string Name
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(0)]
	string Path
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.Interface)]
	ITaskFolder GetFolder([MarshalAs(UnmanagedType.BStr)] string Path);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	[return: MarshalAs(UnmanagedType.Interface)]
	ITaskFolderCollection GetFolders([In] int flags);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	[return: MarshalAs(UnmanagedType.Interface)]
	ITaskFolder CreateFolder([In][MarshalAs(UnmanagedType.BStr)] string subFolderName, [Optional][In][MarshalAs(UnmanagedType.Struct)] object sddl);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	void DeleteFolder([MarshalAs(UnmanagedType.BStr)] string subFolderName, [In] int flags);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(7)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IRegisteredTask GetTask([In][MarshalAs(UnmanagedType.BStr)] string Path);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(8)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IRegisteredTaskCollection GetTasks([In] int flags);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(9)]
	void DeleteTask([In][MarshalAs(UnmanagedType.BStr)] string Name, [In] int flags);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(10)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IRegisteredTask RegisterTask([In][MarshalAs(UnmanagedType.BStr)] string Path, [In][MarshalAs(UnmanagedType.BStr)] string XmlText, [In] int flags, [In][MarshalAs(UnmanagedType.Struct)] object UserId, [In][MarshalAs(UnmanagedType.Struct)] object password, [In] TASK_LOGON_TYPE LogonType, [Optional][In][MarshalAs(UnmanagedType.Struct)] object sddl);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(11)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IRegisteredTask RegisterTaskDefinition([In][MarshalAs(UnmanagedType.BStr)] string Path, [In][MarshalAs(UnmanagedType.Interface)] ITaskDefinition pDefinition, [In] int flags, [In][MarshalAs(UnmanagedType.Struct)] object UserId, [In][MarshalAs(UnmanagedType.Struct)] object password, [In] TASK_LOGON_TYPE LogonType, [Optional][In][MarshalAs(UnmanagedType.Struct)] object sddl);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(12)]
	[return: MarshalAs(UnmanagedType.BStr)]
	string GetSecurityDescriptor(int securityInformation);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(13)]
	void SetSecurityDescriptor([In][MarshalAs(UnmanagedType.BStr)] string sddl, [In] int flags);
}
