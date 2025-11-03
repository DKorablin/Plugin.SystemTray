using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Plugin.SystemTray
{
	internal class FormExpandCollapseCtrl : IDisposable
	{
		private readonly PluginWindows _plugin;
		private NotifyIcon _icon;
		private Form _form;
		private ToolStripMenuItem _menuMinimize;
		private ToolStripMenuItem _menuRestore;

		private Form MainForm
		{
			get
			{
				if(this._form == null || this._form.IsDisposed)
				{
					this._form = this._plugin.Host.Object as Form;
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
					this._icon.DoubleClick += new EventHandler(this.icon_DoubleClick);
					this._icon.Visible = true;

					ContextMenuStrip menu = new ContextMenuStrip();
					this._menuMinimize = new ToolStripMenuItem("Minimize");
					this._menuRestore = new ToolStripMenuItem("Restore");
					menu.Items.AddRange(new ToolStripMenuItem[] { this._menuMinimize, this._menuRestore, });

					this._menuRestore.Click += new EventHandler(this.menuItem_Click);
					this._menuMinimize.Click += new EventHandler(this.menuItem_Click);
					menu.Opening += this.Menu_Opening;
					this._icon.ContextMenuStrip = menu;
				}
				return this._icon;
			}
		}

		public FormExpandCollapseCtrl(PluginWindows plugin)
		{
			this._plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));

			if(this.MainForm == null)
				throw new InvalidOperationException($"{this._plugin} requires MainWindowHandle and Windows environment");

			if(this._plugin.Settings.Enable)
				this.CreateIcon();
		}

		public void RemoveIcon()
		{
			if(this.MainForm.WindowState == FormWindowState.Minimized)
			{
				this.MainForm.Show();
				this.MainForm.Focus();
				this.MainForm.WindowState = FormWindowState.Normal;
			}

			this.MainForm.Shown -= new EventHandler(this.MainForm_Shown);
			this.MainForm.Resize -= new EventHandler(this.MainForm_Resize);

			//Removing icon
			this._icon?.Dispose();
			this._icon = null;
		}

		public void CreateIcon()
		{
			if(this.MainForm.WindowState == FormWindowState.Minimized)
				this.MainForm.Hide();

			//Creating icon
			this.MainForm_Shown(this, EventArgs.Empty);
			this.MainForm.Shown += new EventHandler(this.MainForm_Shown);
			this.MainForm.Resize += new EventHandler(this.MainForm_Resize);
		}

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

		private void Menu_Opening(Object sender, System.ComponentModel.CancelEventArgs e)
		{
			switch(this.MainForm.WindowState)
			{
			case FormWindowState.Minimized:
				this._menuMinimize.Enabled = false;
				this._menuRestore.Enabled = true;
				break;
			case FormWindowState.Maximized:
			case FormWindowState.Normal:
				this._menuMinimize.Enabled = true;
				this._menuRestore.Enabled = false;
				break;
			}
		}

		private void menuItem_Click(Object sender, EventArgs e)
			=> this.icon_DoubleClick(sender, e);

		public void Dispose()
			=> this._icon?.Dispose();
	}
}