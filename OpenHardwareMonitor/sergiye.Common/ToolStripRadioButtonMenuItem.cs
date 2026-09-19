using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace sergiye.Common;

public class ToolStripRadioButtonMenuItem : ToolStripMenuItem
{
	private bool mouseHoverState;

	private bool mouseDownState;

	public static bool DisplayAsCheckboxes { get; set; }

	public override bool Enabled
	{
		get
		{
			if (!base.DesignMode && base.OwnerItem is ToolStripMenuItem { CheckOnClick: not false } toolStripMenuItem)
			{
				if (base.Enabled)
				{
					return toolStripMenuItem.Checked;
				}
				return false;
			}
			return base.Enabled;
		}
		set
		{
			base.Enabled = value;
		}
	}

	public ToolStripRadioButtonMenuItem()
	{
		Initialize();
	}

	public ToolStripRadioButtonMenuItem(string text)
		: base(text, (Image)null, (EventHandler)null)
	{
		Initialize();
	}

	public ToolStripRadioButtonMenuItem(Image image)
		: base((string)null, image, (EventHandler)null)
	{
		Initialize();
	}

	public ToolStripRadioButtonMenuItem(string text, Image image)
		: base(text, image, (EventHandler)null)
	{
		Initialize();
	}

	public ToolStripRadioButtonMenuItem(string text, Image image, EventHandler onClick)
		: base(text, image, onClick)
	{
		Initialize();
	}

	public ToolStripRadioButtonMenuItem(string text, Image image, EventHandler onClick, string name)
		: base(text, image, onClick, name)
	{
		Initialize();
	}

	public ToolStripRadioButtonMenuItem(string text, Image image, params ToolStripItem[] dropDownItems)
		: base(text, image, dropDownItems)
	{
		Initialize();
	}

	public ToolStripRadioButtonMenuItem(string text, Image image, EventHandler onClick, Keys shortcutKeys)
		: base(text, image, onClick)
	{
		Initialize();
		base.ShortcutKeys = shortcutKeys;
	}

	private void Initialize()
	{
		base.CheckOnClick = true;
	}

	protected override void OnCheckedChanged(EventArgs e)
	{
		base.OnCheckedChanged(e);
		if (!base.Checked || base.Parent == null)
		{
			return;
		}
		foreach (ToolStripItem item in base.Parent.Items)
		{
			if (item is ToolStripRadioButtonMenuItem toolStripRadioButtonMenuItem && toolStripRadioButtonMenuItem != this && toolStripRadioButtonMenuItem.Checked)
			{
				toolStripRadioButtonMenuItem.Checked = false;
				break;
			}
		}
	}

	protected override void OnClick(EventArgs e)
	{
		if (!base.Checked)
		{
			base.OnClick(e);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (DisplayAsCheckboxes || Image != null)
		{
			base.OnPaint(e);
			return;
		}
		CheckState checkState = base.CheckState;
		base.CheckState = CheckState.Unchecked;
		base.OnPaint(e);
		base.CheckState = checkState;
		RadioButtonState radioButtonState = RadioButtonState.UncheckedNormal;
		if (Enabled)
		{
			if (mouseDownState)
			{
				radioButtonState = (base.Checked ? RadioButtonState.CheckedPressed : RadioButtonState.UncheckedPressed);
			}
			else if (mouseHoverState)
			{
				radioButtonState = (base.Checked ? RadioButtonState.CheckedHot : RadioButtonState.UncheckedHot);
			}
			else if (base.Checked)
			{
				radioButtonState = RadioButtonState.CheckedNormal;
			}
		}
		else
		{
			radioButtonState = (base.Checked ? RadioButtonState.CheckedDisabled : RadioButtonState.UncheckedDisabled);
		}
		int num = (base.ContentRectangle.Height - RadioButtonRenderer.GetGlyphSize(e.Graphics, radioButtonState).Height) / 2;
		Point glyphLocation = new Point(base.ContentRectangle.Location.X + 4, base.ContentRectangle.Location.Y + num);
		RadioButtonRenderer.DrawRadioButton(e.Graphics, glyphLocation, radioButtonState);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		if (!DisplayAsCheckboxes)
		{
			mouseHoverState = true;
			Invalidate();
		}
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		if (!DisplayAsCheckboxes)
		{
			mouseHoverState = false;
		}
		base.OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (!DisplayAsCheckboxes)
		{
			mouseDownState = true;
			Invalidate();
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (!DisplayAsCheckboxes)
		{
			mouseDownState = false;
		}
		base.OnMouseUp(e);
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		if (base.OwnerItem is ToolStripMenuItem { CheckOnClick: not false } toolStripMenuItem)
		{
			toolStripMenuItem.CheckedChanged -= OwnerMenuItem_CheckedChanged;
			toolStripMenuItem.CheckedChanged += OwnerMenuItem_CheckedChanged;
		}
		base.OnOwnerChanged(e);
	}

	private void OwnerMenuItem_CheckedChanged(object sender, EventArgs e)
	{
		Invalidate();
	}
}
