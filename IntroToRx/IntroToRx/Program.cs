using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;

namespace IntroToRx
{
	internal class Program
	{
		private static readonly List<Expression<Action>> sActionTable = new List<Expression<Action>>
		{
			() => ReplaySubjectBufferExample(),
			() => ReplaySubjectWindowExample(),
			() => BehaviorSubjectCompletedExample(),
			() => AsyncSubjectCompleted(),
			() => PublishAfterCompletedExample(),
			() => GenerateExample(),
			() => StartFuncExample(),
			() => SkipUntilExample(),
			() => TrivialAggregationExample(),
			() => MinByExample(),
			() => GroupByExample(),
			() => LinqLanguageExample(),
			() => MaterializeExample(),
			() => SelectManyExample(),
			() => ForEachExample(),
			() => ToEnumerableExample(),
			() => ToListExample(),
			() => ToTaskExample(),
			() => ToEventExample(),
			() => CatchExample(),
			() => UsingExample(),
			() => RetryExample(),
			() => RepeatExample(),
			() => AmbiguousExample(),
			() => SwitchExample(),
			() => AndThenExample(),
			() => SampleExample(),
			() => PublishExample(),
			() => RefCountExample(),
			() => SubscribeOnExample(),
			() => FileStreamExample(),
			() => SchedulerExample(),
			() => TestSchedulerExample(),
			() => TestStartExample(),
			() => CreateColdObservableExample(),
			() => CreateHotObservableExample(),
			() => WindowFromCountExample(),
			() => WindowFromSelectorExample(),
			() => MyWindowExample(),
			() => MyCombineLatestExample(),
			() => GroupJoinExample()
		};


		private static void Main(string[] args)
		{
			for (var n = 1; n <= sActionTable.Count; n++)
				ExecuteMethod(n);

			System.Console.WriteLine("\n*** All done");
		}


		private static void ExecuteMethod(int n)
		{
			var m = sActionTable[n - 1];
			var name = (m.Body as MethodCallExpression).Method.Name;
			Console.WriteLine("\n*** {0}", name);
			m.Compile().Invoke();
		}


		public static void ReplaySubjectBufferExample()
		{
			var bufferSize = 2; /* Only b and c are cached before Subscribe */
			var subject = new ReplaySubject<string>(bufferSize);
			subject.OnNext("a");
			subject.OnNext("b");
			subject.OnNext("c");
			subject.Subscribe(Console.WriteLine);
			subject.OnNext("d");
		}


		public static void ReplaySubjectWindowExample()
		{
			var window = TimeSpan.FromMilliseconds(150); /* Only x and y are cacehd before Subscribe */
			var subject = new ReplaySubject<string>(window);
			subject.OnNext("w");
			Thread.Sleep(TimeSpan.FromMilliseconds(100));
			subject.OnNext("x");
			Thread.Sleep(TimeSpan.FromMilliseconds(100));
			subject.OnNext("y");
			subject.Subscribe(Console.WriteLine);
			subject.OnNext("z");
		}


		public static void BehaviorSubjectCompletedExample()
		{
			var subject = new BehaviorSubject<string>("a");
			subject.Subscribe(v => Console.WriteLine($"1: {v}"));
			subject.OnNext("b");
			subject.Subscribe(v => Console.WriteLine($"2: {v}"));
			subject.OnNext("c");
			subject.OnCompleted();
		}


		public static void AsyncSubjectCompleted()
		{
			var subject = new AsyncSubject<string>();
			subject.OnNext("a");
			subject.Subscribe(Console.WriteLine);
			subject.OnNext("b");
			subject.OnNext("c");
			subject.OnCompleted();
		}


		public static void PublishAfterCompletedExample()
		{
			var subject = new Subject<string>();
			subject.Subscribe(Console.WriteLine);
			subject.OnNext("a");
			subject.OnNext("b");
			subject.OnCompleted();
			subject.OnNext("c");
		}


		public static void GenerateExample()
		{
			var ev = new ManualResetEvent(false);
			var obs = Observable.Generate(
				'a', x => x <= 'z', x => (char) (x + 1),
				x => x,
				x => TimeSpan.FromSeconds(0.1 + (x - 'a') * 0.02));
			using (obs.Subscribe(c => Console.Write("{0} ", c), () => ev.Set()))
			{
				ev.WaitOne();
			}

			Console.WriteLine();
		}


		public static void StartFuncExample()
		{
			var ev = new ManualResetEvent(false);
			var start = Observable.Start(() =>
			{
				Console.Write("Working away");
				for (int i = 0; i < 10; i++)
				{
					Thread.Sleep(100);
					Console.Write(".");
				}
				return "Published value";
			});
			start.Subscribe(
				Console.WriteLine,
				() => { Console.WriteLine("Action completed"); ev.Set(); });
			ev.WaitOne();

		}


		public static void SkipUntilExample()
		{
			var subject = new Subject<int>();
			var otherSubject = new Subject<Unit>();
			subject
				.SkipUntil(otherSubject)
				.Subscribe(Console.WriteLine, () => Console.WriteLine("Completed"));
			subject.OnNext(1);
			subject.OnNext(2);
			subject.OnNext(3);
			otherSubject.OnNext(Unit.Default);
			otherSubject.OnCompleted();
			subject.OnNext(4);
			subject.OnNext(5);
			subject.OnNext(6);
			subject.OnNext(7);
			subject.OnNext(8);
			subject.OnCompleted();
		}


		public static void TrivialAggregationExample()
		{
			var numbers = new Subject<int>();
			var t1 = numbers.DumpAsync("numbers");
			var t2 = numbers.Min().DumpAsync("Min");
			var t3 = numbers.Sum().DumpAsync("Sum");
			var t4 = numbers.Count().DumpAsync("Count");
			var t5 = numbers.Average().DumpAsync("Average");
			
			numbers.OnNext(1);
			numbers.OnNext(2);
			numbers.OnNext(3);
			numbers.OnCompleted();

			Task.WaitAll(t1, t2, t3, t4, t5);
		}


		public static void MinByExample()
		{
			var source = Observable.Interval(TimeSpan.FromSeconds(0.1)).Take(10);
			var minBy = source.MinBy(n => n % 3);
			var ev = new ManualResetEvent(false);

			using (minBy.Subscribe(ml => Console.WriteLine(string.Join(", ", ml)), () => ev.Set()))
			{
				ev.WaitOne();
			}
		}


		public static void GroupByExample()
		{
			var source = Observable.Interval(TimeSpan.FromSeconds(0.1)).Take(10);
			var group = source.GroupBy(i => i % 3);
			var ev = new ManualResetEvent(false);

			using (group.Subscribe(
				grp => grp.Min().Subscribe(
					minValue => Console.WriteLine("{0} min value = {1}", grp.Key, minValue)),
				() => ev.Set()))
			{
				ev.WaitOne();
			}

			ev.Reset();

			using (group.SelectMany(
				grp => grp.Max().Select(value => new { grp.Key, value })).
					Subscribe(
						a => Console.WriteLine("{0} max value = {1}", a.Key, a.value),
						() => ev.Set()))
			{
				ev.WaitOne();
			}
		}


		public static void LinqLanguageExample()
		{
			var query = from i in Observable.Range(1, 5)
						where i % 2 == 0
						select new { Number = i, Character = (char) (i + 64) };
			query.Dump("anon");
		}


		public static void MaterializeExample()
		{
			var source = new Subject<int>();
			var seq = source.Materialize();
			var t1 = seq.DumpAsync("Materialize");
			var t2 = seq.Dematerialize().DumpAsync("Dematerialize");

			source.OnNext(1);
			source.OnNext(2);
			source.OnNext(3);
			source.OnError(new Exception("Fail?"));

			Task.WaitAll(t1, t2);
		}


		public static void SelectManyExample()
		{
			Observable.Range(1, 3)
				.SelectMany(i => Observable.Range(1, i))
				.Dump("SelectMany");

			IObservable<long> GetSubValues(int offset)
			{
				//Produce values [x*10, (x*10)+1, (x*10)+2] 0.4 seconds apart, but starting immediately.
				return Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.4))
					.Select(x => (offset * 10) + x)
					.Take(3);
			}

			var query = from i in Observable.Range(1, 5)
						where i % 2 == 0
						from j in GetSubValues(i)
						select new { i, j };
			query.Dump("SelectMany");
		}


		public static void ForEachExample()
		{
			var source = Observable.Interval(TimeSpan.FromSeconds(0.2)).Take(5);
			source.Do(i => Console.WriteLine("received {0} @ {1}", i, DateTime.Now)).Wait();
			Console.WriteLine("completed @ {0}", DateTime.Now);
		}


		public static void ToEnumerableExample()
		{
			var source = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(200)).
				Take(5).
				Do(n => { if (n == 4) throw new ArgumentException("Improper value 4"); });

			var result = source.ToEnumerable();

			try
			{
				foreach (var value in result)
				{
					Console.WriteLine(value);
				}
			}
			catch (ArgumentException aex)
			{
				Console.WriteLine(aex.Message);
			}

			Console.WriteLine("done");
		}


		public static void ToListExample()
		{
			var source = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(200)).
				Take(5).
				Do(n => { if (n == 4) throw new ArgumentException("Improper value 4"); }).
				ToList();

			source.Dump("List");
		}


		public static void ToTaskExample()
		{
			var source = Observable.Interval(TimeSpan.FromSeconds(0.2)).Take(5);
			var task = source.ToTask();
			Console.WriteLine("Awaiting task...");
			// Only the last value
			Console.WriteLine(task.Result);
		}


		public static void ToEventExample()
		{
			var source = Observable.Interval(TimeSpan.FromSeconds(0.2)).Take(5);
			var result = source.ToEvent();
			result.OnNext += val => Console.WriteLine(val);
			Task.Delay(1500).Wait();
		}


		public static void CatchExample()
		{
			var source = new Subject<int>();
			var result = source.
				Catch<int, TimeoutException>(tx => Observable.Return(-1)).
				Finally(() => Console.WriteLine("Finally action ran"));
			var dump = result.DumpAsync("Catch");
			source.OnNext(1);
			source.OnNext(2);
			source.OnError(new TimeoutException());
			dump.Wait();
		}


		public static void UsingExample()
		{
			var source = Observable.Interval(TimeSpan.FromSeconds(0.2));
			var result = Observable.Using(
				() => new TimeIt("Subscription Timer"),
				_ => source);
			result.Take(5).Dump("Using");
		}


		public static void RetryExample()
		{
			var source = Observable.Interval(TimeSpan.FromSeconds(0.2)).
				Take(4).
				Do(n => { if (n == 3) throw new IndexOutOfRangeException(); });

			Console.WriteLine("Standard Retry");
			source.Retry(2).Dump("Retry");
			Console.WriteLine("MyRetry, normal case");
			source.MyRetry(TimeSpan.FromSeconds(2), 2, ex => ex is IndexOutOfRangeException).Dump("MyRetry");
			Console.WriteLine("MyRetry, wrong exception");
			source.MyRetry(TimeSpan.FromSeconds(2), 2, ex => ex is ArgumentException).Dump("MyRetry");
		}


		public static void RepeatExample()
		{
			var source = Observable.Range(0, 3).Repeat().Take(11);
			source.Dump("Repeat");
		}


		public static void AmbiguousExample()
		{
			var s1 = new Subject<int>();
			var s2 = new Subject<int>();
			var s3 = new Subject<int>();

			var result = Observable.Amb(s1, s2, s3);
			var dumper = result.DumpAsync("Amb");

			s1.OnNext(100);
			s2.OnNext(200);
			s3.OnNext(300);
			s1.OnNext(101);
			s2.OnNext(201);
			s3.OnNext(301);
			s1.OnCompleted();
			s2.OnCompleted();
			s3.OnCompleted();
			dumper.Wait();
		}


		public static void SwitchExample()
		{
			// 0.1, 0.3, 0.5, 0.7, 0.9, 1.1, 1.3, 1.5
			var s1 = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.2)).
				Select(n => "1-" + (n + 1)).Take(8);

			// 0.4, 0.8, 1.2, 1.6, 2.0
			var s2 = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.4)).
				Select(n => "2-" + (n + 1)).Take(5);

			// 1.0, 1.3, 1.6, 1.9
			var s3 = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.3)).
				Select(n => "3-" + (n + 1)).Take(4);

			var eofs = new[] { s1, s2, s3 };
			var offsets = new[] { 0.1, 0.4, 1 };
			var sofs = Observable.Generate(
				0, n => n < eofs.Length, n => n + 1,
				n => eofs[n],
				n => TimeSpan.FromSeconds(offsets[n]));
			sofs.Switch().Dump("Switch");
		}


		public static void AndThenExample()
		{
			var one = Observable.Interval(TimeSpan.FromSeconds(0.3)).Take(5);
			var two = Observable.Interval(TimeSpan.FromMilliseconds(250)).Take(10);
			var three = Observable.Interval(TimeSpan.FromMilliseconds(150)).Take(14);

			var zippedSequence = Observable.When(
				one.And(two).And(three).
				Then((first, second, third) =>
				new
				{
					One = first,
					Two = second,
					Three = third
				})
			);

			zippedSequence.Dump("AndThen");
		}


		public static void SampleExample()
		{
			var interval = Observable.Interval(TimeSpan.FromMilliseconds(150));
			interval.Sample(TimeSpan.FromSeconds(1)).Take(3).Dump("Sample");
		}


		public static void PublishExample()
		{
			var period = TimeSpan.FromSeconds(0.5);
			var observable = Observable.Interval(period).Publish();
			using (observable.Connect())
			{
				observable.Subscribe(i => Console.WriteLine("first subscription : {0}", i));
				Thread.Sleep(1000);
				observable.Subscribe(i => Console.WriteLine("second subscription : {0}", i));
				Thread.Sleep(2000);
			}
		}


		public static void RefCountExample()
		{
			var period = TimeSpan.FromSeconds(0.25);
			var observable = Observable.Interval(period)
				.Do(l => Console.WriteLine("Publishing {0}", l)) //side effect to show it is running
				.Publish()
				.RefCount();

			Console.WriteLine("Created RefCount.");
			Thread.Sleep(600);

			var subscription = observable.Subscribe(i => Console.WriteLine("subscription : {0}", i));
			Console.WriteLine("Subscribed.");
			Thread.Sleep(600);

			subscription.Dispose();
			Console.WriteLine("Unsubscribed.");
			Thread.Sleep(600);
		}


		public static void SubscribeOnExample()
		{
			Console.WriteLine("Starting on threadId:{0}", Thread.CurrentThread.ManagedThreadId);
			var ev = new ManualResetEvent(false);

			var source = Observable.Create<int>(
				o =>
				{
					Console.WriteLine("Subscribe invoked on threadId:{0}", Thread.CurrentThread.ManagedThreadId);
					o.OnNext(1);
					o.OnNext(2);
					o.OnNext(3);
					o.OnCompleted();
					Console.WriteLine("Subscribe finished on threadId:{0}", Thread.CurrentThread.ManagedThreadId);
					return Disposable.Empty;
				});

			source
				.SubscribeOn(Scheduler.Default)
				.ObserveOn(Scheduler.Default)
				.Subscribe(
					o => Console.WriteLine("OnNext {1} on threadId:{0}", Thread.CurrentThread.ManagedThreadId, o),
					() => { Console.WriteLine("OnCompleted on threadId:{0}", Thread.CurrentThread.ManagedThreadId); ev.Set(); }
				);

			Console.WriteLine("Subscribed on threadId:{0}", Thread.CurrentThread.ManagedThreadId);
			ev.WaitOne();
		}


		public static void FileStreamExample()
		{
			const string FileName = @"C:\Apps\Linux_Reader.exe";

			using (var fs = File.Open(FileName, FileMode.Open))
			{
				var s = fs.ToObservable(4096, Scheduler.Default);
				var count = 0L;
				var ev = new ManualResetEvent(false);

				using (s.Subscribe(b =>
				{
					count++;
					if (count % 1000000 == 0) Console.Write(".");
				}, () => { Console.WriteLine(); ev.Set(); }))
				{
					ev.WaitOne();
				}
				Console.WriteLine($"Counted {count} bytes.");
			}

			using (var fs = File.Open(FileName, FileMode.Open))
			{
				var s = fs.ToObservable(4096, Scheduler.Default);
				var count = 0L;

				var subscr = s.Subscribe(b =>
				{
					count++;
					if (count % 1000000 == 0) Console.Write(".");
				}, () => Console.WriteLine());

				Thread.Sleep(200);
				subscr.Dispose();

				Console.WriteLine();
				Console.WriteLine($"Interrupted at {count} bytes.");
			}
		}


		public static void SchedulerExample()
		{
			IDisposable OuterAction(IScheduler scheduler, string state)
			{
				Console.WriteLine("-- {0} start. ThreadId:{1}", state, Thread.CurrentThread.ManagedThreadId);
				scheduler.Schedule(state + ".inner", InnerAction);
				Console.WriteLine("-- {0} end. ThreadId:{1}", state, Thread.CurrentThread.ManagedThreadId);
				return Disposable.Empty;
			}

			IDisposable InnerAction(IScheduler scheduler, string state)
			{
				Console.WriteLine("---- {0} start. ThreadId:{1}", state, Thread.CurrentThread.ManagedThreadId);
				scheduler.Schedule(state + ".Leaf", LeafAction);
				Console.WriteLine("---- {0} end. ThreadId:{1}", state, Thread.CurrentThread.ManagedThreadId);
				return Disposable.Empty;
			}

			IDisposable LeafAction(IScheduler scheduler, string state)
			{
				Console.WriteLine("------ {0}. ThreadId:{1}", state, Thread.CurrentThread.ManagedThreadId);
				return Disposable.Empty;
			}

			Console.WriteLine("Starting on thread :{0}", Thread.CurrentThread.ManagedThreadId);

			Console.WriteLine("Scheduler.Immediate");
			Scheduler.Immediate.Schedule("Immediate", OuterAction);

			Console.WriteLine("Scheduler.CurrentThread");
			Scheduler.CurrentThread.Schedule("Current", OuterAction);
			Thread.Sleep(100);

			Console.WriteLine("Scheduler.Default");
			Scheduler.Default.Schedule("Def-A", OuterAction);
			Scheduler.Default.Schedule("Def-B", OuterAction);
			Thread.Sleep(100);

			var els = new EventLoopScheduler();
			Console.WriteLine("EventLoopScheduler");
			els.Schedule("Els-A", OuterAction);
			els.Schedule("Els-B", OuterAction);
			Thread.Sleep(100);

			Console.WriteLine("TaskPoolScheduler.Default");
			TaskPoolScheduler.Default.Schedule("Task-A", OuterAction);
			TaskPoolScheduler.Default.Schedule("Task-B", OuterAction);
			Thread.Sleep(100);

			Console.WriteLine("ThreadPoolScheduler.Instance");
			ThreadPoolScheduler.Instance.Schedule("Thread-A", OuterAction);
			ThreadPoolScheduler.Instance.Schedule("Thread-B", OuterAction);
			Thread.Sleep(100);

			Console.WriteLine("NewThreadScheduler.Default");
			NewThreadScheduler.Default.Schedule("New-A", OuterAction);
			NewThreadScheduler.Default.Schedule("New-B", OuterAction);
			Thread.Sleep(100);
		}


		public static void TestSchedulerExample()
		{
			var scheduler = new TestScheduler();
			var wasExecuted = false;
			scheduler.Schedule(() => wasExecuted = true);
			Debug.Assert(!wasExecuted);
			scheduler.AdvanceBy(1); //execute 1 tick of queued actions
			Debug.Assert(wasExecuted);

			scheduler.Schedule(TimeSpan.FromTicks(10), () => Console.WriteLine("A"));
			scheduler.Schedule(TimeSpan.FromTicks(10), () => Console.WriteLine("B"));
			scheduler.Schedule(TimeSpan.FromTicks(10), () =>
			{
				Console.WriteLine("C");
				scheduler.Schedule(TimeSpan.FromTicks(10), () => Console.WriteLine("D"));
			});
			Console.WriteLine("scheduler.Start();");
			scheduler.Start();
			Console.WriteLine("scheduler.Clock:{0}", scheduler.Clock);
		}


		public static void TestStartExample()
		{
			var scheduler = new TestScheduler();
			var source = Observable.Interval(TimeSpan.FromSeconds(1), scheduler).Take(4);
			var testObserver = scheduler.Start(
				() => source, 0, TimeSpan.FromSeconds(2).Ticks, TimeSpan.FromSeconds(5).Ticks);

			Console.WriteLine("Time is {0} ticks", scheduler.Clock);
			Console.WriteLine("Received {0} notifications", testObserver.Messages.Count);

			foreach (Recorded<Notification<long>> message in testObserver.Messages)
			{
				Console.WriteLine("{0} @ {1}", message.Value, message.Time);
			}
		}


		public static void CreateColdObservableExample()
		{
			var scheduler = new TestScheduler();
			var source = scheduler.CreateColdObservable(
				new Recorded<Notification<long>>(10000000, Notification.CreateOnNext(0L)),
				new Recorded<Notification<long>>(20000000, Notification.CreateOnNext(1L)),
				new Recorded<Notification<long>>(30000000, Notification.CreateOnNext(2L)),
				new Recorded<Notification<long>>(40000000, Notification.CreateOnNext(3L)),
				new Recorded<Notification<long>>(40000000, Notification.CreateOnCompleted<long>())
			);

			var testObserver = scheduler.Start(() => source, 0, TimeSpan.FromSeconds(1).Ticks, TimeSpan.FromSeconds(5).Ticks);
			Console.WriteLine("Time is {0} ticks", scheduler.Clock);
			Console.WriteLine("Received {0} notifications", testObserver.Messages.Count);

			foreach (Recorded<Notification<long>> message in testObserver.Messages)
			{
				Console.WriteLine("  {0} @ {1}", message.Value, message.Time);
			}
		}


		public static void CreateHotObservableExample()
		{
			var scheduler = new TestScheduler();
			var source = scheduler.CreateHotObservable(
				new Recorded<Notification<long>>(10000000, Notification.CreateOnNext(0L)),
				new Recorded<Notification<long>>(20000000, Notification.CreateOnNext(1L)),
				new Recorded<Notification<long>>(30000000, Notification.CreateOnNext(2L)),
				new Recorded<Notification<long>>(40000000, Notification.CreateOnNext(3L)),
				new Recorded<Notification<long>>(40000000, Notification.CreateOnCompleted<long>())
			);

			var testObserver = scheduler.Start(() => source, 0, TimeSpan.FromSeconds(1).Ticks, TimeSpan.FromSeconds(5).Ticks);
			Console.WriteLine("Time is {0} ticks", scheduler.Clock);
			Console.WriteLine("Received {0} notifications", testObserver.Messages.Count);

			foreach (Recorded<Notification<long>> message in testObserver.Messages)
			{
				Console.WriteLine("  {0} @ {1}", message.Value, message.Time);
			}
		}


		public static void WindowFromCountExample()
		{
			var windowIdx = 0;
			var ev = new ManualResetEvent(false);

			var source = Observable.Interval(TimeSpan.FromSeconds(0.2)).Take(10);
			using (source.Window(3, 2).Subscribe(window =>
				{
					var thisWindowIdx = windowIdx++;
					Console.WriteLine("--Starting new window");
					var windowName = "Window" + thisWindowIdx;
					window.Subscribe(
						value => Console.WriteLine("{0} : {1}", windowName, value),
						ex => Console.WriteLine("{0} : {1}", windowName, ex),
						() => Console.WriteLine("{0} Completed", windowName)
					);
				},
				() => { Console.WriteLine("Completed"); ev.Set(); }))
			{
				ev.WaitOne();
			}
		}


		public static void WindowFromSelectorExample()
		{
			var windowIdx = 0;
			var done = false;
			var source = Observable.Interval(TimeSpan.FromSeconds(0.2)).Take(10);
			var closer = new Subject<Unit>();
			source.Window(() => closer)
				.Subscribe(window =>
					{
						var thisWindowIdx = windowIdx++;
						Console.WriteLine("--Starting new window");
						var windowName = "Window" + thisWindowIdx;
						window.Subscribe(
							value => Console.WriteLine("{0} : {1}", windowName, value),
							ex => Console.WriteLine("{0} : {1}", windowName, ex),
							() => Console.WriteLine("{0} Completed", windowName));
					},
					() => { Console.WriteLine("Completed"); done = true; }
				);

			while (!done)
			{
				Thread.Sleep(500);
				closer.OnNext(Unit.Default);
			}
		}


		public static void MyWindowExample()
		{
			var windowIdx = 0;
			var ev = new ManualResetEvent(false);

			var source = Observable.Interval(TimeSpan.FromSeconds(0.2)).Take(10);
			using (source.MyWindow(3).Subscribe(window =>
			{
				var thisWindowIdx = windowIdx++;
				Console.WriteLine("--Starting new window");
				var windowName = "Window" + thisWindowIdx;
				window.Subscribe(
					value => Console.WriteLine("{0} : {1}", windowName, value),
					ex => Console.WriteLine("{0} : {1}", windowName, ex),
					() => Console.WriteLine("{0} Completed", windowName)
				);
			},
				() => { Console.WriteLine("Completed"); ev.Set(); }))
			{
				ev.WaitOne();
			}
		}


		public static void MyCombineLatestExample()
		{
			var source1 = Observable.Interval(TimeSpan.FromSeconds(0.1)).Take(10);
			var source2 = Observable.Interval(TimeSpan.FromSeconds(0.2)).Take(10).Select(n => (char) (n + 'A'));
			var res = source1.MyCombineLatest(source2, (n, c) => string.Format($"{n}{c}"));
			var ev = new ManualResetEvent(false);

			using (res.Subscribe(
				v => Console.WriteLine(v),
				() => ev.Set()))
			{
				ev.WaitOne();
			}
		}


		public static void GroupJoinExample()
		{
			var source1 = Observable.Timer(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1)).Take(10)
				.Publish().RefCount();
			var source2 = Observable.Timer(TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(0.2)).Take(10)
				.Select(n => (char) (n + 'A')).Publish().RefCount();

			var res = source1.GroupJoin(source2, _ => source1, _ => source2,
				(n, cl) => cl.Aggregate("", (s, c) => s + (s == "" ? "" : ",") + $"{n}{c}").Select(ag => $"{n} - {ag}")).
				Merge();

			var ev = new ManualResetEvent(false);
			using (res.Subscribe(Console.WriteLine, () => ev.Set()))
			{
				ev.WaitOne();
			}
		}
	}
}
