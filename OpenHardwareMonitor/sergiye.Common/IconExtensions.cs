using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace sergiye.Common;

public static class IconExtensions
{
	public static void Destroy(this Icon icon)
	{
		IconFactory.DestroyIcon(icon.Handle);
		icon.Dispose();
	}

	public static string ToTrayValue(this float value)
	{
		return ((double)value).ToTrayValue();
	}

	public static string ToTrayValue(this double value)
	{
		double num = Math.Round(value, 1);
		if (!(num < 10.0))
		{
			return Math.Round(value).ToString("0");
		}
		return num.ToString("0.0");
	}

	public static Bitmap GetLargestBitmap(this Icon icon)
	{
		using MemoryStream memoryStream = new MemoryStream();
		try
		{
			icon.Save(memoryStream);
			byte[] array = memoryStream.ToArray();
			int num = BitConverter.ToUInt16(array, 4);
			if (num <= 0)
			{
				throw new InvalidOperationException("ICO contains no images.");
			}
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				int num4 = 6 + i * 16;
				int num5 = ((array[num4] == 0) ? 256 : array[num4]);
				int num6 = BitConverter.ToInt32(array, num4 + 8);
				if (num5 > num3 || (num5 == num3 && num6 > 0))
				{
					num3 = num5;
					num2 = i;
				}
			}
			int num7 = 6 + num2 * 16;
			int num8 = BitConverter.ToInt32(array, num7 + 8);
			int num9 = BitConverter.ToInt32(array, num7 + 12);
			if (num9 <= 0 || num9 + num8 > array.Length)
			{
				throw new InvalidOperationException("Invalid ICO entry offsets.");
			}
			MemoryStream imgStream = new MemoryStream(array, num9, num8, writable: false);
			try
			{
				byte[] array2 = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };
				bool flag = true;
				if (num8 < array2.Length)
				{
					flag = false;
				}
				else
				{
					if (array2.Any((byte t) => imgStream.ReadByte() != t))
					{
						flag = false;
					}
					imgStream.Position = 0L;
				}
				if (flag)
				{
					using (Image original = Image.FromStream(imgStream))
					{
						return new Bitmap(original);
					}
				}
				using MemoryStream memoryStream2 = new MemoryStream();
				using BinaryWriter binaryWriter = new BinaryWriter(memoryStream2);
				binaryWriter.Write((ushort)0);
				binaryWriter.Write((ushort)1);
				binaryWriter.Write((ushort)1);
				byte value = array[num7];
				byte value2 = array[num7 + 1];
				binaryWriter.Write(value);
				binaryWriter.Write(value2);
				binaryWriter.Write(array[num7 + 2]);
				binaryWriter.Write(array[num7 + 3]);
				binaryWriter.Write(BitConverter.ToUInt16(array, num7 + 4));
				binaryWriter.Write(BitConverter.ToUInt16(array, num7 + 6));
				binaryWriter.Write(num8);
				int value3 = 22;
				binaryWriter.Write(value3);
				binaryWriter.Write(array, num9, num8);
				binaryWriter.Flush();
				memoryStream2.Position = 0L;
				using Icon icon2 = new Icon(memoryStream2);
				return icon2.ToBitmap();
			}
			finally
			{
				if (imgStream != null)
				{
					((IDisposable)imgStream).Dispose();
				}
			}
		}
		catch (Exception)
		{
			return icon.ToBitmap();
		}
	}
}
