using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace sergiye.Common;

public static class Updater
{
	public class GitHubRelease
	{
		public string Tag_name { get; set; }

		public string Name { get; set; }

		public bool Prerelease { get; set; }

		public Asset[] Assets { get; set; }
	}

	public class Asset
	{
		public string Name { get; set; }

		public string Browser_download_url { get; set; }
	}

	public enum CheckUpdatesMode
	{
		AllMessages,
		NotifyOnNewVersion,
		AutoUpdate
	}

	public static readonly string ApplicationName;

	public static readonly string ApplicationTitle;

	public static readonly string ApplicationCompany;

	public static readonly string SelfFileName;

	public static readonly string CurrentVersion;

	public static readonly string CurrentFileLocation;

	public static bool AutoUpdate;

	private static Timer timer;

	public static event Action<string, bool> OnMessage;

	public static event Func<string, bool> OnQuestion;

	public static event Action OnExit;

	[DllImport("shlwapi.dll", CharSet = CharSet.Unicode, EntryPoint = "StrCmpLogicalW", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern int StrCmpLogical(string psz1, string psz2);

	static Updater()
	{
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		ApplicationName = GetAttribute<AssemblyProductAttribute>(entryAssembly)?.Product;
		ApplicationTitle = GetAttribute<AssemblyTitleAttribute>(entryAssembly)?.Title;
		ApplicationCompany = GetAttribute<AssemblyCompanyAttribute>(entryAssembly)?.Company;
		CurrentVersion = entryAssembly?.GetName().Version.ToString(3);
		CurrentFileLocation = entryAssembly?.Location;
		SelfFileName = Path.GetFileName(CurrentFileLocation);
		ServicePointManager.Expect100Continue = false;
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object _, X509Certificate _, X509Chain _, SslPolicyErrors _) => true));
	}

	public static void Subscribe(Action<string, bool> onMessage, Func<string, bool> onQuestion, Action onExit = null, bool autoUpdate = true)
	{
		if (onMessage != null)
		{
			OnMessage += onMessage;
		}
		if (onQuestion != null)
		{
			OnQuestion += onQuestion;
		}
		if (onExit != null)
		{
			OnExit += onExit;
		}
		AutoUpdate = autoUpdate;
		timer = new Timer(delegate
		{
			CheckForUpdates((!AutoUpdate) ? CheckUpdatesMode.NotifyOnNewVersion : CheckUpdatesMode.AutoUpdate);
		}, null, 10000, 86400000);
	}

	private static T GetAttribute<T>(ICustomAttributeProvider assembly, bool inherit = false) where T : Attribute
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(T), inherit);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	private static string GetAppSiteUrl(string subPage = null)
	{
		string text = "https://github.com/" + ApplicationCompany + "/" + ApplicationName;
		if (!string.IsNullOrEmpty(subPage))
		{
			text = text + "/" + subPage;
		}
		return text;
	}

	private static string GetAppReleasesUrl()
	{
		return "https://api.github.com/repos/" + ApplicationCompany + "/" + ApplicationName + "/releases";
	}

	public static bool CheckForUpdates(CheckUpdatesMode mode)
	{
		try
		{
			string jsonString;
			using (WebClient webClient = new WebClient())
			{
				webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.500.27 Safari/537.36");
				jsonString = webClient.DownloadString(GetAppReleasesUrl());
			}
			return CheckForUpdatesInternal(mode, jsonString);
		}
		catch (Exception ex)
		{
			if (mode == CheckUpdatesMode.AllMessages)
			{
				Updater.OnMessage?.Invoke("Error checking for a new version.\n" + ex.Message, arg2: true);
			}
			return false;
		}
	}

	public static async Task<bool> CheckForUpdatesAsync(CheckUpdatesMode mode)
	{
		try
		{
			string jsonString;
			using (WebClient wc = new WebClient())
			{
				wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.500.27 Safari/537.36");
				jsonString = await wc.DownloadStringTaskAsync(GetAppReleasesUrl()).ConfigureAwait(continueOnCapturedContext: false);
			}
			return CheckForUpdatesInternal(mode, jsonString);
		}
		catch (Exception ex)
		{
			if (mode == CheckUpdatesMode.AllMessages)
			{
				Updater.OnMessage?.Invoke("Error checking for a new version.\n" + ex.Message, arg2: true);
			}
			return false;
		}
	}

	private static bool CheckForUpdatesInternal(CheckUpdatesMode mode, string jsonString)
	{
		GitHubRelease[] array = jsonString.FromJson<GitHubRelease[]>();
		if (array == null || array.Length == 0)
		{
			throw new Exception("Error getting list of releases.");
		}
		GitHubRelease gitHubRelease = array.FirstOrDefault((GitHubRelease r) => !r.Prerelease) ?? array[0];
		string tag_name = gitHubRelease.Tag_name;
		Asset asset = gitHubRelease.Assets.FirstOrDefault((Asset a) => SelfFileName.Equals(a.Name, StringComparison.OrdinalIgnoreCase));
		if (asset == null)
		{
			asset = gitHubRelease.Assets.FirstOrDefault((Asset a) => a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
		}
		string text = asset?.Browser_download_url;
		if (string.IsNullOrEmpty(text))
		{
			if (mode == CheckUpdatesMode.AllMessages)
			{
				Updater.OnMessage?.Invoke("Your version is: " + CurrentVersion + "\nLatest released version is: " + tag_name + "\nNo assets found to update.", arg2: false);
			}
			return true;
		}
		if (StrCmpLogical(CurrentVersion, tag_name) >= 0)
		{
			if (mode == CheckUpdatesMode.AllMessages)
			{
				Updater.OnMessage?.Invoke("Your version: " + CurrentVersion + "\nLast release: " + tag_name + "\nNo need to update.", arg2: false);
			}
			return true;
		}
		if (mode != CheckUpdatesMode.AutoUpdate)
		{
			if (Updater.OnQuestion == null)
			{
				Updater.OnMessage?.Invoke("New version found: " + tag_name + ", app will be updated after close.", arg2: false);
			}
			else if (!Updater.OnQuestion("Your version: " + CurrentVersion + "\nLast release: " + tag_name + "\nDownload this update?"))
			{
				return true;
			}
		}
		try
		{
			string text2 = Path.GetTempPath() + SelfFileName;
			using (WebClient webClient = new WebClient())
			{
				webClient.DownloadFile(text, text2);
			}
			RestartApp(3, text2);
			return true;
		}
		catch (Exception ex)
		{
			if (mode == CheckUpdatesMode.AllMessages)
			{
				Updater.OnMessage?.Invoke("Error downloading new version\n" + ex.Message, arg2: true);
			}
			return false;
		}
	}

	public static void RestartApp(int timeout = 0, string replaceWithFile = null)
	{
		string text = Path.GetTempPath() + SelfFileName + "_updater.cmd";
		int id = Process.GetCurrentProcess().Id;
		using (StreamWriter streamWriter = new StreamWriter(File.Create(text)))
		{
			streamWriter.WriteLine("@ECHO OFF");
			streamWriter.WriteLine(":Loop");
			streamWriter.WriteLine("TASKLIST /fi \"PID eq " + id + "\" | find \":\"");
			streamWriter.WriteLine("if Errorlevel 1 (");
			streamWriter.WriteLine("  TIMEOUT /t 1 /nobreak");
			streamWriter.WriteLine("  Goto Loop");
			streamWriter.WriteLine(")");
			if (timeout > 0)
			{
				streamWriter.WriteLine($"TIMEOUT /t {timeout} /nobreak > NUL");
			}
			streamWriter.WriteLine("TASKKILL /F /PID \"{0}\" > NUL", id);
			streamWriter.WriteLine("TASKKILL /IM \"{0}\" > NUL", SelfFileName);
			if (!string.IsNullOrEmpty(replaceWithFile))
			{
				streamWriter.WriteLine("MOVE \"{0}\" \"{1}\"", replaceWithFile, CurrentFileLocation);
			}
			streamWriter.WriteLine("DEL \"%~f0\" & START \"\" /B \"{0}\"", CurrentFileLocation);
		}
		Process.Start(new ProcessStartInfo(text)
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			WorkingDirectory = Path.GetTempPath()
		});
		Updater.OnExit?.Invoke();
		Environment.Exit(0);
	}

	public static void VisitAppSite(string subPage = null)
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = GetAppSiteUrl(subPage),
			UseShellExecute = true
		});
	}

	public static void ShowAbout()
	{
		Updater.OnMessage?.Invoke(ApplicationTitle + " " + CurrentVersion + " " + (Environment.Is64BitProcess ? "x64" : "x86") + "\nWritten by Sergiy Egoshyn (egoshin.sergey@gmail.com)", arg2: false);
	}
}
