using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveLab
{
	internal class BufferedObservable
	{
		private static IEnumerable<string> EndlessBarrageOfEmail(CancellationToken token)
		{
			Console.WriteLine("Sending emails on thread {0}", Thread.CurrentThread.ManagedThreadId);
			var random = new Random();
			var emails = new List<string> { "Email-1", "Email-2", "Email-3" };
			for (; ; )
			{
				// Return some random emails at random intervals.
				yield return emails[random.Next(emails.Count)];

				if (token.IsCancellationRequested)
				{
					yield break;
				}

				// This Sleep prevents the "using Subscribe" to get into its body.
				Thread.Sleep(random.Next(500));
			}
		}


		internal static void Run()
		{
			var cts = new CancellationTokenSource();
			var ev = new ManualResetEvent(false);

			Console.WriteLine("Start and wait, thread {0}", Thread.CurrentThread.ManagedThreadId);

			NewThreadScheduler.Default.Schedule(() =>
			{
				var myInbox = EndlessBarrageOfEmail(cts.Token).ToObservable();
				var getMailEveryTwoSeconds = myInbox.Buffer(TimeSpan.FromSeconds(2));

				Console.WriteLine("Subscribed to buffer, thread {0}", Thread.CurrentThread.ManagedThreadId);

				using (getMailEveryTwoSeconds.Subscribe(emails =>
					{
						Console.WriteLine("You've got {0} new messages, thread {1}", emails.Count(), Thread.CurrentThread.ManagedThreadId);
						Console.WriteLine(string.Join(", ", emails));
					}))
				{
					ev.WaitOne();
					Console.WriteLine("Unsubscribing, thread {0}", Thread.CurrentThread.ManagedThreadId);
				}
			});

			Thread.Sleep(6100);
			cts.Cancel();
			ev.Set();

			// Let it finish
			Thread.Sleep(200);
		}
	}
}
