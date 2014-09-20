using System;
using ReactiveGpio;

namespace InNOut
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            using (var inputPort = InputPort.Create(18, GpioEdge.Both).Result)
            using (var outputPort = OutputPort.Create(23, OutputPort.InitialValue.Low).Result)
            {

                Console.WriteLine("Started");
                inputPort.Subscribe(outputPort);

                Console.ReadLine();
            }
        }
    }
}