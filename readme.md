# ReactiveGpio

ReactiveGpio is a .Net library for the control of GPIO pins on Linux utilising Reactive Extensions.

The current release supports GPIO input/output. 

## Input Port

An input port can be constructed using the static constructor  method passing in the Gpio pin number, the edge to trigger on and optionally the driver to use.

`var port = await InputPort.Create(22, GpioEdge.Both);`

The value of the port can then be read via the `Read()` or `ReadAsync()` methods.

An input port is also an `IObservable<bool>` so it can also be observed using Reactive extensions.

```
var port = await InputPort.Create(22, GpioEdge.Both);

// Will write out to the console every time the edge stated is hit.
port.Subscribe(reading => Console.WriteLine("Reading: {0}", reading));
```

## Output Port

Like the input port the output port is created with a static constructor method. It is created by passing in the pin number and optionally the initial value and driver.

```
var port = await OutputPort.Create(23);
// or
var port = await OutputPort.Create(23, OutputPort.InitialValue.Low);
```

The value of the port can then be set via the `Write(value)` or `WriteAsync(value)` methods.

The Output port is also an `IObserver<bool>` so it can observe an `IObservable<bool>` using Reactive extensions.

```
// Create observable that will generate an incrementing number every second
var observable = Observable.Generate(1, x => true, x => x + 1, x => x, x => TimeSpan.FromSeconds(1));

var port = await OutputPort.Create(23);

// Write true whenever the number is even and odd when the number is odd
observable.Select(x => x%2 == 0)
          .Subscribe(port)
```

## Drivers

The ports take a driver class that controls how the port is read.

### FileDriver

The file driver is the default driver, if no driver is specified it will be used.  It uses the file based I/O to control the port.

This driver can be used as a non-root user.