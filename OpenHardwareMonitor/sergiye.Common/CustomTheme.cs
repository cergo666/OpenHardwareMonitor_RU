using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace sergiye.Common;

public class CustomTheme : Theme
{
	public class ThemeDto
	{
		public string DisplayName { get; set; }

		public string ForegroundColor { get; set; }

		public string BackgroundColor { get; set; }

		public string HyperlinkColor { get; set; }

		public string SelectedForegroundColor { get; set; }

		public string SelectedBackgroundColor { get; set; }

		public string LineColor { get; set; }

		public string StrongLineColor { get; set; }

		public bool DarkMode { get; set; }

		public string DisabledColor { get; set; }

		public string MessageColor { get; set; }

		public string InfoColor { get; set; }

		public string WarnColor { get; set; }
	}

	private CustomTheme(string id, ThemeDto theme)
		: base(id, theme.DisplayName)
	{
		ForegroundColor = ColorTranslator.FromHtml(theme.ForegroundColor);
		BackgroundColor = ColorTranslator.FromHtml(theme.BackgroundColor);
		HyperlinkColor = ColorTranslator.FromHtml(theme.HyperlinkColor);
		SelectedForegroundColor = ColorTranslator.FromHtml(theme.SelectedForegroundColor);
		SelectedBackgroundColor = ColorTranslator.FromHtml(theme.SelectedBackgroundColor);
		LineColor = ColorTranslator.FromHtml(theme.LineColor);
		StrongLineColor = ColorTranslator.FromHtml(theme.StrongLineColor);
		WindowTitleBarFallbackToImmersiveDarkMode = theme.DarkMode;
		DisabledColor = ((theme.DisabledColor != null) ? ColorTranslator.FromHtml(theme.DisabledColor) : (theme.DarkMode ? DarkTheme.DefaultDisabledColor : LightTheme.DefaultDisabledColor));
		MessageColor = ((theme.MessageColor != null) ? ColorTranslator.FromHtml(theme.MessageColor) : (theme.DarkMode ? DarkTheme.DefaultMessageColor : LightTheme.DefaultMessageColor));
		InfoColor = ((theme.InfoColor != null) ? ColorTranslator.FromHtml(theme.InfoColor) : (theme.DarkMode ? DarkTheme.DefaultInfoColor : LightTheme.DefaultInfoColor));
		WarnColor = ((theme.WarnColor != null) ? ColorTranslator.FromHtml(theme.WarnColor) : (theme.DarkMode ? DarkTheme.DefaultWarnColor : LightTheme.DefaultWarnColor));
	}

	public static IEnumerable<Theme> GetAllThemes(string themesFolder = "themes", string resourcesPath = null)
	{
		yield return new LightTheme();
		yield return new DarkTheme();
		Assembly assembly = Assembly.GetEntryAssembly();
		if (!string.IsNullOrEmpty(resourcesPath))
		{
			foreach (string item in from n in assembly.GetManifestResourceNames()
				where n.StartsWith(resourcesPath) && n.EndsWith(".json")
				select n)
			{
				using Stream stream = assembly.GetManifestResourceStream(item);
				using (StreamReader reader = new StreamReader(stream))
				{
					ThemeDto themeDto;
					try
					{
						themeDto = reader.ReadToEnd().FromJson<ThemeDto>();
					}
					catch (Exception)
					{
						goto end_IL_013d;
					}
					yield return new CustomTheme(themeDto.DisplayName, themeDto);
					goto end_IL_0124;
					end_IL_013d:;
				}
				end_IL_0124:;
			}
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Updater.CurrentFileLocation), themesFolder));
		if (!directoryInfo.Exists)
		{
			yield break;
		}
		FileInfo[] files = directoryInfo.GetFiles("*.json", SearchOption.TopDirectoryOnly);
		foreach (FileInfo fileInfo in files)
		{
			ThemeDto themeDto2 = null;
			try
			{
				themeDto2 = File.ReadAllText(fileInfo.FullName).FromJson<ThemeDto>();
				if (string.IsNullOrEmpty(themeDto2.DisplayName))
				{
					themeDto2.DisplayName = Path.GetFileNameWithoutExtension(fileInfo.Name);
				}
			}
			catch (Exception)
			{
			}
			if (themeDto2 != null)
			{
				yield return new CustomTheme(fileInfo.Name, themeDto2);
			}
		}
	}

	public static T FillThemesMenu<T>(Func<string, Theme, EventHandler, T> getMenuItem, Action onThemeChanged, string initThemeId = null, string resourcesPath = null, string themesFolder = "themes") where T : class
	{
		T currentMenuItem = null;
		Theme.OnCurrentChanged = (Action)Delegate.Remove(Theme.OnCurrentChanged, onThemeChanged);
		onThemeChanged?.Invoke();
		Theme.OnCurrentChanged = (Action)Delegate.Combine(Theme.OnCurrentChanged, onThemeChanged);
		if (Theme.SupportsAutoThemeSwitching())
		{
			T val = getMenuItem("Auto", null, delegate
			{
				Theme.SetAutoTheme();
			});
			if (string.IsNullOrEmpty(initThemeId) || initThemeId == "auto")
			{
				currentMenuItem = val;
			}
		}
		List<Theme> source = (from t in GetAllThemes(themesFolder, resourcesPath)
			orderby !t.WindowTitleBarFallbackToImmersiveDarkMode, t is CustomTheme, t.DisplayName
			select t).ToList();
		getMenuItem("-", null, null);
		List<Theme> list = source.Where((Theme t) => !t.WindowTitleBarFallbackToImmersiveDarkMode).ToList();
		AddThemeMenuItems(list);
		if (list.Count > 0)
		{
			getMenuItem("-", null, null);
		}
		AddThemeMenuItems(source.Where((Theme t) => t.WindowTitleBarFallbackToImmersiveDarkMode));
		return currentMenuItem;
		void AddThemeMenuItems(IEnumerable<Theme> themes)
		{
			foreach (Theme theme in themes)
			{
				T val2 = getMenuItem(theme.DisplayName, theme, delegate
				{
					Theme.Current = theme;
				});
				if (initThemeId == theme.Id)
				{
					currentMenuItem = val2;
				}
			}
		}
	}
}
