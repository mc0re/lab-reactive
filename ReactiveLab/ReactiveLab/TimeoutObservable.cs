using System;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveLab
{
	internal class TimeoutObservable
	{
		internal static void Run()
		{
			Console.WriteLine(DateTime.Now);
			var ev = new ManualResetEvent(false);

			// create a single event in 3 seconds time
			var observable = Observable.Timer(TimeSpan.FromSeconds(3)).Timestamp();

			// raise exception if no event received within 2 seconds
			var observableWithTimeout = Observable.Timeout(observable, TimeSpan.FromSeconds(2));

			using (observableWithTimeout.Subscribe(
				x => { Console.WriteLine("{0}: {1}", x.Value, x.Timestamp); ev.Set(); },
				ex => { Console.WriteLine("{0} {1}", ex.Message, DateTime.Now); ev.Set(); }))
			{
				ev.WaitOne();
			}
		}
	}
}
