#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Vermeil.Core;

#endregion

namespace Vermeil.Notify
{
	internal class ProgressIndicatorService : IProgressIndicatorService
	{
		private readonly List<TaskHolder> _holders = new List<TaskHolder>();
		private readonly object _syncRoot = new object();

		#region Public members

		public void ShowIndeterminate(string message, string token)
		{
			if (Deployment.Current.Dispatcher.CheckAccess())
			{
				UpdateHolder(-1, message, token);
			}
			else
			{
				Deployment.Current.Dispatcher.BeginInvoke(() => UpdateHolder(-1, message, token));
			}
		}

		public void ShowProgress(double progress, string message, string token)
		{
			if (Deployment.Current.Dispatcher.CheckAccess())
			{
				UpdateHolder(progress, message, token);
			}
			else
			{
				Deployment.Current.Dispatcher.BeginInvoke(() => UpdateHolder(progress, message, token));
			}
		}

		public void Hide(string token)
		{
			if (Deployment.Current.Dispatcher.CheckAccess())
			{
				HideInternal(token);
			}
			else
			{
				Deployment.Current.Dispatcher.BeginInvoke(() => HideInternal(token));
			}
		}

		#endregion

		#region Private members

		private void UpdateHolder(double progress, string message, string token)
		{
			lock (_syncRoot)
			{
				var existing = _holders.FirstOrDefault(x => x.Token == token);
				if (existing == null)
				{
					existing = new TaskHolder
					{
						Token = token,
						Message = message,
						Progress = progress
					};
				}
				else
				{
					existing.Message = message;
					existing.Progress = progress;
					_holders.Remove(existing);
				}
				_holders.Add(existing);
				Update();
			}
		}

		private void HideInternal(string token)
		{
			if (token == null)
			{
				throw new ArgumentException("Token is null");
			}
			lock (_syncRoot)
			{
				var existing = _holders.FirstOrDefault(x => x.Token == token);
				if (existing != null)
				{
					_holders.Remove(existing);
				}
				Update();
			}
		}

		internal void Update()
		{
			var text = "";
			var isBusy = false;
			var progress = -1d;
			var last = _holders.LastOrDefault();
			if (last != null)
			{
				text = last.Message;
				progress = last.Progress;
				isBusy = true;
			}

			Application.Current.RootVisual.
			            With(x => x as PhoneApplicationFrame).
			            With(x => x.Content as PhoneApplicationPage).
			            With(x =>
				            {
					            if (SystemTray.GetProgressIndicator(x) == null)
					            {
						            SystemTray.SetProgressIndicator(x, new ProgressIndicator());
					            }
					            return SystemTray.GetProgressIndicator(x);
				            }).
			            Do(x => x.IsVisible = isBusy).
			            Do(x => x.Text = text).
			            Do(x => x.IsIndeterminate = progress < 0).
			            Do(x => x.Value = progress);
		}

		#endregion

		private class TaskHolder
		{
			public string Token { get; set; }
			public string Message { get; set; }
			public double Progress { get; set; }
		}
	}
}
