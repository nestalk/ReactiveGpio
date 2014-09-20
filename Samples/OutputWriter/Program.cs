using System;
using System.Reactive.Linq;
using ReactiveGpio;

namespace OutputWriter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var observable = Observable.Generate(1, x => true, x => x + 1, x => x,
                x => TimeSpan.FromSeconds(1));

            using (var port = OutputPort.Create(23, OutputPort.InitialValue.Low).Result)
            {
                Console.WriteLine("Started");
                observable.Subscribe(Console.WriteLine);
                observable.Select(x => x%2 == 0).Subscribe(port);

                Console.ReadLine();
            }
        }
    }
}