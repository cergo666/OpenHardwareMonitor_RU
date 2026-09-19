using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[ClassInterface(ClassInterfaceType.None)]
[DefaultMember("TargetServer")]
[Guid("0F87369F-A4E5-4CFC-BD3E-73E6154572DD")]
[TypeLibType(2)]
public class TaskSchedulerClass : ITaskService, TaskScheduler
{
	[DispId(5)]
	public virtual extern bool Connected
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(5)]
		get;
	}

	[DispId(7)]
	public virtual extern string ConnectedDomain
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(7)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(6)]
	public virtual extern string ConnectedUser
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(6)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(8)]
	public virtual extern uint HighestVersion
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(8)]
		get;
	}

	[DispId(0)]
	public virtual extern string TargetServer
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	public virtual extern void Connect([Optional][In][MarshalAs(UnmanagedType.Struct)] object serverName, [Optional][In][MarshalAs(UnmanagedType.Struct)] object user, [Optional][In][MarshalAs(UnmanagedType.Struct)] object domain, [Optional][In][MarshalAs(UnmanagedType.Struct)] object password);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern ITaskFolder GetFolder([In][MarshalAs(UnmanagedType.BStr)] string Path);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IRunningTaskCollection GetRunningTasks([In] int flags);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern ITaskDefinition NewTask([In] uint flags);
}
