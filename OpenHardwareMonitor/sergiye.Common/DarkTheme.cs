using System.Drawing;

namespace sergiye.Common;

public class DarkTheme : Theme
{
	public static readonly Color DefaultDisabledColor = Color.Gray;

	public static readonly Color DefaultMessageColor = Color.LawnGreen;

	public static readonly Color DefaultInfoColor = Color.Gold;

	public static readonly Color DefaultWarnColor = Color.OrangeRed;

	public DarkTheme()
		: base("dark", "Dark")
	{
		ForegroundColor = ColorTranslator.FromHtml("#E9E9E9");
		BackgroundColor = ColorTranslator.FromHtml("#1E1E1E");
		HyperlinkColor = ColorTranslator.FromHtml("#74B2D8");
		SelectedForegroundColor = ColorTranslator.FromHtml("#E9E9E9");
		SelectedBackgroundColor = ColorTranslator.FromHtml("#2B5278");
		LineColor = ColorTranslator.FromHtml("#262626");
		StrongLineColor = ColorTranslator.FromHtml("#454545");
		WindowTitleBarFallbackToImmersiveDarkMode = true;
		DisabledColor = DefaultDisabledColor;
		MessageColor = DefaultMessageColor;
		InfoColor = DefaultInfoColor;
		WarnColor = DefaultWarnColor;
	}
}
