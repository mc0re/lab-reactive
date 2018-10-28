using System;
using System.Threading;

namespace ReactiveLab
{
	internal class PingPong
	{
		internal static void Run()
		{
			var ping = new Ping();
			var pong = new Pong();

			var pongSubscription = ping.Subscribe(pong);
			var pingSubscription = pong.Subscribe(ping);

			Thread.Sleep(5000);

			pongSubscription.Dispose();
			pingSubscription.Dispose();

			Console.WriteLine("Ping Pong has completed.");
		}
	}
}