using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32;

namespace sergiye.Common;

public class StartupManager
{
	private readonly TaskSchedulerClass scheduler;

	private bool startup;

	private const string REGISTRY_RUN = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";

	public bool IsAvailable { get; }

	public bool Startup
	{
		get
		{
			return startup;
		}
		set
		{
			if (startup == value)
			{
				return;
			}
			if (!IsAvailable)
			{
				throw new InvalidOperationException();
			}
			if (scheduler != null)
			{
				DeleteRegistryRun();
				if (value)
				{
					CreateSchedulerTask();
				}
				else
				{
					DeleteSchedulerTask();
				}
				startup = value;
			}
			else
			{
				if (value)
				{
					CreateRegistryRun();
				}
				else
				{
					DeleteRegistryRun();
				}
				startup = value;
			}
		}
	}

	public StartupManager()
	{
		if (OSHelper.IsUnix)
		{
			scheduler = null;
			IsAvailable = false;
			return;
		}
		if (OSHelper.IsAdministrator())
		{
			try
			{
				scheduler = new TaskSchedulerClass();
				scheduler.Connect(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
			}
			catch
			{
				scheduler = null;
			}
			if (scheduler != null)
			{
				try
				{
					try
					{
						scheduler.GetRunningTasks(0);
					}
					catch (ArgumentException)
					{
					}
					IRegisteredTask task = scheduler.GetFolder("\\" + Updater.ApplicationTitle).GetTask(GetTaskNameByUser());
					startup = task != null && task.Definition.Triggers.Count > 0 && task.Definition.Triggers[1].Type == TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON && task.Definition.Actions.Count > 0 && task.Definition.Actions[1].Type == TASK_ACTION_TYPE.TASK_ACTION_EXEC && task.Definition.Actions[1] is IExecAction execAction && execAction.Path == Updater.CurrentFileLocation;
				}
				catch (IOException)
				{
					startup = false;
				}
				catch (UnauthorizedAccessException)
				{
					scheduler = null;
				}
				catch (COMException)
				{
					scheduler = null;
				}
				catch (NotImplementedException)
				{
					scheduler = null;
				}
			}
		}
		else
		{
			scheduler = null;
		}
		if (scheduler == null)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run"))
				{
					startup = false;
					string text = (string)registryKey?.GetValue(Updater.ApplicationName);
					if (text != null)
					{
						startup = text == Updater.CurrentFileLocation;
					}
				}
				IsAvailable = true;
				return;
			}
			catch (SecurityException)
			{
				IsAvailable = false;
				return;
			}
		}
		IsAvailable = true;
	}

	private void CreateSchedulerTask()
	{
		ITaskDefinition taskDefinition = scheduler.NewTask(0u);
		taskDefinition.RegistrationInfo.Description = "This task starts the " + Updater.ApplicationTitle + " on Windows startup.";
		taskDefinition.Principal.RunLevel = TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
		taskDefinition.Settings.DisallowStartIfOnBatteries = false;
		taskDefinition.Settings.StopIfGoingOnBatteries = false;
		taskDefinition.Settings.ExecutionTimeLimit = "PT0S";
		ILogonTrigger obj = (ILogonTrigger)taskDefinition.Triggers.Create(TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
		obj.UserId = WindowsIdentity.GetCurrent().Name;
		obj.Delay = "PT5S";
		IExecAction obj2 = (IExecAction)taskDefinition.Actions.Create(TASK_ACTION_TYPE.TASK_ACTION_EXEC);
		obj2.Path = Updater.CurrentFileLocation;
		obj2.WorkingDirectory = Path.GetDirectoryName(Updater.CurrentFileLocation);
		ITaskFolder folder = scheduler.GetFolder("\\");
		ITaskFolder taskFolder;
		try
		{
			taskFolder = folder.GetFolder(Updater.ApplicationTitle);
		}
		catch (IOException)
		{
			taskFolder = folder.CreateFolder(Updater.ApplicationTitle, "");
		}
		taskFolder.RegisterTaskDefinition(GetTaskNameByUser(), taskDefinition, 6, null, null, TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN, "");
	}

	private void DeleteSchedulerTask()
	{
		ITaskFolder folder = scheduler.GetFolder("\\");
		try
		{
			folder.GetFolder(Updater.ApplicationTitle).DeleteTask(GetTaskNameByUser(), 0);
		}
		catch (Exception)
		{
		}
		try
		{
			folder.DeleteFolder(Updater.ApplicationTitle, 0);
		}
		catch (Exception)
		{
		}
	}

	private static string GetTaskNameByUser()
	{
		return "Startup for " + WindowsIdentity.GetCurrent().Name.Replace("\\", "_");
	}

	private void CreateRegistryRun()
	{
		using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
		registryKey?.SetValue(Updater.ApplicationName, Updater.CurrentFileLocation);
	}

	private void DeleteRegistryRun()
	{
		using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
		registryKey?.DeleteValue(Updater.ApplicationName, throwOnMissingValue: false);
	}
}
