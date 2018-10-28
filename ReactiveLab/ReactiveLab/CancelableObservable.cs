using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveLab
{
	internal class CancelableObservable
	{
		internal static void Run()
		{
			var cts = new CancellationTokenSource();

			var subscription =
				Observable.Create<int>(o =>
				{
					var cancel = new CancellationDisposable(cts);

					NewThreadScheduler.Default.Schedule(() =>
					{
						int i = 0;
						for (; ; )
						{
							Thread.Sleep(200);  // here we do the long lasting background operation
							if (!cancel.Token.IsCancellationRequested)    // check cancel token periodically
							{
								o.OnNext(i++);
							}
							else
							{
								Console.WriteLine("Aborting because cancel event was signaled!");
								o.OnCompleted(); // will not make it to the subscriber
								return;
							}
						}
					});

					return cancel;
				})
				.Subscribe(i =>
					Console.Write("{0} ", i)
				);

			Thread.Sleep(new Random().Next(1500, 3000));
			cts.Cancel();
			subscription.Dispose();

			// give background thread chance to write the cancel acknowledge message
			Thread.Sleep(100);
		}
	}
}
