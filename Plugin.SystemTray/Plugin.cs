using System;
using System.Diagnostics;
using SAL.Flatbed;

namespace Plugin.SystemTray
{
	public class PluginWindows : IPlugin, IPluginSettings<PluginSettings>
	{
		private PluginSettings _settings;
		private TraceSource _trace;
		private FormExpandCollapseCtrl _ctrl;

		internal IHost Host { get; }

		/// <summary>Настройки для взаимодействия из хоста</summary>
		Object IPluginSettings.Settings => this.Settings;

		/// <summary>Настройки для взаимодействия из плагина</summary>
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

		internal TraceSource Trace => this._trace ?? (this._trace = PluginWindows.CreateTraceSource<PluginWindows>());

		public PluginWindows(IHost host)
			=> this.Host = host ?? throw new ArgumentNullException(nameof(host));

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