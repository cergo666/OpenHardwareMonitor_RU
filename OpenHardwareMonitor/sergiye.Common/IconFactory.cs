using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace sergiye.Common;

public class IconFactory : IDisposable
{
	public struct BITMAP_INFO_HEADER
	{
		public readonly uint Size;

		public readonly int Width;

		public readonly int Height;

		public readonly ushort Planes;

		public readonly ushort BitCount;

		public readonly uint Compression;

		public readonly uint SizeImage;

		public readonly int XPelsPerMeter;

		public readonly int YPelsPerMeter;

		public readonly uint ClrUsed;

		public readonly uint ClrImportant;

		public BITMAP_INFO_HEADER(int width, int height, int bitCount)
		{
			Size = 40u;
			Width = width;
			Height = height;
			Planes = 1;
			BitCount = (ushort)bitCount;
			Compression = 0u;
			SizeImage = 0u;
			XPelsPerMeter = 0;
			YPelsPerMeter = 0;
			ClrUsed = 0u;
			ClrImportant = 0u;
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write(Size);
			bw.Write(Width);
			bw.Write(Height);
			bw.Write(Planes);
			bw.Write(BitCount);
			bw.Write(Compression);
			bw.Write(SizeImage);
			bw.Write(XPelsPerMeter);
			bw.Write(YPelsPerMeter);
			bw.Write(ClrUsed);
			bw.Write(ClrImportant);
		}
	}

	public struct ICON_IMAGE
	{
		public BITMAP_INFO_HEADER Header;

		public readonly byte[] Colors;

		public readonly int MaskSize;

		public ICON_IMAGE(int width, int height, byte[] colors)
		{
			Header = new BITMAP_INFO_HEADER(width, height << 1, 8 * colors.Length / (width * height));
			Colors = colors;
			MaskSize = width * height >> 3;
		}

		public void Write(BinaryWriter bw)
		{
			Header.Write(bw);
			int num = Header.Width << 2;
			for (int num2 = (Header.Height >> 1) - 1; num2 >= 0; num2--)
			{
				bw.Write(Colors, num2 * num, num);
			}
			for (int i = 0; i < 2 * MaskSize; i++)
			{
				bw.Write((byte)0);
			}
		}
	}

	public struct ICON_DIR_ENTRY
	{
		public readonly byte Width;

		public readonly byte Height;

		public readonly byte ColorCount;

		public readonly byte Reserved;

		public readonly ushort Planes;

		public readonly ushort BitCount;

		public readonly uint BytesInRes;

		public uint ImageOffset;

		public uint Size => 16u;

		public ICON_DIR_ENTRY(ICON_IMAGE image, int imageOffset)
		{
			Width = (byte)image.Header.Width;
			Height = (byte)(image.Header.Height >> 1);
			ColorCount = 0;
			Reserved = 0;
			Planes = image.Header.Planes;
			BitCount = image.Header.BitCount;
			BytesInRes = (uint)(image.Header.Size + image.Colors.Length + image.MaskSize + image.MaskSize);
			ImageOffset = (uint)imageOffset;
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write(Width);
			bw.Write(Height);
			bw.Write(ColorCount);
			bw.Write(Reserved);
			bw.Write(Planes);
			bw.Write(BitCount);
			bw.Write(BytesInRes);
			bw.Write(ImageOffset);
		}
	}

	public struct ICON_DIR
	{
		public readonly ushort Reserved;

		public readonly ushort Type;

		public readonly ushort Count;

		public readonly ICON_DIR_ENTRY[] Entries;

		public uint Size => (uint)(6 + Entries.Length * ((Entries.Length != 0) ? Entries[0].Size : 0));

		public ICON_DIR(ICON_DIR_ENTRY[] entries)
		{
			Reserved = 0;
			Type = 1;
			Count = (ushort)entries.Length;
			Entries = entries;
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write(Reserved);
			bw.Write(Type);
			bw.Write(Count);
			for (int i = 0; i < Entries.Length; i++)
			{
				Entries[i].Write(bw);
			}
		}
	}

	private readonly Bitmap bitmap;

	private readonly Graphics graphics;

	private readonly Font font;

	private readonly Font smallFont;

	private Color color;

	private Color darkColor;

	private Brush brush;

	private Brush darkBrush;

	private Pen pen;

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			darkColor = Color.FromArgb(255, color.R / 3, color.G / 3, color.B / 3);
			Brush obj = brush;
			brush = new SolidBrush(color);
			obj?.Dispose();
			Brush obj2 = darkBrush;
			darkBrush = new SolidBrush(darkColor);
			obj2?.Dispose();
			Pen obj3 = pen;
			pen = new Pen(Color.FromArgb(96, color), 1f);
			obj3?.Dispose();
		}
	}

	[DllImport("user32", SetLastError = true)]
	public static extern bool DestroyIcon(IntPtr handle);

	public IconFactory(Color? color = null)
	{
		if (color.HasValue)
		{
			Color = color.Value;
		}
		else
		{
			Color = (WinApiHelper.IsTaskbarDark() ? Color.Cyan : Color.Teal);
		}
		float horizontalResolution;
		using (Bitmap bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
		{
			horizontalResolution = bitmap.HorizontalResolution;
		}
		float num = horizontalResolution / 96f;
		int num2 = Math.Max(16, (int)Math.Round(16f * num));
		int height = Math.Max(16, (int)Math.Round(16f * num));
		string familyName = (IsFontInstalled("Arial", 15f) ? "Arial" : (IsFontInstalled("Segoe UI", 15f) ? "Segoe UI" : SystemFonts.MessageBoxFont.Name));
		float emSize;
		float emSize2;
		switch (num2)
		{
		case 16:
			emSize = 10f;
			emSize2 = 8f;
			break;
		case 20:
			emSize = 11f;
			emSize2 = 8f;
			break;
		case 28:
			emSize = 12f;
			emSize2 = 9f;
			break;
		default:
			emSize = 8f * num;
			emSize2 = 6f * num;
			break;
		}
		font = new Font(familyName, emSize);
		smallFont = new Font(familyName, emSize2);
		this.bitmap = new Bitmap(num2, height, PixelFormat.Format32bppArgb);
		graphics = Graphics.FromImage(this.bitmap);
		if (OSHelper.IsWindows7OrLower)
		{
			graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		}
	}

	public void Dispose()
	{
		graphics.Dispose();
		bitmap.Dispose();
		font.Dispose();
		smallFont.Dispose();
		brush?.Dispose();
		darkBrush?.Dispose();
		pen.Dispose();
	}

	public static Icon Create(byte[] colors, int width, int height, PixelFormat format)
	{
		if (format != PixelFormat.Format32bppArgb)
		{
			throw new NotImplementedException();
		}
		ICON_IMAGE image = new ICON_IMAGE(width, height, colors);
		ICON_DIR iCON_DIR = new ICON_DIR(new ICON_DIR_ENTRY[1]
		{
			new ICON_DIR_ENTRY(image, 0)
		});
		iCON_DIR.Entries[0].ImageOffset = iCON_DIR.Size;
		using BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream());
		binaryWriter.BaseStream.Position = 0L;
		iCON_DIR.Write(binaryWriter);
		image.Write(binaryWriter);
		binaryWriter.BaseStream.Position = 0L;
		return new Icon(binaryWriter.BaseStream);
	}

	public static Icon Create(Bitmap bitmap)
	{
		return Icon.FromHandle(bitmap.GetHicon());
	}

	public Icon CreatePercentageIcon(float value)
	{
		try
		{
			graphics.Clear(Color.Transparent);
		}
		catch (Exception)
		{
			graphics.Clear(Color.Black);
		}
		graphics.FillRectangle(darkBrush, 0.5f, -0.5f, bitmap.Width - 2, bitmap.Height);
		float num = (float)(bitmap.Height - 4) * (value / 100f);
		if (value > 0f && num < 1f)
		{
			num = 1f;
		}
		float y = (float)(bitmap.Height - 2) - num;
		graphics.FillRectangle(brush, 2f, y, bitmap.Width - 5, num);
		graphics.DrawRectangle(pen, 1, 1, bitmap.Width - 3, bitmap.Height - 2);
		return Create(bitmap);
	}

	public Icon CreatePercentagePieIcon(byte value)
	{
		try
		{
			graphics.Clear(Color.Transparent);
		}
		catch (Exception)
		{
			graphics.Clear(Color.Black);
		}
		float num = 360f * (float)(int)value / 100f;
		if (num > 0f)
		{
			using SolidBrush solidBrush = new SolidBrush(color);
			graphics.FillPie(solidBrush, 4f, 4f, bitmap.Width - 8, bitmap.Height - 8, -90f, num);
		}
		using (Pen pen = new Pen(color, 1f))
		{
			graphics.DrawEllipse(pen, 1, 1, bitmap.Width - 2, bitmap.Height - 2);
		}
		return Create(bitmap);
	}

	public Icon CreateTransparentIcon(string text)
	{
		Color color = Color.Transparent;
		try
		{
			graphics.Clear(color);
		}
		catch (Exception)
		{
			try
			{
				color = WinApiHelper.GetTaskbarColor();
			}
			catch (Exception)
			{
				color = Color.Black;
			}
			graphics.Clear(color);
		}
		if (text.Length > 2)
		{
			if (text[1] == '.' || text[1] == ',')
			{
				string text2 = text.Substring(0, 1);
				string text3 = text.Substring(1);
				TextRenderer.DrawText(graphics, text2, font, new Point(-bitmap.Width / 4, bitmap.Height / 2), this.color, color, TextFormatFlags.VerticalCenter);
				TextRenderer.DrawText(graphics, text3, smallFont, new Point(bitmap.Width / 4, bitmap.Height), this.color, color, TextFormatFlags.Bottom);
			}
			else
			{
				Size size = TextRenderer.MeasureText(text, smallFont);
				TextRenderer.DrawText(graphics, text, smallFont, new Point((bitmap.Width - size.Width) / 2, bitmap.Height / 2), this.color, color, TextFormatFlags.VerticalCenter);
			}
		}
		else
		{
			Size size2 = TextRenderer.MeasureText(text, font);
			TextRenderer.DrawText(graphics, text, font, new Point((bitmap.Width - size2.Width) / 2, bitmap.Height / 2), this.color, color, TextFormatFlags.VerticalCenter);
		}
		if (color == Color.Transparent)
		{
			return Create(bitmap);
		}
		BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		IntPtr scan = bitmapData.Scan0;
		int num = bitmap.Width * bitmap.Height * 4;
		byte[] array = new byte[num];
		Marshal.Copy(scan, array, 0, num);
		bitmap.UnlockBits(bitmapData);
		for (int i = 0; i < array.Length; i += 4)
		{
			byte b = array[i];
			byte b2 = array[i + 1];
			byte b3 = array[i + 2];
			array[i] = this.color.B;
			array[i + 1] = this.color.G;
			array[i + 2] = this.color.R;
			array[i + 3] = (byte)(0.3 * (double)(int)b3 + 0.59 * (double)(int)b2 + 0.11 * (double)(int)b);
		}
		return Create(array, bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
	}

	private static bool IsFontInstalled(string fontName, float fontSize = 12f)
	{
		using Font font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
		return font.Name == fontName;
	}
}
