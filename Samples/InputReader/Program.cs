using System;
using ReactiveGpio;

namespace InputReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var port = InputPort.Create(18, GpioEdge.Both).Result)
            {
                Console.WriteLine("Started");
                port.Subscribe(reading => Console.WriteLine("Reading: {0}", reading),
                    error => Console.WriteLine("Error: {0}", error.Message));

                Console.ReadLine();
            }
        }
    }
}