using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace ReactiveDrawWpf
{
	public static class UIElementExtensions
	{
		public static IObservable<KeyEventArgs> GetKeyDown(this UIElement el)
		{
			var allKeyDowns = Observable.FromEvent<KeyEventHandler, KeyEventArgs>(
				proc => new KeyEventHandler((s, e) => proc.Invoke(e)),
				h => el.KeyDown += h,
				h => el.KeyDown -= h);

			return allKeyDowns;
		}


		public static IObservable<MouseButtonEventArgs> GetMouseDown(this UIElement el)
		{
			var mdEvents = Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
				proc => new MouseButtonEventHandler((s, e) => proc.Invoke(e)),
				h => el.MouseDown += h,
				h => el.MouseDown -= h);

			return mdEvents;
		}


		public static IObservable<MouseButtonEventArgs> GetMouseUp(this UIElement el)
		{
			var muEvents = Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
				proc => new MouseButtonEventHandler((s, e) => proc.Invoke(e)),
				h => el.MouseUp += h,
				h => el.MouseUp -= h);

			return muEvents;
		}


		public static IObservable<MouseEventArgs> GetMouseMove(this UIElement el)
		{
			var mmEvents = Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
				proc => new MouseEventHandler((s, e) => proc.Invoke(e)),
				h => el.MouseMove += h,
				h => el.MouseMove -= h);

			return mmEvents;
		}
	}
}
