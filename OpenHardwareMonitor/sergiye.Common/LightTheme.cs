using System.Drawing;

namespace sergiye.Common;

public class LightTheme : Theme
{
	public static readonly Color DefaultDisabledColor = Color.DimGray;

	public static readonly Color DefaultMessageColor = Color.ForestGreen;

	public static readonly Color DefaultInfoColor = Color.DarkGoldenrod;

	public static readonly Color DefaultWarnColor = Color.Red;

	public LightTheme()
		: base("light", "Light")
	{
		ForegroundColor = Color.Black;
		BackgroundColor = Color.White;
		HyperlinkColor = ColorTranslator.FromHtml("#0075FF");
		SelectedForegroundColor = ForegroundColor;
		SelectedBackgroundColor = ColorTranslator.FromHtml("#CCD5F0");
		LineColor = ColorTranslator.FromHtml("#F7F7F7");
		StrongLineColor = ColorTranslator.FromHtml("#40568D");
		WindowTitleBarFallbackToImmersiveDarkMode = false;
		DisabledColor = DefaultDisabledColor;
		MessageColor = DefaultMessageColor;
		InfoColor = DefaultInfoColor;
		WarnColor = DefaultWarnColor;
	}
}
