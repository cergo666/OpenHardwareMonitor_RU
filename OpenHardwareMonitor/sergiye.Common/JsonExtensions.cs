using System.IO;
using System.Web.Script.Serialization;

namespace sergiye.Common;

public static class JsonExtensions
{
	public static string ToJson(this object value)
	{
		return new JavaScriptSerializer().Serialize(value);
	}

	public static T FromJson<T>(this string json)
	{
		return new JavaScriptSerializer().Deserialize<T>(json);
	}

	public static void ToJsonFile(this object value, string filePath)
	{
		File.WriteAllText(filePath, value.ToJson());
	}

	public static T ReadJsonFile<T>(string fileName) where T : class
	{
		T result = null;
		if (File.Exists(fileName))
		{
			return File.ReadAllText(fileName).FromJson<T>();
		}
		return result;
	}
}
