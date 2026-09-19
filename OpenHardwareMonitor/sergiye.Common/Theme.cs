using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;

namespace sergiye.Common;

public abstract class Theme
{
	public const string SkipThemeTag = "sergiye.Common.Theme.SkipThemeTag";

	public const string SkipThemeWithChildsTag = "sergiye.Common.Theme.SkipThemeWithChildsTag";

	public const string SkipThemeSubItems = "sergiye.Common.Theme.SkipThemeSubItems";

	private static Theme current;

	public static Action OnCurrentChanged;

	public static Func<Control, Theme, bool> OnApplyToControl;

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

	private const int DWMWA_BORDER_COLOR = 34;

	private const int DWMWA_CAPTION_COLOR = 35;

	private const int DWMWA_TEXT_COLOR = 36;

	private const int DWMWA_MICA_EFFECT = 1029;

	public static Theme Current
	{
		get
		{
			if (current == null)
			{
				current = new LightTheme();
				CurrentColorsChanged();
			}
			return current;
		}
		set
		{
			current = value;
			foreach (Form openForm in Application.OpenForms)
			{
				current.Apply(openForm);
			}
			SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
			IsAutoThemeEnabled = false;
			CurrentColorsChanged();
		}
	}

	public static bool IsAutoThemeEnabled { get; private set; }

	public static bool IsThemedCheckboxEnabled { get; set; } = true;

	public static bool IsThemedRadioButtonEnabled { get; set; } = true;

	public static bool AppsUseLightTheme
	{
		get
		{
			if (!(Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", null) is int num))
			{
				return true;
			}
			return num > 0;
		}
	}

	public string Id { get; }

	public string DisplayName { get; }

	public virtual Color BackgroundColor { get; protected set; }

	public virtual Color ForegroundColor { get; protected set; }

	public virtual Color HyperlinkColor { get; protected set; }

	public virtual Color LineColor { get; protected set; }

	public virtual Color StrongLineColor { get; protected set; }

	public virtual Color SelectedBackgroundColor { get; protected set; }

	public virtual Color SelectedForegroundColor { get; protected set; }

	public virtual Color DisabledColor { get; protected set; }

	public virtual Color MessageColor { get; protected set; }

	public virtual Color InfoColor { get; protected set; }

	public virtual Color WarnColor { get; protected set; }

	public virtual bool WindowTitleBarFallbackToImmersiveDarkMode { get; protected set; }

	internal static Brush BackgroundBrush { get; private set; }

	internal static Brush ForegroundBrush { get; private set; }

	internal static Brush SelectedBackBrush { get; private set; }

	internal static Brush SelectedForeBrush { get; private set; }

	internal static Pen SelectedForePen { get; private set; }

	internal static Pen ForegroundPen { get; private set; }

	internal static Pen LinePen { get; private set; }

	public static bool SupportsAutoThemeSwitching()
	{
		if (Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1) is int num)
		{
			return num != -1;
		}
		return false;
	}

	public static void SetAutoTheme()
	{
		if (AppsUseLightTheme)
		{
			if (!(Current is LightTheme))
			{
				Current = new LightTheme();
			}
		}
		else if (!(Current is DarkTheme))
		{
			Current = new DarkTheme();
		}
		if (SupportsAutoThemeSwitching())
		{
			SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
			SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
			IsAutoThemeEnabled = true;
			OnCurrentChanged?.Invoke();
		}
	}

	private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		if (AppsUseLightTheme)
		{
			if (Current is LightTheme)
			{
				return;
			}
		}
		else if (Current is DarkTheme)
		{
			return;
		}
		SetAutoTheme();
	}

	protected Theme(string id, string displayName)
	{
		Id = id;
		DisplayName = displayName;
	}

	public void Apply(Form form)
	{
		if (IsWindows10OrGreater(22000))
		{
			int attrValue = ColorTranslator.ToWin32(BackgroundColor);
			DwmSetWindowAttribute(form.Handle, 35, ref attrValue, 4);
			attrValue = ColorTranslator.ToWin32(ForegroundColor);
			DwmSetWindowAttribute(form.Handle, 36, ref attrValue, 4);
		}
		else if (IsWindows10OrGreater(17763))
		{
			int attr = 19;
			if (IsWindows10OrGreater(18985))
			{
				attr = 20;
			}
			int attrValue2 = (WindowTitleBarFallbackToImmersiveDarkMode ? 1 : 0);
			DwmSetWindowAttribute(form.Handle, attr, ref attrValue2, 4);
		}
		form.BackColor = BackgroundColor;
		foreach (Control control in form.Controls)
		{
			Apply(control);
		}
		if (form.Visible && !OSHelper.IsWindows11OrGreater)
		{
			form.Visible = false;
			form.Visible = true;
		}
	}

	public void Apply(Control control)
	{
		string text = control.Tag as string;
		if (text == null || (!"sergiye.Common.Theme.SkipThemeTag".Equals(text) && !"sergiye.Common.Theme.SkipThemeWithChildsTag".Equals(text)))
		{
			ApplyInner(control);
		}
		if (text != null && "sergiye.Common.Theme.SkipThemeWithChildsTag".Equals(text))
		{
			return;
		}
		foreach (Control control2 in control.Controls)
		{
			Apply(control2);
		}
	}

	private void ApplyInner(Control control)
	{
		if (control is Button button)
		{
			button.ForeColor = ForegroundColor;
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = ForegroundColor;
			button.FlatAppearance.MouseOverBackColor = SelectedBackgroundColor;
			button.MouseEnter -= Button_MouseEnter;
			button.MouseLeave -= Button_MouseLeave;
			button.MouseEnter += Button_MouseEnter;
			button.MouseLeave += Button_MouseLeave;
		}
		if (control is ComboBox comboBox)
		{
			comboBox.ForeColor = ForegroundColor;
			comboBox.BackColor = BackgroundColor;
			comboBox.FlatStyle = FlatStyle.Flat;
			comboBox.DrawMode = DrawMode.OwnerDrawFixed;
			comboBox.DrawItem -= ComboBox_DrawItem;
			comboBox.DrawItem += ComboBox_DrawItem;
		}
		if (control is CheckBox checkBox)
		{
			checkBox.ForeColor = ForegroundColor;
			checkBox.BackColor = BackgroundColor;
			checkBox.FlatStyle = FlatStyle.Flat;
			checkBox.Paint -= CheckBox_DrawItem;
			checkBox.Paint += CheckBox_DrawItem;
			if (checkBox.Appearance == Appearance.Button)
			{
				checkBox.Paint -= CheckBox_DrawButtonItem;
				checkBox.Paint += CheckBox_DrawButtonItem;
				checkBox.MouseEnter -= Button_MouseEnter;
				checkBox.MouseLeave -= Button_MouseLeave;
				checkBox.MouseEnter += Button_MouseEnter;
				checkBox.MouseLeave += Button_MouseLeave;
			}
		}
		if (control is RadioButton radioButton)
		{
			radioButton.ForeColor = ForegroundColor;
			radioButton.BackColor = BackgroundColor;
			radioButton.FlatStyle = FlatStyle.Flat;
			radioButton.Paint -= RadioButton_DrawItem;
			radioButton.Paint += RadioButton_DrawItem;
		}
		else if (control is LinkLabel linkLabel)
		{
			linkLabel.LinkColor = HyperlinkColor;
		}
		else if (control is ListBox listBox)
		{
			listBox.ForeColor = ForegroundColor;
			listBox.BackColor = BackgroundColor;
		}
		else if (control is ListView listView)
		{
			control.BackColor = BackgroundColor;
			control.ForeColor = ForegroundColor;
			listView.OwnerDraw = true;
			listView.DrawColumnHeader -= ListView_DrawColumnHeader;
			listView.DrawColumnHeader += ListView_DrawColumnHeader;
			listView.DrawSubItem -= ListView_DrawSubItem;
			listView.DrawSubItem += ListView_DrawSubItem;
		}
		else if (control is TabControl tabControl)
		{
			control.BackColor = BackgroundColor;
			control.ForeColor = ForegroundColor;
			tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
			tabControl.DrawItem -= TabControl_DrawItem;
			tabControl.DrawItem += TabControl_DrawItem;
		}
		else if (control is TabPage tabPage)
		{
			tabPage.UseVisualStyleBackColor = true;
			tabPage.BorderStyle = BorderStyle.None;
			control.BackColor = BackgroundColor;
			control.ForeColor = ForegroundColor;
		}
		else if (control is GroupBox groupBox)
		{
			groupBox.BackColor = BackgroundColor;
			groupBox.ForeColor = ForegroundColor;
		}
		else if (control is PropertyGrid propertyGrid)
		{
			propertyGrid.BackColor = BackgroundColor;
			propertyGrid.ForeColor = ForegroundColor;
			propertyGrid.CategoryForeColor = ForegroundColor;
			propertyGrid.CategorySplitterColor = LineColor;
			propertyGrid.CommandsActiveLinkColor = HyperlinkColor;
			propertyGrid.CommandsDisabledLinkColor = InfoColor;
			propertyGrid.CommandsLinkColor = HyperlinkColor;
			propertyGrid.CommandsBackColor = BackgroundColor;
			propertyGrid.CommandsBorderColor = LineColor;
			propertyGrid.CommandsForeColor = ForegroundColor;
			propertyGrid.DisabledItemForeColor = InfoColor;
			propertyGrid.LineColor = LineColor;
			propertyGrid.SelectedItemWithFocusBackColor = SelectedBackgroundColor;
			propertyGrid.SelectedItemWithFocusForeColor = SelectedForegroundColor;
			propertyGrid.ViewBackColor = BackgroundColor;
			propertyGrid.ViewBorderColor = StrongLineColor;
			propertyGrid.ViewForeColor = ForegroundColor;
		}
		else if (control is NumericUpDown numericUpDown)
		{
			numericUpDown.BackColor = BackgroundColor;
			numericUpDown.ForeColor = ForegroundColor;
		}
		else if (control is DateTimePicker dateTimePicker)
		{
			dateTimePicker.BackColor = BackgroundColor;
			dateTimePicker.ForeColor = ForegroundColor;
		}
		else if (OnApplyToControl == null || !OnApplyToControl(control, current))
		{
			control.BackColor = BackgroundColor;
			control.ForeColor = ForegroundColor;
		}
		if (control is IThemeApplicable themeApplicable)
		{
			themeApplicable.ApplyTheme(this);
		}
		if (WindowTitleBarFallbackToImmersiveDarkMode)
		{
			SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
		}
		else
		{
			SetWindowTheme(control.Handle, "Explorer", null);
		}
	}

	private static void CurrentColorsChanged()
	{
		BackgroundBrush = new SolidBrush(current.BackgroundColor);
		ForegroundBrush = new SolidBrush(current.ForegroundColor);
		SelectedBackBrush = new SolidBrush(current.SelectedBackgroundColor);
		SelectedForeBrush = new SolidBrush(current.SelectedForegroundColor);
		SelectedForePen = new Pen(current.SelectedForegroundColor, 2f);
		ForegroundPen = new Pen(current.ForegroundColor, 2f);
		LinePen = new Pen(current.LineColor, 2f);
		OnCurrentChanged?.Invoke();
	}

	public Bitmap GetBitmapFromImage(Image image, Size size, bool invertForDarkTheme = true)
	{
		Bitmap bitmap = new Bitmap(image, size);
		if (invertForDarkTheme && WindowTitleBarFallbackToImmersiveDarkMode)
		{
			for (int i = 0; i <= bitmap.Height - 1; i++)
			{
				for (int j = 0; j <= bitmap.Width - 1; j++)
				{
					Color pixel = bitmap.GetPixel(j, i);
					pixel = Color.FromArgb(pixel.A, 255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
					bitmap.SetPixel(j, i, pixel);
				}
			}
		}
		return bitmap;
	}

	private void Button_MouseEnter(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			button.ForeColor = SelectedForegroundColor;
		}
		if (sender is CheckBox checkBox)
		{
			checkBox.BackColor = SelectedBackgroundColor;
			checkBox.ForeColor = SelectedForegroundColor;
		}
	}

	private void Button_MouseLeave(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			button.ForeColor = ForegroundColor;
		}
		if (sender is CheckBox checkBox)
		{
			checkBox.BackColor = BackgroundColor;
			checkBox.ForeColor = ForegroundColor;
		}
	}

	private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
	{
		if (e.Index >= 0 && sender is ComboBox comboBox)
		{
			bool flag = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
			e.Graphics.FillRectangle(flag ? SelectedBackBrush : BackgroundBrush, e.Bounds);
			e.Graphics.DrawString(comboBox.Items[e.Index].ToString(), e.Font, flag ? SelectedForeBrush : ForegroundBrush, e.Bounds);
			e.DrawFocusRectangle();
		}
	}

	private void CheckBox_DrawItem(object sender, PaintEventArgs e)
	{
		if (!(sender is CheckBox checkBox))
		{
			return;
		}
		e.Graphics.Clear(checkBox.BackColor);
		CheckBoxState checkBoxState = GetCheckBoxState(checkBox);
		Size glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, checkBoxState);
		Rectangle rectangle = new Rectangle(0, (checkBox.Height - glyphSize.Height) / 2, glyphSize.Width, glyphSize.Height);
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		if (IsThemedCheckboxEnabled && checkBox.Checked)
		{
			e.Graphics.FillRectangle(SelectedBackBrush, rectangle);
			ControlPaint.DrawBorder(e.Graphics, rectangle, StrongLineColor, ButtonBorderStyle.Solid);
			if (checkBox.CheckState == CheckState.Checked)
			{
				int num = Math.Min(rectangle.Height, rectangle.Width) / 3;
				e.Graphics.DrawLines(SelectedForePen, new Point[3]
				{
					new Point(rectangle.Left + num, rectangle.Top + rectangle.Height / 2),
					new Point(rectangle.Left + rectangle.Width / 2 - 1, rectangle.Bottom - num),
					new Point(rectangle.Right - num, rectangle.Top + num)
				});
			}
			else if (checkBox.CheckState == CheckState.Indeterminate)
			{
				int num2 = Math.Min(rectangle.Height, rectangle.Width) / 3;
				e.Graphics.DrawLines(SelectedForePen, new Point[2]
				{
					new Point(rectangle.Left + num2, rectangle.Top + rectangle.Height / 2),
					new Point(rectangle.Right - num2, rectangle.Top + rectangle.Height / 2)
				});
			}
		}
		else
		{
			CheckBoxRenderer.DrawCheckBox(e.Graphics, rectangle.Location, checkBoxState);
		}
		TextRenderer.DrawText(bounds: new Rectangle(glyphSize.Width, 0, checkBox.Width - glyphSize.Width, checkBox.Height), dc: e.Graphics, text: checkBox.Text, font: checkBox.Font, foreColor: checkBox.Enabled ? checkBox.ForeColor : DisabledColor, flags: TextFormatFlags.VerticalCenter);
	}

	private void CheckBox_DrawButtonItem(object sender, PaintEventArgs e)
	{
		if (sender is CheckBox checkBox)
		{
			e.Graphics.Clear(checkBox.Checked ? SelectedBackgroundColor : checkBox.BackColor);
			TextRenderer.DrawText(e.Graphics, checkBox.Text, checkBox.Font, e.ClipRectangle, checkBox.Checked ? SelectedForegroundColor : checkBox.ForeColor, GetTextAlignment(checkBox.TextAlign));
		}
	}

	private void RadioButton_DrawItem(object sender, PaintEventArgs e)
	{
		if (sender is RadioButton radioButton)
		{
			e.Graphics.Clear(radioButton.BackColor);
			Size glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, CheckBoxState.MixedNormal);
			Rectangle rectangle = new Rectangle(0, (radioButton.Height - glyphSize.Height) / 2, glyphSize.Width, glyphSize.Height);
			if (IsThemedRadioButtonEnabled && radioButton.Checked)
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				e.Graphics.FillEllipse(SelectedBackBrush, rectangle);
				int num = Math.Min(rectangle.Height, rectangle.Width) / 3;
				rectangle.Inflate(-num, -num);
				e.Graphics.FillEllipse(SelectedForeBrush, rectangle);
			}
			else
			{
				ControlPaint.DrawRadioButton(e.Graphics, rectangle, ButtonState.Normal);
			}
			TextRenderer.DrawText(bounds: new Rectangle(glyphSize.Width, 0, radioButton.Width - glyphSize.Width, radioButton.Height), dc: e.Graphics, text: radioButton.Text, font: radioButton.Font, foreColor: radioButton.Enabled ? radioButton.ForeColor : DisabledColor, flags: TextFormatFlags.VerticalCenter);
		}
	}

	private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
	{
		e.Graphics.FillRectangle(BackgroundBrush, e.Bounds);
		e.Graphics.DrawString(e.Header.Text, e.Font, ForegroundBrush, e.Bounds);
	}

	private void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
	{
		if (!(sender is ListView listView))
		{
			return;
		}
		if (listView.Tag is string value && "sergiye.Common.Theme.SkipThemeSubItems".Equals(value))
		{
			e.DrawDefault = true;
			return;
		}
		Color foreColor = e.Item.ForeColor;
		if (e.Item.Selected)
		{
			if (e.ColumnIndex == 0 || listView.FullRowSelect)
			{
				e.Graphics.FillRectangle(SelectedBackBrush, e.Bounds);
				foreColor = SelectedForegroundColor;
			}
		}
		else
		{
			e.DrawBackground();
		}
		bool flag = listView.CheckBoxes && e.Item.Checked;
		Rectangle rectangle = new Rectangle(e.Bounds.X, e.Bounds.Y + (e.Bounds.Height - 16) / 2, 16, 16);
		Rectangle bounds = (listView.CheckBoxes ? new Rectangle(e.Bounds.X + 20, e.Bounds.Y, e.Bounds.Width - 20, e.Bounds.Height) : e.Bounds);
		if (listView.CheckBoxes && e.ColumnIndex == 0)
		{
			if (IsThemedCheckboxEnabled && flag)
			{
				e.Graphics.Clear(listView.BackColor);
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				e.Graphics.FillRectangle(SelectedBackBrush, rectangle);
				int num = Math.Min(rectangle.Height, rectangle.Width) / 3;
				e.Graphics.DrawLines(SelectedForePen, new Point[3]
				{
					new Point(rectangle.Left + num, rectangle.Top + rectangle.Height / 2),
					new Point(rectangle.Left + rectangle.Width / 2 - 1, rectangle.Bottom - num),
					new Point(rectangle.Right - num, rectangle.Top + num)
				});
			}
			else
			{
				ControlPaint.DrawCheckBox(e.Graphics, rectangle, flag ? ButtonState.Checked : ButtonState.Normal);
			}
		}
		TextFormatFlags textAlignment = GetTextAlignment(listView, e.ColumnIndex);
		TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, bounds, foreColor, textAlignment);
	}

	private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
	{
		if (sender is TabControl tabControl)
		{
			TabPage tabPage = tabControl.TabPages[e.Index];
			e.Graphics.FillRectangle(BackgroundBrush, e.Bounds);
			Rectangle bounds = e.Bounds;
			int y = ((e.State != DrawItemState.Selected) ? 1 : (-2));
			bounds.Offset(1, y);
			TextRenderer.DrawText(e.Graphics, tabPage.Text, e.Font, bounds, ForegroundColor);
		}
	}

	private TextFormatFlags GetTextAlignment(ListView lstView, int colIndex)
	{
		TextFormatFlags textFormatFlags = ((lstView.View != View.Tile) ? TextFormatFlags.VerticalCenter : ((colIndex != 0) ? TextFormatFlags.Bottom : TextFormatFlags.Default));
		if (lstView.View == View.Details)
		{
			textFormatFlags |= TextFormatFlags.LeftAndRightPadding;
		}
		if (lstView.Columns[colIndex].TextAlign != HorizontalAlignment.Left)
		{
			textFormatFlags = (TextFormatFlags)((int)textFormatFlags | (int)(lstView.Columns[colIndex].TextAlign ^ (HorizontalAlignment)3));
		}
		return textFormatFlags;
	}

	private TextFormatFlags GetTextAlignment(System.Drawing.ContentAlignment alignment)
	{
		return alignment switch
		{
			System.Drawing.ContentAlignment.BottomCenter => TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter, 
			System.Drawing.ContentAlignment.BottomLeft => TextFormatFlags.Bottom, 
			System.Drawing.ContentAlignment.BottomRight => TextFormatFlags.Bottom | TextFormatFlags.Right, 
			System.Drawing.ContentAlignment.MiddleCenter => TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter, 
			System.Drawing.ContentAlignment.MiddleLeft => TextFormatFlags.VerticalCenter, 
			System.Drawing.ContentAlignment.MiddleRight => TextFormatFlags.Right | TextFormatFlags.VerticalCenter, 
			System.Drawing.ContentAlignment.TopCenter => TextFormatFlags.HorizontalCenter, 
			System.Drawing.ContentAlignment.TopLeft => TextFormatFlags.Default, 
			System.Drawing.ContentAlignment.TopRight => TextFormatFlags.Right, 
			_ => TextFormatFlags.Default, 
		};
	}

	private static CheckBoxState GetCheckBoxState(CheckBox checkBox)
	{
		bool enabled = checkBox.Enabled;
		bool capture = checkBox.Capture;
		bool flag = checkBox.ClientRectangle.Contains(checkBox.PointToClient(Cursor.Position));
		switch (checkBox.CheckState)
		{
		case CheckState.Unchecked:
			if (!enabled)
			{
				return CheckBoxState.UncheckedDisabled;
			}
			if (capture)
			{
				return CheckBoxState.UncheckedPressed;
			}
			if (!flag)
			{
				return CheckBoxState.UncheckedNormal;
			}
			return CheckBoxState.UncheckedHot;
		case CheckState.Checked:
			if (!enabled)
			{
				return CheckBoxState.CheckedDisabled;
			}
			if (capture)
			{
				return CheckBoxState.CheckedPressed;
			}
			if (!flag)
			{
				return CheckBoxState.CheckedNormal;
			}
			return CheckBoxState.CheckedHot;
		case CheckState.Indeterminate:
			if (!enabled)
			{
				return CheckBoxState.MixedDisabled;
			}
			if (capture)
			{
				return CheckBoxState.MixedPressed;
			}
			if (!flag)
			{
				return CheckBoxState.MixedNormal;
			}
			return CheckBoxState.MixedHot;
		default:
			return CheckBoxState.UncheckedNormal;
		}
	}

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

	private static bool IsWindows10OrGreater(int build = -1)
	{
		if (Environment.OSVersion.Version.Major >= 10)
		{
			return Environment.OSVersion.Version.Build >= build;
		}
		return false;
	}
}
