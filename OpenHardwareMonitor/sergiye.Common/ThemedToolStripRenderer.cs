using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace sergiye.Common;

public class ThemedToolStripRenderer : ToolStripRenderer
{
	protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
	{
		if (e.Item is ToolStripSeparator)
		{
			Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
			e.Graphics.FillRectangle(Theme.BackgroundBrush, rect);
			using Pen pen = new Pen(Theme.ForegroundBrush, 1.5f);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			int left = e.ToolStrip.Padding.Left;
			e.Graphics.DrawLines(pen, new Point[2]
			{
				new Point(rect.Left + left, rect.Top + rect.Height / 2),
				new Point(rect.Right - 3, rect.Top + rect.Height / 2)
			});
			return;
		}
		base.OnRenderSeparator(e);
	}

	protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
	{
		e.ArrowColor = (e.Item.Selected ? Theme.Current.SelectedForegroundColor : Theme.Current.ForegroundColor);
		base.OnRenderArrow(e);
	}

	protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle contentRectangle = e.Item.ContentRectangle;
		contentRectangle.Width = contentRectangle.Height;
		e.Graphics.FillRectangle(Theme.SelectedBackBrush, contentRectangle);
		int num = Math.Min(contentRectangle.Height, contentRectangle.Width) / 3;
		e.Graphics.DrawLines(Theme.SelectedForePen, new Point[3]
		{
			new Point(contentRectangle.Left + num, contentRectangle.Top + contentRectangle.Height / 2),
			new Point(contentRectangle.Left + contentRectangle.Width / 2 - 1, contentRectangle.Bottom - num),
			new Point(contentRectangle.Right - num, contentRectangle.Top + num)
		});
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		e.TextColor = (e.Item.Selected ? Theme.Current.SelectedForegroundColor : Theme.Current.ForegroundColor);
		base.OnRenderItemText(e);
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		if (!(e.ToolStrip.Parent is Form))
		{
			Rectangle rect = new Rectangle(Point.Empty, new Size(e.ToolStrip.Width - 1, e.ToolStrip.Height - 1));
			using Pen pen = new Pen(Theme.Current.ForegroundColor, 0.5f);
			e.Graphics.DrawRectangle(pen, rect);
			return;
		}
		base.OnRenderToolStripBorder(e);
	}

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		Rectangle rect = new Rectangle(Point.Empty, e.ToolStrip.Size);
		e.Graphics.FillRectangle(Theme.BackgroundBrush, rect);
	}

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
		e.Graphics.FillRectangle(e.Item.Selected ? Theme.SelectedBackBrush : Theme.BackgroundBrush, rect);
	}
}
