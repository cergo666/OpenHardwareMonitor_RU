using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace sergiye.Common;

public class PersistentSettings : ISettings
{
	private readonly string configFilePath;

	private IDictionary<string, string> settings = new Dictionary<string, string>();

	private bool isPortable;

	public bool WaitCommit { get; set; }

	public bool IsPortable
	{
		get
		{
			return isPortable;
		}
		set
		{
			if (isPortable != value)
			{
				isPortable = value;
				Save();
			}
		}
	}

	public PersistentSettings()
	{
		configFilePath = Path.ChangeExtension(Updater.CurrentFileLocation, ".config");
	}

	public void Load()
	{
		if (File.Exists(configFilePath))
		{
			try
			{
				IDictionary<string, string> dictionary = File.ReadAllText(configFilePath).FromJson<IDictionary<string, string>>();
				if (dictionary != null)
				{
					settings = dictionary;
					isPortable = true;
					return;
				}
			}
			catch (Exception)
			{
			}
		}
		using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(GetAppRegistryKey());
		if (registryKey != null)
		{
			string[] valueNames = registryKey.GetValueNames();
			foreach (string text in valueNames)
			{
				string value = registryKey.GetValue(text, null) as string;
				settings.Add(text, value);
			}
		}
	}

	public void Save(bool commit = false)
	{
		if (commit)
		{
			WaitCommit = false;
		}
		if (WaitCommit)
		{
			return;
		}
		Registry.CurrentUser.DeleteSubKeyTree(GetAppRegistryKey(), throwOnMissingSubKey: false);
		if (IsPortable)
		{
			SaveToFile(configFilePath);
			return;
		}
		try
		{
			using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(GetAppRegistryKey()))
			{
				if (registryKey == null)
				{
					return;
				}
				foreach (KeyValuePair<string, string> setting in settings)
				{
					if (setting.Value != null)
					{
						registryKey.SetValue(setting.Key, setting.Value);
					}
				}
			}
			if (File.Exists(configFilePath))
			{
				try
				{
					File.Delete(configFilePath);
					return;
				}
				catch (Exception)
				{
					return;
				}
			}
		}
		catch (Exception)
		{
			SaveToFile(configFilePath);
		}
	}

	public void SaveToFile(string configFilePath)
	{
		try
		{
			settings.ToJsonFile(configFilePath);
		}
		catch (UnauthorizedAccessException)
		{
			MessageBox.Show("Access to the path '" + configFilePath + "' is denied. The current settings could not be saved.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		catch (IOException)
		{
			MessageBox.Show("The path '" + configFilePath + "' is not writeable. The current settings could not be saved.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public bool Contains(string name)
	{
		return settings.ContainsKey(name);
	}

	public void SetValue(string name, string value)
	{
		if (!settings.TryGetValue(name, out var value2) || !(value2 == value))
		{
			settings[name] = value;
			Save();
		}
	}

	public string GetValue(string name, string value)
	{
		if (!settings.TryGetValue(name, out var value2))
		{
			return value;
		}
		return value2;
	}

	public void Remove(string name)
	{
		settings.Remove(name);
		Save();
	}

	public void SetValue(string name, int value)
	{
		if (!settings.TryGetValue(name, out var value2) || !int.TryParse(value2, out var result) || result != value)
		{
			settings[name] = value.ToString();
			Save();
		}
	}

	public int GetValue(string name, int value)
	{
		if (!settings.TryGetValue(name, out var value2) || !int.TryParse(value2, out var result))
		{
			return value;
		}
		return result;
	}

	public void SetValue(string name, float value)
	{
		if (!settings.TryGetValue(name, out var value2) || !float.TryParse(value2, out var result) || result != value)
		{
			settings[name] = value.ToString(CultureInfo.InvariantCulture);
			Save();
		}
	}

	public void SetValue(string name, double value)
	{
		if (!settings.TryGetValue(name, out var value2) || !double.TryParse(value2, out var result) || result != value)
		{
			settings[name] = value.ToString(CultureInfo.InvariantCulture);
			Save();
		}
	}

	public float GetValue(string name, float value)
	{
		if (!settings.TryGetValue(name, out var value2) || !float.TryParse(value2, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return value;
		}
		return result;
	}

	public double GetValue(string name, double value)
	{
		if (!settings.TryGetValue(name, out var value2) || !double.TryParse(value2, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return value;
		}
		return result;
	}

	public void SetValue(string name, bool value)
	{
		if (!settings.TryGetValue(name, out var value2) || !bool.TryParse(value2, out var result) || result != value)
		{
			settings[name] = (value ? "true" : "false");
			Save();
		}
	}

	public bool GetValue(string name, bool value)
	{
		if (!settings.TryGetValue(name, out var value2))
		{
			return value;
		}
		return value2 == "true";
	}

	public void SetValue(string name, Color color)
	{
		if (!settings.TryGetValue(name, out var value) || !int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result) || !(Color.FromArgb(result) == color))
		{
			settings[name] = color.ToArgb().ToString("X8");
			Save();
		}
	}

	public Color GetValue(string name, Color value)
	{
		if (!settings.TryGetValue(name, out var value2) || !int.TryParse(value2, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
		{
			return value;
		}
		return Color.FromArgb(result);
	}

	private string GetAppRegistryKey()
	{
		return "Software\\sergiye\\" + Updater.ApplicationName;
	}
}
