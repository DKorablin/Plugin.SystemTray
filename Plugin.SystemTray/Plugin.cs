using System;
using System.Diagnostics;
using System.Windows.Forms;
using SAL.Flatbed;

namespace Plugin.SystemTray
{
	public class PluginWindows : IPlugin
	{
		private readonly IHost _host;
		private TraceSource _trace;
		private NotifyIcon _icon;
		private Form _form;
		private MenuItem _menuMinimize;
		private MenuItem _menuRestore;

		private Form MainForm
		{
			get
			{
				if(this._form == null || this._form.IsDisposed)
				{
					this._form = this._host.Object as Form;
					if(this._form == null)
					{
						IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;
						if(hwnd != IntPtr.Zero)
							this._form = (Form)Control.FromHandle(hwnd);
					}
				}
				return this._form;
			}
		}

		private NotifyIcon Icon
		{
			get
			{
				if(this._icon == null)
				{
					this._icon = new NotifyIcon();
					this._icon.DoubleClick += new EventHandler(icon_DoubleClick);
					this._icon.Visible = true;

					ContextMenu menu = new ContextMenu();
					this._menuMinimize=new MenuItem("Minimize");
					this._menuRestore=new MenuItem("Restore");
					menu.MenuItems.AddRange(new MenuItem[] { this._menuMinimize, this._menuRestore, });

					this._menuRestore.Click += new EventHandler(menuItem_Click);
					this._menuMinimize.Click += new EventHandler(menuItem_Click);
					menu.Popup += new EventHandler(menu_Popup);
					this._icon.ContextMenu = menu;
				}
				return this._icon;
			}
		}

		internal TraceSource Trace => this._trace ?? (this._trace = PluginWindows.CreateTraceSource<PluginWindows>());

		private void icon_DoubleClick(Object sender, EventArgs e)
		{
			if(this.MainForm.WindowState == FormWindowState.Normal)
				this.MainForm.WindowState = FormWindowState.Minimized;
			else
			{
				this.MainForm.Show();
				this.MainForm.Focus();
				this.MainForm.WindowState = FormWindowState.Normal;
			}
		}

		private void MainForm_Shown(Object sender, EventArgs e)
		{
			this.Icon.Text = this.MainForm.Text;
			this.Icon.Icon = this.MainForm.Icon;
		}

		private void MainForm_Resize(Object sender, EventArgs e)
		{
			switch(this.MainForm.WindowState)
			{
				case FormWindowState.Minimized:
					this.MainForm.Hide();
					break;
			}
		}
		private void menu_Popup(Object sender, EventArgs e)
		{
			switch(this.MainForm.WindowState)
			{
				case FormWindowState.Minimized:
					this.Icon.ContextMenu.MenuItems[0].Enabled = false;
					this.Icon.ContextMenu.MenuItems[1].Enabled = true;
					break;
				case FormWindowState.Maximized:
				case FormWindowState.Normal:
					this.Icon.ContextMenu.MenuItems[0].Enabled = true;
					this.Icon.ContextMenu.MenuItems[1].Enabled = false;
					break;
			}
		}
		private void menuItem_Click(Object sender, EventArgs e)
			=> this.icon_DoubleClick(sender, e);

		public PluginWindows(IHost host)
			=> this._host = host ?? throw new ArgumentNullException(nameof(host));

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			if(this.MainForm == null)
			{
				this.Trace.TraceEvent(TraceEventType.Error, 10, "{0} requires MainWindowHandle and Windows environment", this);
				return false;
			} else
			{
				this.Icon.Text = this.MainForm.Text;
				this.Icon.Icon = this.MainForm.Icon;
				this.MainForm.Shown += new EventHandler(MainForm_Shown);
				this.MainForm.Resize += new EventHandler(MainForm_Resize);
				return true;
			}
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			this._icon?.Dispose();

			return true;
		}

		private static TraceSource CreateTraceSource<T>(String name = null) where T : IPlugin
		{
			TraceSource result = new TraceSource(typeof(T).Assembly.GetName().Name + name);
			result.Switch.Level = SourceLevels.All;
			result.Listeners.Remove("Default");
			result.Listeners.AddRange(System.Diagnostics.Trace.Listeners);
			return result;
		}
	}
}