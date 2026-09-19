using System;
using System.Drawing;
using System.Windows.Forms;

namespace sergiye.Common;

public class ThemedVScrollIndicator : Control
{
	private readonly VScrollBar _scrollbar;

	private int _startValue;

	private int _startPos;

	private bool _isScrolling;

	public static void AddToControl(Control control)
	{
		foreach (Control control2 in control.Controls)
		{
			if (control2 is VScrollBar scrollBar)
			{
				control.Controls.Add(new ThemedVScrollIndicator(scrollBar));
				break;
			}
		}
	}

	public ThemedVScrollIndicator(VScrollBar scrollBar)
	{
		_scrollbar = scrollBar;
		base.Width = 8;
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		base.Left = scrollBar.Parent.Width - base.Width;
		base.Top = 0;
		base.Size = new Size(base.Width, scrollBar.Parent.Height);
		Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
		base.Visible = scrollBar.Visible;
		scrollBar.VisibleChanged += delegate(object s, EventArgs e)
		{
			base.Visible = (s as ScrollBar).Visible;
		};
		scrollBar.Scroll += delegate
		{
			Invalidate();
		};
		scrollBar.ValueChanged += delegate
		{
			Invalidate();
		};
		scrollBar.Width = 0;
		base.MouseDown += OnMouseDown;
	}

	private void OnMouseDown(object sender, MouseEventArgs e)
	{
		if (!_isScrolling)
		{
			_isScrolling = true;
			_startPos = e.Y;
			_startValue = _scrollbar.Value;
			base.MouseUp += OnMouseUp;
			base.MouseMove += OnMouseMove;
		}
	}

	private void OnMouseUp(object sender, MouseEventArgs e)
	{
		_isScrolling = false;
		base.MouseUp -= OnMouseUp;
		base.MouseMove -= OnMouseMove;
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (!_isScrolling)
		{
			return;
		}
		double num = _scrollbar.Maximum - _scrollbar.Minimum;
		if (!(num <= 0.0))
		{
			double num2 = num / (double)base.Bounds.Height;
			double num3 = (double)_startValue + (double)(e.Y - _startPos) * num2;
			if (num3 < (double)_scrollbar.Minimum)
			{
				num3 = _scrollbar.Minimum;
			}
			if (num3 > (double)(_scrollbar.Maximum - _scrollbar.LargeChange))
			{
				num3 = _scrollbar.Maximum - _scrollbar.LargeChange;
			}
			_scrollbar.Value = (int)num3;
			Refresh();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.FillRectangle(Theme.BackgroundBrush, new Rectangle(0, 0, base.Bounds.Width, base.Bounds.Height));
		int num = base.Bounds.Height;
		int num2 = _scrollbar.Maximum - _scrollbar.Minimum;
		if (num2 > 0)
		{
			int num3 = num * (_scrollbar.Value - _scrollbar.Minimum) / num2;
			int num4 = num * (_scrollbar.Value - _scrollbar.Minimum + _scrollbar.LargeChange) / num2;
			using SolidBrush brush = new SolidBrush(Theme.Current.StrongLineColor);
			graphics.FillRectangle(brush, new Rectangle(2, num3, base.Bounds.Width - 4, num4 - num3));
		}
	}
}
