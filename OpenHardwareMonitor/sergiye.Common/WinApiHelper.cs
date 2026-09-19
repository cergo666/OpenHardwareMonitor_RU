using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace sergiye.Common;

public static class WinApiHelper
{
	public struct LUID
	{
		public uint m_nLowPart;

		public uint m_nHighPart;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct TokPriv1Luid
	{
		public int Count;

		public long Luid;

		public int Attr;
	}

	public struct TOKEN_PRIVILEGES
	{
		public int m_nPrivilegeCount;

		public LUID m_oLUID;

		public int m_nAttributes;
	}

	public struct PROFILEINFO
	{
		public int dwSize;

		public int dwFlags;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpUserName;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpProfilePath;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpDefaultPath;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpServerName;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpPolicyPath;

		public IntPtr hProfile;
	}

	public struct STARTUPINFO
	{
		public int cb;

		public string lpReserved;

		public string lpDesktop;

		public string lpTitle;

		public int dwX;

		public int dwY;

		public int dwXSize;

		public int dwXCountChars;

		public int dwYCountChars;

		public int dwFillAttribute;

		public int dwFlags;

		public short wShowWindow;

		public short cbReserved2;

		public IntPtr lpReserved2;

		public IntPtr hStdInput;

		public IntPtr hStdOutput;

		public IntPtr hStdError;
	}

	public struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;

		public IntPtr hThread;

		public int dwProcessID;

		public int dwThreadID;
	}

	public struct SECURITY_ATTRIBUTES
	{
		public int Length;

		public IntPtr lpSecurityDescriptor;

		public bool bInheritHandle;
	}

	[Flags]
	public enum CreationFlags
	{
		CREATE_SUSPENDED = 4,
		CREATE_NEW_CONSOLE = 0x10,
		NORMAL_PRIORITY_CLASS = 0x20,
		CREATE_NEW_PROCESS_GROUP = 0x200,
		CREATE_UNICODE_ENVIRONMENT = 0x400,
		CREATE_SEPARATE_WOW_VDM = 0x800,
		CREATE_DEFAULT_ERROR_MODE = 0x4000000,
		CREATE_NO_WINDOW = 0x8000000
	}

	[Flags]
	public enum LogonFlags
	{
		LOGON_WITH_PROFILE = 1,
		LOGON_NETCREDENTIALS_ONLY = 2
	}

	public enum WTS_INFO_CLASS
	{
		WTSInitialProgram,
		WTSApplicationName,
		WTSWorkingDirectory,
		WTSOEMId,
		WTSSessionId,
		WTSUserName,
		WTSWinStationName,
		WTSDomainName,
		WTSConnectState,
		WTSClientBuildNumber,
		WTSClientName,
		WTSClientDirectory,
		WTSClientProductId,
		WTSClientHardwareId,
		WTSClientAddress,
		WTSClientDisplay,
		WTSClientProtocolType
	}

	public enum SECURITY_IMPERSONATION_LEVEL
	{
		SecurityAnonymous,
		SecurityIdentification,
		SecurityImpersonation,
		SecurityDelegation
	}

	public enum TOKEN_TYPE
	{
		TokenPrimary = 1,
		TokenImpersonation
	}

	[Flags]
	public enum TokenAccess : uint
	{
		STANDARD_RIGHTS_REQUIRED = 0xF0000u,
		STANDARD_RIGHTS_READ = 0x20000u,
		TOKEN_ASSIGN_PRIMARY = 1u,
		TOKEN_DUPLICATE = 2u,
		TOKEN_IMPERSONATE = 4u,
		TOKEN_QUERY = 8u,
		TOKEN_QUERY_SOURCE = 0x10u,
		TOKEN_ADJUST_PRIVILEGES = 0x20u,
		TOKEN_ADJUST_GROUPS = 0x40u,
		TOKEN_ADJUST_DEFAULT = 0x80u,
		TOKEN_ADJUST_SESSIONID = 0x100u,
		TOKEN_READ = 0x20008u,
		TOKEN_ALL_ACCESS = 0xF01FFu
	}

	public delegate bool EnumWindowsCallbackDelegate(IntPtr hWnd, uint lParam);

	public struct ScrollInfoStruct
	{
		public int CbSize;

		public int FMask;

		public int NMin;

		public int NMax;

		public int NPage;

		public int NPos;

		public int NTrackPos;
	}

	public enum RtbwFlags
	{
		RtbwDefault,
		RtbwKeepundo,
		RtbwSelection
	}

	public struct Settextex
	{
		public RtbwFlags flags;

		public long codepage;
	}

	public struct Input
	{
		public int type;

		public InputUnion u;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct InputUnion
	{
		[FieldOffset(0)]
		public readonly Mouseinput mi;

		[FieldOffset(0)]
		public Keybdinput ki;

		[FieldOffset(0)]
		public readonly Hardwareinput hi;
	}

	public struct Mouseinput
	{
		public readonly int dx;

		public readonly int dy;

		public readonly uint mouseData;

		public readonly uint dwFlags;

		public readonly uint time;

		public readonly IntPtr dwExtraInfo;
	}

	public struct Keybdinput
	{
		public ushort wVk;

		public ushort wScan;

		public uint dwFlags;

		public readonly uint time;

		public IntPtr dwExtraInfo;
	}

	public struct Hardwareinput
	{
		public readonly uint uMsg;

		public readonly ushort wParamL;

		public readonly ushort wParamH;
	}

	public struct APPBARDATA
	{
		public int cbSize;

		public IntPtr hWnd;

		public int uCallbackMessage;

		public int uEdge;

		public RECT rc;

		public IntPtr lParam;
	}

	public struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;
	}

	public struct POINT
	{
		public readonly int X;

		public readonly int Y;
	}

	[Flags]
	public enum NotifyIconDataFlags
	{
		Message = 1,
		Icon = 2,
		Tip = 4,
		State = 8,
		Info = 0x10
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class NotifyIconData
	{
		private int size = Marshal.SizeOf(typeof(NotifyIconData));

		public IntPtr Window;

		public int ID;

		public NotifyIconDataFlags Flags;

		public int CallbackMessage;

		public IntPtr Icon;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string Tip;

		public int State;

		public int StateMask;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string Info;

		public int TimeoutOrVersion;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string InfoTitle;

		public int InfoFlags;
	}

	public enum NotifyIconMessage
	{
		Add,
		Modify,
		Delete
	}

	public delegate int CallbackToFindChildWindow(int hWnd, int lParam);

	public const ushort WM_NULL = 0;

	public const ushort WM_DESTROY = 2;

	public const ushort WM_SETREDRAW = 11;

	public const ushort WM_CLOSE = 16;

	public const ushort WM_QUIT = 18;

	public const ushort WM_COMMMAND = 273;

	public const ushort WM_SYS_COMMAND = 274;

	public const ushort WM_VSCROLL = 277;

	public const ushort WM_INIT_MENU_POPUP = 279;

	public const ushort WM_MOUSE_MOVE = 512;

	public const ushort WM_LBUTTON_DOWN = 513;

	public const ushort WM_LBUTTON_UP = 514;

	public const ushort WM_LBUTTON_DBLCLK = 515;

	public const ushort WM_RBUTTON_DOWN = 516;

	public const ushort WM_RBUTTON_UP = 517;

	public const ushort WM_RBUTTON_DBLCLK = 518;

	public const ushort WM_MBUTTON_DOWN = 519;

	public const ushort WM_MBUTTON_UP = 520;

	public const ushort WM_MBUTTON_DBLCLK = 521;

	public const ushort WM_TRAY_MOUSE_MESSAGE = 2048;

	public const ushort WM_USER = 1024;

	public const ushort WM_MOUSEWHEEL = 522;

	public const ushort WM_WININICHANGE = 26;

	public const ushort WM_SETTEXT = 12;

	public const int InputMouse = 0;

	public const int InputKeyboard = 1;

	public const int InputHardware = 2;

	public const uint KeyeventfExtendedkey = 1u;

	public const uint KeyeventfKeyup = 2u;

	public const uint KeyeventfUnicode = 4u;

	public const uint KeyeventfScancode = 8u;

	public const uint Xbutton1 = 1u;

	public const uint Xbutton2 = 2u;

	public const uint MouseeventfMove = 1u;

	public const uint MouseeventfLeftdown = 2u;

	public const uint MouseeventfLeftup = 4u;

	public const uint MouseeventfRightdown = 8u;

	public const uint MouseeventfRightup = 16u;

	public const uint MouseeventfMiddledown = 32u;

	public const uint MouseeventfMiddleup = 64u;

	public const uint MouseeventfXdown = 128u;

	public const uint MouseeventfXup = 256u;

	public const uint MouseeventfWheel = 2048u;

	public const uint MouseeventfVirtualdesk = 16384u;

	public const uint MouseeventfAbsolute = 32768u;

	public const ushort HWND_BROADCAST = ushort.MaxValue;

	public const ushort SC_SIZE = 61440;

	public const ushort SC_CLOSE = 61536;

	public const ushort SC_MINIMIZE = 61472;

	public const ushort SC_MAXIMIZE = 61488;

	public const ushort SW_SHOWNORMAL = 1;

	public const ushort SW_RESTORE = 9;

	public const ushort SB_THUMBTRACK = 5;

	public const ushort SB_ENDSCROLL = 8;

	public const ushort SIF_TRACKPOS = 16;

	public const ushort SIF_RANGE = 1;

	public const ushort SIF_POS = 4;

	public const ushort SIF_PAGE = 2;

	public const ushort SIF_ALL = 23;

	public const ushort MF_UNCHECKED = 0;

	public const ushort MF_BY_COMMAND = 0;

	public const ushort MF_STRING = 0;

	public const ushort MF_GRAYED = 1;

	public const ushort MF_DISABLED = 2;

	public const ushort MF_CHECKED = 8;

	public const ushort MF_POPUP = 16;

	public const ushort MF_BAR_BREAK = 32;

	public const ushort MF_BREAK = 64;

	public const ushort MF_BY_POSITION = 1024;

	public const ushort MF_SEPARATOR = 2048;

	public const int MF_TOKEN_QUERY = 8;

	public const int MF_TOKEN_ADJUST_PRIVILEGES = 32;

	public const int MF_SE_PRIVILEGE_DISABLED = 0;

	public const int MF_SE_PRIVILEGE_ENABLED = 2;

	public const string MF_SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";

	public const string MF_SE_AUDIT_NAME = "SeAuditPrivilege";

	public const string MF_SE_BACKUP_NAME = "SeBackupPrivilege";

	public const string MF_SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";

	public const string MF_SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";

	public const string MF_SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";

	public const string MF_SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";

	public const string MF_SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";

	public const string MF_SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";

	public const string MF_SE_DEBUG_NAME = "SeDebugPrivilege";

	public const string MF_SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";

	public const string MF_SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";

	public const string MF_SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";

	public const string MF_SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";

	public const string MF_SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";

	public const string MF_SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";

	public const string MF_SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";

	public const string MF_SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";

	public const string MF_SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";

	public const string MF_SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";

	public const string MF_SE_RELABEL_NAME = "SeRelabelPrivilege";

	public const string MF_SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";

	public const string MF_SE_RESTORE_NAME = "SeRestorePrivilege";

	public const string MF_SE_SECURITY_NAME = "SeSecurityPrivilege";

	public const string MF_SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

	public const string MF_SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";

	public const string MF_SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";

	public const string MF_SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";

	public const string MF_SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";

	public const string MF_SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";

	public const string MF_SE_TCB_NAME = "SeTcbPrivilege";

	public const string MF_SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";

	public const string MF_SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";

	public const string MF_SE_UNDOCK_NAME = "SeUndockPrivilege";

	public const string MF_SE_UNSOLICITED_INPUT_NAME = "SeUnsolicitedInputPrivilege";

	public const ushort NIN_BALLOON_SHOW = 1026;

	public const ushort NIN_BALLOON_HIDE = 1027;

	public const ushort NIN_BALLOON_TIMEOUT = 1028;

	public const ushort NIN_BALLOON_USER_CLICK = 1029;

	public const ushort SE_PRIVILEGE_ENABLED = 2;

	public const ushort ERROR_NO_TOKEN = 1008;

	public const ushort RPC_S_INVALID_BINDING = 1702;

	public const string AllowSecondInstanceArgument = "AllowSecondInstance";

	public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

	private static string newText;

	public const int ABM_GETTASKBARPOS = 5;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIdNewItem, string lpNewItem);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool CheckMenuItem(IntPtr hMenu, int uIDCheckItem, int uCheck);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "InsertMenuW", ExactSpelling = true, SetLastError = true)]
	public static extern int InsertMenu(IntPtr hMenu, uint position, int uFlags, int uIdNewItem, string lpNewItem);

	[DllImport("user32.dll")]
	public static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

	[DllImport("user32.dll")]
	public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetWindowText(int hWnd, StringBuilder s, int nMaxCount);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern IntPtr FindWindow(string className, string windowName);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetParent(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern IntPtr GetActiveWindow();

	[DllImport("user32.dll")]
	public static extern IntPtr SetActiveWindow(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool SetForegroundWindow(HandleRef hWnd);

	[DllImport("user32.dll")]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", EntryPoint = "SendMessageA", SetLastError = true)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern int SendMessage(IntPtr hwnd, int msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);

	[DllImport("User32.dll")]
	public static extern int SendMessage(int hWnd, int msg, int wParam, string lParam);

	[DllImport("user32.dll")]
	public static extern IntPtr GetShellWindow();

	[DllImport("user32.dll")]
	public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr PostMessage(HandleRef hwnd, int msg, int wparam, int lparam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern int RegisterWindowMessage(string message);

	[DllImport("user32.dll")]
	public static extern IntPtr GetOpenClipboardWindow();

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	[DllImport("User32.dll")]
	public static extern bool EnumChildWindows(IntPtr hWndParent, Delegate lpEnumFunc, int lParam);

	[DllImport("User32")]
	public static extern int SetDlgItemText(IntPtr hwnd, int id, string title);

	[DllImport("user32.dll")]
	public static extern IntPtr GetMessageExtraInfo();

	[DllImport("user32.dll", SetLastError = true)]
	public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int GetScrollInfo(IntPtr hWnd, int n, ref ScrollInfoStruct lpScrollInfo);

	public static bool CheckRunningInstances(bool terminate, bool sendShowMe)
	{
		if (Environment.GetCommandLineArgs().Any((string a) => "AllowSecondInstance".Equals(a, StringComparison.OrdinalIgnoreCase)))
		{
			return false;
		}
		Process currentProcess = Process.GetCurrentProcess();
		int currentSessionID = Process.GetCurrentProcess().SessionId;
		Process[] array = (from p in Process.GetProcessesByName(currentProcess.ProcessName)
			where p.SessionId == currentSessionID && p.Id != currentProcess.Id
			select p).ToArray();
		if (array.Length != 0)
		{
			if (sendShowMe)
			{
				Process[] array2 = array;
				foreach (Process process in array2)
				{
					process.Refresh();
					IntPtr intPtr = FindWindow(null, Updater.ApplicationTitle);
					if (intPtr == IntPtr.Zero)
					{
						intPtr = process.MainWindowHandle;
					}
					if (intPtr != IntPtr.Zero)
					{
						ShowWindowAsync(intPtr, 9);
						ShowWindowAsync(intPtr, 1);
						SetForegroundWindow(intPtr);
						SendMessage(intPtr, WM_SHOWME, 0, IntPtr.Zero);
					}
				}
			}
			if (terminate)
			{
				Environment.Exit(0);
			}
			return true;
		}
		return false;
	}

	public static void SetDialogResult(string text)
	{
		newText = text;
		IntPtr intPtr = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "#32770", "Save the file as");
		if (intPtr != IntPtr.Zero)
		{
			EnumChildWindows(intPtr, new CallbackToFindChildWindow(EnumChildGetValue), 0);
		}
	}

	public static void AccessEditControlOfDialog(string text)
	{
		newText = text;
		IntPtr intPtr = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "#32770", "Save the file as");
		string windowName = string.Empty;
		StringBuilder stringBuilder = new StringBuilder(256);
		if (GetWindowText((int)intPtr, stringBuilder, 256) > 0)
		{
			windowName = stringBuilder.ToString();
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		GetClassName(intPtr, stringBuilder2, stringBuilder2.Capacity);
		CallbackToFindChildWindow lpEnumFunc = EnumChildGetValue;
		IntPtr intPtr2 = FindWindow(null, windowName);
		if (!(intPtr2 == IntPtr.Zero))
		{
			EnumChildWindows(intPtr2, lpEnumFunc, 0);
		}
	}

	private static int EnumChildGetValue(int handleWnd, int param)
	{
		StringBuilder stringBuilder = new StringBuilder();
		GetClassName((IntPtr)handleWnd, stringBuilder, stringBuilder.Capacity);
		if (!string.IsNullOrWhiteSpace(stringBuilder.ToString()) && (stringBuilder.ToString().Equals("Edit") || stringBuilder.ToString().Equals("RichEdit20W")))
		{
			if (stringBuilder.ToString().Equals("Edit"))
			{
				SendMessage(handleWnd, 12, newText.Length, newText);
			}
			else if (stringBuilder.ToString().ToLower().Equals("richedit20w"))
			{
				SendMessage(handleWnd, 12, 1, "");
				SetRichEditText((IntPtr)handleWnd, newText);
			}
		}
		return 0;
	}

	private static void SetRichEditText(IntPtr handleWnd, string text)
	{
		Settextex settextex = new Settextex
		{
			codepage = 1200L,
			flags = RtbwFlags.RtbwSelection
		};
		IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(settextex.GetType()));
		Marshal.StructureToPtr(settextex, ptr, fDeleteOld: true);
		IntPtr ptr2 = Marshal.StringToBSTR(text);
		SendMessage((int)handleWnd, 1121, ptr.ToInt32(), text);
		Marshal.FreeCoTaskMem(ptr);
		Marshal.FreeBSTR(ptr2);
	}

	public static void SendString(string s)
	{
		List<Input> list = new List<Input>();
		foreach (char wScan in s)
		{
			bool[] array = new bool[2] { false, true };
			foreach (bool flag in array)
			{
				Input item = new Input
				{
					type = 1,
					u = new InputUnion
					{
						ki = new Keybdinput
						{
							wVk = 0,
							wScan = wScan,
							dwFlags = (uint)(4 | (flag ? 2 : 0)),
							dwExtraInfo = GetMessageExtraInfo()
						}
					}
				};
				list.Add(item);
			}
		}
		SendInput((uint)list.Count, list.ToArray(), Marshal.SizeOf(typeof(Input)));
	}

	[DllImport("shell32.dll")]
	public static extern IntPtr SHAppBarMessage(int msg, ref APPBARDATA data);

	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

	public static Rectangle GetTaskbarPosition()
	{
		APPBARDATA data = default(APPBARDATA);
		data.cbSize = Marshal.SizeOf(data);
		if (SHAppBarMessage(5, ref data) == IntPtr.Zero)
		{
			throw new Win32Exception("Please re-install Windows");
		}
		return new Rectangle(data.rc.left, data.rc.top, data.rc.right - data.rc.left, data.rc.bottom - data.rc.top);
	}

	public static Color GetColorAt(Point location)
	{
		using Bitmap bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
		using Graphics graphics = Graphics.FromImage(bitmap);
		using (Graphics graphics2 = Graphics.FromHwnd(IntPtr.Zero))
		{
			IntPtr hdc = graphics2.GetHdc();
			BitBlt(graphics.GetHdc(), 0, 0, 1, 1, hdc, location.X, location.Y, 13369376);
			graphics.ReleaseHdc();
			graphics2.ReleaseHdc();
		}
		return bitmap.GetPixel(0, 0);
	}

	public static Color GetTaskbarColor()
	{
		return GetColorAt(GetTaskbarPosition().Location);
	}

	public static bool IsTaskbarDark()
	{
		try
		{
			Color colorAt = GetColorAt(GetTaskbarPosition().Location);
			return 0.299 * (double)(int)colorAt.R + 0.587 * (double)(int)colorAt.G + 0.114 * (double)(int)colorAt.B < 186.0;
		}
		catch (Exception)
		{
			return true;
		}
	}

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool Shell_NotifyIcon(NotifyIconMessage message, NotifyIconData pnid);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool TrackPopupMenuEx(HandleRef hmenu, int fuFlags, int x, int y, HandleRef hwnd, IntPtr tpm);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool GetCursorPos(ref POINT point);
}
