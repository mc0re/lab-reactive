using System;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveLab
{
	internal class TimeIntervalObservable
	{
		internal static void Run()
		{
			var observable = Observable.Interval(TimeSpan.FromMilliseconds(750)).TimeInterval();

			using (observable.Subscribe(
				x => Console.WriteLine("{0}: {1}", x.Value, x.Interval)))
			{
				Thread.Sleep(3000);
			}
		}
	}
}