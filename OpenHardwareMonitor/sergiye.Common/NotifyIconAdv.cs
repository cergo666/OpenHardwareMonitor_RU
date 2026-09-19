using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace sergiye.Common;

public class NotifyIconAdv : IDisposable
{
	private class NotifyIconWindowsImplementation : Component
	{
		private class NotifyIconNativeWindow : NativeWindow
		{
			private readonly NotifyIconWindowsImplementation reference;

			private GCHandle referenceHandle;

			internal NotifyIconNativeWindow(NotifyIconWindowsImplementation component)
			{
				reference = component;
			}

			~NotifyIconNativeWindow()
			{
				if (base.Handle != IntPtr.Zero)
				{
					WinApiHelper.PostMessage(new HandleRef(this, base.Handle), 16, 0, 0);
				}
			}

			public void LockReference(bool locked)
			{
				if (locked)
				{
					if (!referenceHandle.IsAllocated)
					{
						referenceHandle = GCHandle.Alloc(reference, GCHandleType.Normal);
					}
				}
				else if (referenceHandle.IsAllocated)
				{
					referenceHandle.Free();
				}
			}

			protected override void OnThreadException(Exception e)
			{
				Application.OnThreadException(e);
			}

			protected override void WndProc(ref Message m)
			{
				reference.WndProc(ref m);
			}
		}

		private Icon icon;

		private string text = "";

		private readonly int id;

		private bool created;

		private static int nextId;

		private readonly object syncObj = new object();

		private NotifyIconNativeWindow window;

		private bool doubleClickDown;

		private readonly MethodInfo commandDispatch;

		private DateTime lastClickTime = DateTime.MinValue;

		private readonly int doubleClickThreshold = SystemInformation.DoubleClickTime;

		private bool visible;

		private static readonly int wmTaskBarCreated = WinApiHelper.RegisterWindowMessage("TaskbarCreated");

		public string BalloonTipText { get; set; }

		public ToolTipIcon BalloonTipIcon { get; set; }

		public string BalloonTipTitle { get; set; }

		public ContextMenu ContextMenu { get; set; }

		public ContextMenuStrip ContextMenuStrip { get; set; }

		public Icon Icon
		{
			get
			{
				return icon;
			}
			set
			{
				if (icon != value)
				{
					icon = value;
					UpdateNotifyIcon(visible);
				}
			}
		}

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (value.Length > 63)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (!value.Equals(text))
				{
					text = value;
					if (visible)
					{
						UpdateNotifyIcon(visible);
					}
				}
			}
		}

		public bool Visible
		{
			get
			{
				return visible;
			}
			set
			{
				if (visible != value)
				{
					visible = value;
					UpdateNotifyIcon(visible);
				}
			}
		}

		public event EventHandler BalloonTipClicked;

		public event EventHandler BalloonTipClosed;

		public event EventHandler BalloonTipShown;

		public event EventHandler Click;

		public event EventHandler DoubleClick;

		public event MouseEventHandler MouseClick;

		public event MouseEventHandler MouseDoubleClick;

		public event MouseEventHandler MouseDown;

		public event MouseEventHandler MouseMove;

		public event MouseEventHandler MouseUp;

		public NotifyIconWindowsImplementation()
		{
			BalloonTipText = "";
			BalloonTipTitle = "";
			commandDispatch = typeof(Form).Assembly.GetType("System.Windows.Forms.Command").GetMethod("DispatchID", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(int) }, null);
			id = ++nextId;
			window = new NotifyIconNativeWindow(this);
			UpdateNotifyIcon(visible);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (window != null)
				{
					icon = null;
					text = "";
					UpdateNotifyIcon(showNotifyIcon: false);
					window.DestroyHandle();
					window = null;
					ContextMenu = null;
					ContextMenuStrip = null;
				}
			}
			else if (window != null && window.Handle != IntPtr.Zero)
			{
				WinApiHelper.PostMessage(new HandleRef(window, window.Handle), 16, 0, 0);
				window.ReleaseHandle();
			}
			base.Dispose(disposing);
		}

		public void ShowBalloonTip(int timeout)
		{
			ShowBalloonTip(timeout, BalloonTipTitle, BalloonTipText, BalloonTipIcon);
		}

		public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
		{
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			if (string.IsNullOrEmpty(tipText))
			{
				throw new ArgumentException("tipText");
			}
			if (!base.DesignMode && created)
			{
				WinApiHelper.NotifyIconData notifyIconData = new WinApiHelper.NotifyIconData();
				if (window.Handle == IntPtr.Zero)
				{
					window.CreateHandle(new CreateParams());
				}
				notifyIconData.Window = window.Handle;
				notifyIconData.ID = id;
				notifyIconData.Flags = WinApiHelper.NotifyIconDataFlags.Info;
				notifyIconData.TimeoutOrVersion = timeout;
				notifyIconData.InfoTitle = tipTitle;
				notifyIconData.Info = tipText;
				notifyIconData.InfoFlags = (int)tipIcon;
				WinApiHelper.Shell_NotifyIcon(WinApiHelper.NotifyIconMessage.Modify, notifyIconData);
			}
		}

		private void ShowContextMenu()
		{
			if (ContextMenu != null || ContextMenuStrip != null)
			{
				WinApiHelper.POINT point = default(WinApiHelper.POINT);
				WinApiHelper.GetCursorPos(ref point);
				WinApiHelper.SetForegroundWindow(new HandleRef(window, window.Handle));
				if (ContextMenu != null)
				{
					ContextMenu.GetType().InvokeMember("OnPopup", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, ContextMenu, new object[1] { EventArgs.Empty });
					WinApiHelper.TrackPopupMenuEx(new HandleRef(ContextMenu, ContextMenu.Handle), 72, point.X, point.Y, new HandleRef(window, window.Handle), IntPtr.Zero);
					WinApiHelper.PostMessage(new HandleRef(window, window.Handle), 0, 0, 0);
				}
				else
				{
					ContextMenuStrip?.GetType().InvokeMember("ShowInTaskbar", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, ContextMenuStrip, new object[2] { point.X, point.Y });
				}
			}
		}

		private void UpdateNotifyIcon(bool showNotifyIcon)
		{
			if (base.DesignMode)
			{
				return;
			}
			lock (syncObj)
			{
				window.LockReference(showNotifyIcon);
				WinApiHelper.NotifyIconData notifyIconData = new WinApiHelper.NotifyIconData
				{
					CallbackMessage = 2048,
					Flags = WinApiHelper.NotifyIconDataFlags.Message
				};
				if (showNotifyIcon && window.Handle == IntPtr.Zero)
				{
					window.CreateHandle(new CreateParams());
				}
				notifyIconData.Window = window.Handle;
				notifyIconData.ID = id;
				if (icon != null)
				{
					notifyIconData.Flags |= WinApiHelper.NotifyIconDataFlags.Icon;
					notifyIconData.Icon = icon.Handle;
				}
				notifyIconData.Flags |= WinApiHelper.NotifyIconDataFlags.Tip;
				notifyIconData.Tip = text;
				if (showNotifyIcon && icon != null)
				{
					if (!created)
					{
						if (WinApiHelper.Shell_NotifyIcon(WinApiHelper.NotifyIconMessage.Modify, notifyIconData))
						{
							created = true;
							return;
						}
						int num = 0;
						do
						{
							created = WinApiHelper.Shell_NotifyIcon(WinApiHelper.NotifyIconMessage.Add, notifyIconData);
							if (!created)
							{
								Thread.Sleep(200);
								num++;
							}
						}
						while (!created && num < 40);
					}
					else
					{
						WinApiHelper.Shell_NotifyIcon(WinApiHelper.NotifyIconMessage.Modify, notifyIconData);
					}
				}
				else
				{
					if (!created)
					{
						return;
					}
					int num2 = 0;
					bool flag;
					do
					{
						flag = WinApiHelper.Shell_NotifyIcon(WinApiHelper.NotifyIconMessage.Delete, notifyIconData);
						if (!flag)
						{
							Thread.Sleep(200);
							num2++;
						}
					}
					while (!flag && num2 < 40);
					created = false;
				}
			}
		}

		private void ProcessMouseDown(MouseButtons button, bool doubleClick)
		{
			doubleClick = false;
			if (button == MouseButtons.Left)
			{
				DateTime now = DateTime.Now;
				if ((now - lastClickTime).TotalMilliseconds <= (double)doubleClickThreshold)
				{
					lastClickTime = DateTime.MinValue;
					doubleClick = true;
				}
				else
				{
					lastClickTime = now;
				}
			}
			if (doubleClick)
			{
				this.DoubleClick?.Invoke(this, new MouseEventArgs(button, 2, 0, 0, 0));
				this.MouseDoubleClick?.Invoke(this, new MouseEventArgs(button, 2, 0, 0, 0));
				doubleClickDown = true;
			}
			this.MouseDown?.Invoke(this, new MouseEventArgs(button, (!doubleClick) ? 1 : 2, 0, 0, 0));
		}

		private void ProcessMouseUp(MouseButtons button)
		{
			this.MouseUp?.Invoke(this, new MouseEventArgs(button, 0, 0, 0, 0));
			if (!doubleClickDown)
			{
				this.Click?.Invoke(this, new MouseEventArgs(button, 0, 0, 0, 0));
				this.MouseClick?.Invoke(this, new MouseEventArgs(button, 0, 0, 0, 0));
			}
			doubleClickDown = false;
		}

		private void ProcessInitMenuPopup(ref Message message)
		{
			if (ContextMenu == null || !(bool)ContextMenu.GetType().InvokeMember("ProcessInitMenuPopup", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, ContextMenu, new object[1] { message.WParam }))
			{
				window.DefWndProc(ref message);
			}
		}

		private void WndProc(ref Message message)
		{
			switch (message.Msg)
			{
			case 2:
				UpdateNotifyIcon(showNotifyIcon: false);
				return;
			case 273:
				if (message.LParam != IntPtr.Zero)
				{
					window.DefWndProc(ref message);
					return;
				}
				commandDispatch.Invoke(null, new object[1] { message.WParam.ToInt32() & 0xFFFF });
				return;
			case 279:
				ProcessInitMenuPopup(ref message);
				return;
			case 2048:
				switch ((int)message.LParam)
				{
				case 512:
					this.MouseMove?.Invoke(this, new MouseEventArgs(Control.MouseButtons, 0, 0, 0, 0));
					break;
				case 513:
					ProcessMouseDown(MouseButtons.Left, doubleClick: false);
					break;
				case 514:
					ProcessMouseUp(MouseButtons.Left);
					break;
				case 515:
					ProcessMouseDown(MouseButtons.Left, doubleClick: true);
					break;
				case 516:
					ProcessMouseDown(MouseButtons.Right, doubleClick: false);
					break;
				case 517:
					if (ContextMenu != null || ContextMenuStrip != null)
					{
						ShowContextMenu();
					}
					ProcessMouseUp(MouseButtons.Right);
					break;
				case 518:
					ProcessMouseDown(MouseButtons.Right, doubleClick: true);
					break;
				case 519:
					ProcessMouseDown(MouseButtons.Middle, doubleClick: false);
					break;
				case 520:
					ProcessMouseUp(MouseButtons.Middle);
					break;
				case 521:
					ProcessMouseDown(MouseButtons.Middle, doubleClick: true);
					break;
				case 1026:
					this.BalloonTipShown?.Invoke(this, EventArgs.Empty);
					break;
				case 1027:
				case 1028:
					this.BalloonTipClosed?.Invoke(this, EventArgs.Empty);
					break;
				case 1029:
					this.BalloonTipClicked?.Invoke(this, EventArgs.Empty);
					break;
				}
				return;
			}
			if (message.Msg == wmTaskBarCreated)
			{
				lock (syncObj)
				{
					created = false;
				}
				UpdateNotifyIcon(visible);
			}
			window.DefWndProc(ref message);
		}
	}

	private readonly NotifyIcon genericNotifyIcon;

	private readonly NotifyIconWindowsImplementation windowsNotifyIcon;

	public string BalloonTipText
	{
		get
		{
			if (genericNotifyIcon == null)
			{
				return windowsNotifyIcon.BalloonTipText;
			}
			return genericNotifyIcon.BalloonTipText;
		}
		set
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.BalloonTipText = value;
			}
			else
			{
				windowsNotifyIcon.BalloonTipText = value;
			}
		}
	}

	public ToolTipIcon BalloonTipIcon
	{
		get
		{
			return genericNotifyIcon?.BalloonTipIcon ?? windowsNotifyIcon.BalloonTipIcon;
		}
		set
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.BalloonTipIcon = value;
			}
			else
			{
				windowsNotifyIcon.BalloonTipIcon = value;
			}
		}
	}

	public string BalloonTipTitle
	{
		get
		{
			if (genericNotifyIcon != null)
			{
				return genericNotifyIcon.BalloonTipTitle;
			}
			return windowsNotifyIcon.BalloonTipTitle;
		}
		set
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.BalloonTipTitle = value;
			}
			else
			{
				windowsNotifyIcon.BalloonTipTitle = value;
			}
		}
	}

	public ContextMenu ContextMenu
	{
		get
		{
			if (genericNotifyIcon == null)
			{
				return windowsNotifyIcon.ContextMenu;
			}
			return genericNotifyIcon.ContextMenu;
		}
		set
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.ContextMenu = value;
			}
			else
			{
				windowsNotifyIcon.ContextMenu = value;
			}
		}
	}

	public ContextMenuStrip ContextMenuStrip
	{
		get
		{
			if (genericNotifyIcon == null)
			{
				return windowsNotifyIcon.ContextMenuStrip;
			}
			return genericNotifyIcon.ContextMenuStrip;
		}
		set
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.ContextMenuStrip = value;
			}
			else
			{
				windowsNotifyIcon.ContextMenuStrip = value;
			}
		}
	}

	public object Tag { get; set; }

	public Icon Icon
	{
		get
		{
			if (genericNotifyIcon == null)
			{
				return windowsNotifyIcon.Icon;
			}
			return genericNotifyIcon.Icon;
		}
		set
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.Icon = value;
			}
			else
			{
				windowsNotifyIcon.Icon = value;
			}
		}
	}

	public string Text
	{
		get
		{
			if (genericNotifyIcon == null)
			{
				return windowsNotifyIcon.Text;
			}
			return genericNotifyIcon.Text;
		}
		set
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.Text = value;
			}
			else
			{
				windowsNotifyIcon.Text = value;
			}
		}
	}

	public bool Visible
	{
		get
		{
			return genericNotifyIcon?.Visible ?? windowsNotifyIcon.Visible;
		}
		set
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.Visible = value;
			}
			else
			{
				windowsNotifyIcon.Visible = value;
			}
		}
	}

	public event EventHandler BalloonTipClicked
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.BalloonTipClicked += value;
			}
			else
			{
				windowsNotifyIcon.BalloonTipClicked += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.BalloonTipClicked -= value;
			}
			else
			{
				windowsNotifyIcon.BalloonTipClicked -= value;
			}
		}
	}

	public event EventHandler BalloonTipClosed
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.BalloonTipClosed += value;
			}
			else
			{
				windowsNotifyIcon.BalloonTipClosed += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.BalloonTipClosed -= value;
			}
			else
			{
				windowsNotifyIcon.BalloonTipClosed -= value;
			}
		}
	}

	public event EventHandler BalloonTipShown
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.BalloonTipShown += value;
			}
			else
			{
				windowsNotifyIcon.BalloonTipShown += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.BalloonTipShown -= value;
			}
			else
			{
				windowsNotifyIcon.BalloonTipShown -= value;
			}
		}
	}

	public event EventHandler Click
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.Click += value;
			}
			else
			{
				windowsNotifyIcon.Click += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.Click -= value;
			}
			else
			{
				windowsNotifyIcon.Click -= value;
			}
		}
	}

	public event EventHandler DoubleClick
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.DoubleClick += value;
			}
			else
			{
				windowsNotifyIcon.DoubleClick += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.DoubleClick -= value;
			}
			else
			{
				windowsNotifyIcon.DoubleClick -= value;
			}
		}
	}

	public event MouseEventHandler MouseClick
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseClick += value;
			}
			else
			{
				windowsNotifyIcon.MouseClick += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseClick -= value;
			}
			else
			{
				windowsNotifyIcon.MouseClick -= value;
			}
		}
	}

	public event MouseEventHandler MouseDoubleClick
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseDoubleClick += value;
			}
			else
			{
				windowsNotifyIcon.MouseDoubleClick += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseDoubleClick -= value;
			}
			else
			{
				windowsNotifyIcon.MouseDoubleClick -= value;
			}
		}
	}

	public event MouseEventHandler MouseDown
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseDown += value;
			}
			else
			{
				windowsNotifyIcon.MouseDown += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseDown -= value;
			}
			else
			{
				windowsNotifyIcon.MouseDown -= value;
			}
		}
	}

	public event MouseEventHandler MouseMove
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseMove += value;
			}
			else
			{
				windowsNotifyIcon.MouseMove += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseMove -= value;
			}
			else
			{
				windowsNotifyIcon.MouseMove -= value;
			}
		}
	}

	public event MouseEventHandler MouseUp
	{
		add
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseUp += value;
			}
			else
			{
				windowsNotifyIcon.MouseUp += value;
			}
		}
		remove
		{
			if (genericNotifyIcon != null)
			{
				genericNotifyIcon.MouseUp -= value;
			}
			else
			{
				windowsNotifyIcon.MouseUp -= value;
			}
		}
	}

	public NotifyIconAdv()
	{
		if (OSHelper.IsUnix)
		{
			genericNotifyIcon = new NotifyIcon();
		}
		else
		{
			windowsNotifyIcon = new NotifyIconWindowsImplementation();
		}
	}

	public void Dispose()
	{
		if (genericNotifyIcon != null)
		{
			genericNotifyIcon.Dispose();
		}
		else
		{
			windowsNotifyIcon.Dispose();
		}
	}

	public void ShowBalloonTip(int timeout)
	{
		ShowBalloonTip(timeout, BalloonTipTitle, BalloonTipText, BalloonTipIcon);
	}

	public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
	{
		if (genericNotifyIcon != null)
		{
			genericNotifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
		}
		else
		{
			windowsNotifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
		}
	}
}
