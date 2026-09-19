using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace sergiye.Common;

public static class Crasher
{
	[Flags]
	private enum ErrorModes : uint
	{
		SYSTEM_DEFAULT = 0u,
		SEM_FAILCRITICALERRORS = 1u,
		SEM_NO_ALIGNMENT_FAULT_EXCEPT = 4u,
		SEM_NOGPFAULT_ERROR_BOX = 2u,
		SEM_NO_OPEN_FILE_ERROR_BOX = 0x8000u
	}

	public static event EventHandler SaveState;

	public static void Listen(bool disableCrashDialog = true)
	{
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		if (disableCrashDialog)
		{
			SetErrorMode(SetErrorMode(ErrorModes.SEM_NOGPFAULT_ERROR_BOX) | ErrorModes.SEM_NOGPFAULT_ERROR_BOX);
		}
	}

	private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
		{
			string contents = ex.TraceException();
			File.WriteAllText(Path.Combine(Path.GetDirectoryName(Updater.CurrentFileLocation), "Crash_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss")), contents);
		}
		Crasher.SaveState?.Invoke(null, EventArgs.Empty);
		Environment.Exit(-1);
	}

	[DllImport("kernel32.dll")]
	private static extern ErrorModes SetErrorMode(ErrorModes uMode);

	private static string TraceException(this Exception ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (ex != null)
		{
			stringBuilder.AppendLine(ex.Message);
			stringBuilder.AppendLine(ex.GetType().Name);
			stringBuilder.AppendLine(ex.StackTrace);
			ex = ex.InnerException;
		}
		return stringBuilder.ToString();
	}
}
