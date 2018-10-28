using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace ReactiveLab
{
	public class Pong : ISubject<Ping, Pong>
	{
		#region Implementation of IObserver<Ping>

		/// <summary>
		/// Notifies the observer of a new value in the sequence.
		/// </summary>
		public void OnNext(Ping value)
		{
			Console.WriteLine("Pong received Ping.");
		}


		/// <summary>
		/// Notifies the observer that an exception has occurred.
		/// </summary>
		public void OnError(Exception exception)
		{
			Console.WriteLine("Pong experienced an exception and had to quit playing.");
		}


		/// <summary>
		/// Notifies the observer of the end of the sequence.
		/// </summary>
		public void OnCompleted()
		{
			Console.WriteLine("Pong finished.");
		}

		#endregion


		#region Implementation of IObservable<Pong>

		/// <summary>
		/// Subscribes an observer to the observable sequence.
		/// </summary>
		public IDisposable Subscribe(IObserver<Pong> observer)
		{
			return Observable.Interval(TimeSpan.FromSeconds(1.5))
				.Where(n => n < 10)
				.Select(n => this)
				.Subscribe(observer);
		}

		#endregion


		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			OnCompleted();
		}

		#endregion
	}
}