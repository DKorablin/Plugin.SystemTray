using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Plugin.SystemTray
{
	public class PluginSettings : INotifyPropertyChanged
	{
		private Boolean _isEnabled = true;

		[Category("Automation")]
		[Description("Collapse application to system tray")]
		[DefaultValue(true)]
		public Boolean Enable
		{
			get => this._isEnabled;
			set => this.SetField(ref this._isEnabled, value, nameof(this.Enable));
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		private Boolean SetField<T>(ref T field, T value, String propertyName)
		{
			if(EqualityComparer<T>.Default.Equals(field, value))
				return false;

			field = value;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
		#endregion INotifyPropertyChanged
	}
}