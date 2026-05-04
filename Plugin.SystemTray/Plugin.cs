using System;
using System.Diagnostics;
using SAL.Flatbed;

namespace Plugin.SystemTray
{
	public class PluginWindows : IPlugin, IPluginSettings<PluginSettings>
	{
		private PluginSettings _settings;
		private FormExpandCollapseCtrl _ctrl;

		internal IHost Host { get; }

		/// <summary>Settings for interaction from the host</summary>
		Object IPluginSettings.Settings => this.Settings;

		/// <summary>Settings for interaction from the plugin</summary>
		public PluginSettings Settings
		{
			get
			{
				if(this._settings == null)
				{
					this._settings = new PluginSettings();
					this.Host.Plugins.Settings(this).LoadAssemblyParameters(this._settings);
					this._settings.PropertyChanged += this.Settings_PropertyChanged;
				}
				return this._settings;
			}
		}

		internal ITraceSource Trace { get; }

		public PluginWindows(IHost host, ITraceSource trace)
		{
			this.Host = host ?? throw new ArgumentNullException(nameof(host));
			this.Trace = trace ?? throw new ArgumentNullException(nameof(trace));
		}

		private void Settings_PropertyChanged(Object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
			case nameof(PluginSettings.Enable):
				if(this.Settings.Enable)
					this._ctrl.CreateIcon();
				else
					this._ctrl.RemoveIcon();
				break;
			}
		}

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			try
			{
				this._ctrl = new FormExpandCollapseCtrl(this);
			}catch(InvalidOperationException exc)
			{
				this.Trace.TraceEvent(TraceEventType.Error, 10, exc.Message);
				return false;
			}
			return true;
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			switch(mode)
			{
			case DisconnectMode.UserClosed:
				this._ctrl?.RemoveIcon();
				break;
			default:
				this._ctrl?.Dispose();
				break;
			}
			return true;
		}
	}
}