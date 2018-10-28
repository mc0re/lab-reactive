using System;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveLab
{
	internal class TimeObservable
	{
		internal static void Run()
		{
			var ticker = Observable.Interval(TimeSpan.FromMilliseconds(100));

			// Active unsubscribe
			using (ticker.Subscribe(t => Console.Write(".")))
			{
				Thread.Sleep(1000);
				Console.WriteLine();
			}

			// Using Where
			var timeToStop = new ManualResetEvent(false);
			(from n in ticker where n % 2 == 0 select n)
				.Take(5)
				.Subscribe(t => Console.Write("."), () => timeToStop.Set());
			timeToStop.WaitOne();
			Console.WriteLine();
		}
	}
}
