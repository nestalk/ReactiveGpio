using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ReactiveGpio.Drivers;

namespace ReactiveGpio
{
    public class OutputPort : IDisposable, IObserver<bool>
    {
        public enum InitialValue
        {
            High,
            Low,
        }

        private readonly IGpioDriver _driver;
        private readonly string _pin;

        private OutputPort(int pin, IGpioDriver driver)
        {
            _driver = driver;
            _pin = pin.ToString(CultureInfo.InvariantCulture);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async void OnNext(bool value)
        {
            await WriteAsync(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnAssignPin().Wait();
            }
        }

        public static async Task<OutputPort> Create(int pin, IGpioDriver driver = null)
        {
            var port = new OutputPort(pin, driver ?? new FileDriver());

            // If port already exists delete and recreate it
            if (Directory.Exists(GpioPath.Path(pin)))
                await port.UnAssignPin();
            await port.AssignPin();

            await port.SetDirection(GpioDirection.Out);

            return port;
        }

        public static async Task<OutputPort> Create(int pin, InitialValue initial, IGpioDriver driver = null)
        {
            var port = await Create(pin, driver ?? new FileDriver());

            await port.SetDirection(initial == InitialValue.High ? GpioDirection.High : GpioDirection.Low);

            return port;
        }

        private async Task SetDirection(GpioDirection direction)
        {
            await _driver.SetDirection(_pin, direction);
        }

        private async Task AssignPin()
        {
            await _driver.AssignPin(_pin);
        }

        private async Task UnAssignPin()
        {
            await _driver.UnAssignPin(_pin);
        }

        public async Task WriteAsync(bool value)
        {
            await _driver.WriteAsync(_pin, value);
        }

        public void Write(bool value)
        {
            _driver.Write(_pin, value);
        }
    }
}