using System;
using System.Reactive.Linq;
using System.Threading;


namespace ReactiveLab
{
	class BackgroundObservable
	{
		public static void Run()
		{
			Console.WriteLine("Shows use of Start to start on a background thread. Main thread {0}.",
				Thread.CurrentThread.ManagedThreadId);

			var o = Observable.Start(() =>
			{
				//This starts on a background thread.
				Console.WriteLine("From background thread {0}. Does not block main thread.", Thread.CurrentThread.ManagedThreadId);
				Thread.Sleep(500);
				Console.WriteLine("Background work completed.");
			}).Finally(() =>
				Console.WriteLine("Thread {0} completed.", Thread.CurrentThread.ManagedThreadId)
			);

			Console.WriteLine("Main thread {0} continues...", Thread.CurrentThread.ManagedThreadId);

			o.Wait();   // Wait for completion of background operation.
		}
	}
}
