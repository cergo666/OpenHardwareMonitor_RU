using System;
using System.Drawing;
using System.Windows.Forms;

namespace sergiye.Common;

public class ThemedHScrollIndicator : Control
{
	private readonly HScrollBar _scrollbar;

	private int _startValue;

	private int _startPos;

	private bool _isScrolling;

	public static void AddToControl(Control control)
	{
		foreach (Control control2 in control.Controls)
		{
			if (control2 is HScrollBar scrollBar)
			{
				control.Controls.Add(new ThemedHScrollIndicator(scrollBar));
				break;
			}
		}
	}

	public ThemedHScrollIndicator(HScrollBar scrollBar)
	{
		_scrollbar = scrollBar;
		base.Height = 8;
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		base.Left = 0;
		base.Top = scrollBar.Parent.Height - base.Height;
		base.Size = new Size(scrollBar.Parent.Width, base.Height);
		Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
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
		scrollBar.Height = 0;
		base.MouseDown += OnMouseDown;
	}

	private void OnMouseDown(object sender, MouseEventArgs e)
	{
		if (!_isScrolling)
		{
			_isScrolling = true;
			_startPos = e.X;
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
			double num2 = num / (double)base.Bounds.Width;
			double num3 = (double)_startValue + (double)(e.X - _startPos) * num2;
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
		int num = base.Bounds.Width;
		int num2 = _scrollbar.Maximum - _scrollbar.Minimum;
		if (num2 > 0)
		{
			int num3 = num * (_scrollbar.Value - _scrollbar.Minimum) / num2;
			int num4 = num * (_scrollbar.Value - _scrollbar.Minimum + _scrollbar.LargeChange) / num2;
			using SolidBrush brush = new SolidBrush(Theme.Current.StrongLineColor);
			graphics.FillRectangle(brush, new Rectangle(num3, 2, num4 - num3, base.Bounds.Height - 4));
		}
	}
}
