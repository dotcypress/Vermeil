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
		private readonly object _syncRoot = new object();
		private readonly List<TaskHolder> _tokens = new List<TaskHolder>();

		#region Public members

		public void ShowIndeterminate(string message, string token)
		{
			if (Deployment.Current.Dispatcher.CheckAccess())
			{
				ShowIndeterminateInternal(message, token);
			}
			else
			{
				Deployment.Current.Dispatcher.BeginInvoke(() => ShowIndeterminateInternal(message, token));
			}
		}

		public void ShowProgress(double progress, string message, string token)
		{
			if (Deployment.Current.Dispatcher.CheckAccess())
			{
				ShowProgressInternal(progress, message, token);
			}
			else
			{
				Deployment.Current.Dispatcher.BeginInvoke(() => ShowProgressInternal(progress, message, token));
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

		#region Properties

		private bool IsBusy { get; set; }

		private string Text { get; set; }

		private double Progress { get; set; }

		#endregion

		#region Private members

		private void ShowIndeterminateInternal(string message, string token)
		{
			if (token == null)
			{
				throw new ArgumentException("Token is null");
			}
			lock (_syncRoot)
			{
				if (_tokens.All(x => x.Token != token))
				{
					_tokens.Add(new TaskHolder
					{
						Token = token,
						Message = message,
						Progress = -1
					});
				}
				Update();
			}
		}

		private void ShowProgressInternal(double progress, string message, string token)
		{
			if (token == null)
			{
				throw new ArgumentException("Token is null");
			}
			lock (_syncRoot)
			{
				var existing = _tokens.FirstOrDefault(x => x.Token == token);
				if (existing == null)
				{
					_tokens.Add(new TaskHolder
					{
						Token = token,
						Message = message,
						Progress = progress
					});
				}
				else
				{
					existing.Message = message;
					existing.Progress = progress;
				}
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
				var existing = _tokens.FirstOrDefault(x => x.Token == token);
				if (existing != null)
				{
					_tokens.Remove(existing);
				}
				Update();
			}
		}

		internal void Update()
		{
			var last = _tokens.LastOrDefault();
			if (last != null)
			{
				Text = last.Message;
				Progress = last.Progress;
				IsBusy = true;
			}
			else
			{
				Text = null;
				IsBusy = false;
				Progress = 0;
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
			            Do(x => x.IsVisible = IsBusy).
			            Do(x => x.Text = Text).
			            Do(x => x.IsIndeterminate = Progress < 0).
			            Do(x => x.Value = Progress);
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
