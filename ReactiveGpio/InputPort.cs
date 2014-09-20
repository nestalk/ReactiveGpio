using System;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveGpio.Drivers;

namespace ReactiveGpio
{
    public class InputPort : IDisposable, IObservable<bool>
    {
        private readonly IGpioDriver _driver;
        private readonly string _pin;
        private int _pollFd;

        private IObservable<bool> _readings;

        private InputPort(int pin, IGpioDriver driver)
        {
            _driver = driver;
            _pin = pin.ToString(CultureInfo.InvariantCulture);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            return _readings.Subscribe(observer);
        }

        public static async Task<InputPort> Create(int pin, GpioEdge edge, IGpioDriver driver = null)
        {
            var port = new InputPort(pin, driver ?? new FileDriver());

            // If port already exists delete and recreate it
            if (Directory.Exists(GpioPath.Path(pin)))
                await port.UnAssignPin();
            await port.AssignPin();

            await port.SetDirection();
            await port.SetEdge(edge);
            port.SetupInterrupt();

            return port;
        }

        private void SetupInterrupt()
        {
            _pollFd = _driver.SetupInterrupt(_pin);

            _readings = Observable.Create<bool>(o =>
            {
                var cancel = new CancellationDisposable();

                NewThreadScheduler.Default.Schedule(async () =>
                {
                    while (true)
                    {
                        if (cancel.Token.IsCancellationRequested)
                        {
                            o.OnCompleted();
                            return;
                        }

                        try
                        {
                            var wait = _driver.WaitOnInterrupt(_pollFd);
                            if (wait <= 0)
                                continue;

                            o.OnNext(await ReadAsync());
                        }
                        catch (Exception e)
                        {
                            o.OnError(e);
                        }
                    }
                });

                return cancel;
            });
        }

        private async Task AssignPin()
        {
            await _driver.AssignPin(_pin);
        }

        private async Task UnAssignPin()
        {
            await _driver.UnAssignPin(_pin);
        }

        private async Task SetDirection()
        {
            await _driver.SetDirection(_pin, GpioDirection.In);
        }

        private async Task SetEdge(GpioEdge edge)
        {
            await _driver.SetEdge(_pin, edge);
        }

        public async Task<bool> ReadAsync()
        {
            return await _driver.ReadAsync(_pin);
        }

        public bool Read()
        {
            return _driver.Read(_pin);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _driver.CloseInterrupt(_pollFd);
                UnAssignPin().Wait();
            }
        }
    }
}