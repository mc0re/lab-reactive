using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace IntroToRx
{
	public static class ObservableExtensions
	{
		public static IObservable<bool> MyAny<T>(this IObservable<T> source)
		{
			return Observable.Create<bool>(
				obr =>
				{
					var hasValues = false;

					return source.Take(1).Subscribe(_ => hasValues = true, obr.OnError, () =>
					  {
						  obr.OnNext(hasValues);
						  obr.OnCompleted();
					  });
				});
		}


		public static IObservable<T> MyRetry<T>(
			this IObservable<T> source, TimeSpan delay, int retryCount, Func<Exception, bool> retryCondition)
		{
			return source.RetryWhen(obsEx =>
			{
				var retries = 0;
				var retrySignal = new Subject<Unit>();

				var subscr = obsEx.Subscribe(
					ex =>
					{
						retries++;
						if (retries >= retryCount || !retryCondition(ex))
						{
							retrySignal.OnError(ex);
						}
						else
						{
							Observable.Timer(delay).Do(_ => retrySignal.OnNext(Unit.Default)).Subscribe();
						}
					});

				return retrySignal;
			});
		}


		public static IObservable<T> Dump<T>(this IObservable<T> source, string name)
		{
			var ev = new ManualResetEvent(false);

			using (source.Subscribe(
				i => Console.WriteLine("{0}-->{1}", name, i),
				ex => { Console.WriteLine("{0} failed-->{1}", name, ex.Message); ev.Set(); },
				() => { Console.WriteLine("{0} completed", name); ev.Set(); }))
			{
				ev.WaitOne();
			}

			return source;
		}


		public static Task DumpAsync<T>(this IObservable<T> source, string name)
		{
			var task = Task.Factory.StartNew(() =>
			{
				var ev = new ManualResetEvent(false);

				using (source.Subscribe(
					i => Console.WriteLine("{0}-->{1}", name, i),
					ex => { Console.WriteLine("{0} failed-->{1}", name, ex.Message); ev.Set(); },
					() => { Console.WriteLine("{0} completed", name); ev.Set(); }))
				{
					ev.WaitOne();
				}
			});

			Thread.Sleep(50);

			return task;
		}


		private sealed class StreamReaderState
		{
			private readonly int mBufferSize;

			private readonly IObservable<int> mFactory;

			public byte[] Buffer { get; set; }


			public StreamReaderState(FileStream source, int bufferSize)
			{
				mBufferSize = bufferSize;
				Buffer = new byte[bufferSize];
				mFactory = Observable.FromAsync(() => source.ReadAsync(Buffer, 0, bufferSize));
			}


			public IObservable<int> ReadNext()
			{
				return mFactory;
			}
		}


		public static IObservable<byte> ToObservable(
			this FileStream source,
			int buffersize,
			IScheduler scheduler)
		{
			var bytes = Observable.Create<byte>(o =>
			{
				var initialState = new StreamReaderState(source, buffersize);
				var currentStateSubscription = new SerialDisposable();

				void iterator(StreamReaderState state, Action<StreamReaderState> self) =>
					currentStateSubscription.Disposable = state.ReadNext()
						.Subscribe(
							bytesRead =>
							{
								for (int i = 0; i < bytesRead; i++)
								{
									o.OnNext(state.Buffer[i]);
								}
								if (bytesRead > 0)
									self(state);
								else
									o.OnCompleted();
							},
							o.OnError);

				var scheduledWork = scheduler.Schedule(initialState, iterator);

				return new CompositeDisposable(currentStateSubscription, scheduledWork);
			});

			return Observable.Using(() => source, _ => bytes);
		}


		public static IObservable<IObservable<T>> MyWindow<T>(this IObservable<T> source, int count)
		{
			var shared = source.Publish().RefCount();
			var windowEdge = shared
				.Select((i, idx) => idx % count)
				.Where(mod => mod == 0)
				.Publish()
				.RefCount();
			return shared.Window(windowEdge, _ => windowEdge);
		}


		public static IObservable<IList<T>> MyBuffer<T>(this IObservable<T> source, int count)
		{
			return source.Window(count)
				.SelectMany(window =>
					window.Aggregate(
						new List<T>(),
						(list, item) => { list.Add(item); return list; }
					));
		}


		public static IObservable<TResult> MyCombineLatest<TLeft, TRight, TResult>(
			this IObservable<TLeft> left,
			IObservable<TRight> right,
			Func<TLeft, TRight, TResult> resultSelector)
		{
			var refcountedLeft = left.Publish().RefCount();
			var refcountedRight = right.Publish().RefCount();
			return Observable.Join(
				refcountedLeft,
				refcountedRight,
				value => refcountedLeft,
				value => refcountedRight,
				resultSelector);
		}
	}
}
