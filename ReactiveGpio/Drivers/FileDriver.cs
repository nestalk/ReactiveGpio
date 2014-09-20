using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mono.Unix.Native;

namespace ReactiveGpio.Drivers
{
    public class FileDriver : IGpioDriver
    {
        public async Task AssignPin(string pin)
        {
            using (var writer = new StreamWriter(GpioPath.ExportPath(pin), false))
            {
                await writer.WriteAsync(pin);
            }
        }


        public async Task UnAssignPin(string pin)
        {
            using (var writer = new StreamWriter(GpioPath.UnExportPath(pin), false))
                await writer.WriteAsync(pin);
        }

        public async Task SetDirection(string pin, GpioDirection direction)
        {
            string direct;
            switch (direction)
            {
                case GpioDirection.In:
                    direct = "in";
                    break;
                case GpioDirection.Out:
                    direct = "out";
                    break;
                case GpioDirection.Low:
                    direct = "low";
                    break;
                case GpioDirection.High:
                    direct = "high";
                    break;
                default:
                    throw new ArgumentException("Requires a direction");
            }

            try
            {
                using (var writer = new StreamWriter(GpioPath.DirectionPath(pin), false))
                {
                    await writer.WriteAsync(direct);
                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // User is not root and the permissions have not been set yet. Need to wait.
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            using (var writer = new StreamWriter(GpioPath.DirectionPath(pin), false))
            {
                await writer.WriteAsync(direct);
            }
        }


        public async Task SetEdge(string pin, GpioEdge edge)
        {
            string edgeValue;
            switch (edge)
            {
                case GpioEdge.None:
                    edgeValue = "none";
                    break;
                case GpioEdge.Both:
                    edgeValue = "both";
                    break;
                case GpioEdge.Rising:
                    edgeValue = "rising";
                    break;
                case GpioEdge.Falling:
                    edgeValue = "falling";
                    break;
                default:
                    throw new ArgumentException("Requires a direction");
            }

            try
            {
                using (var writer = new StreamWriter(GpioPath.EdgePath(pin), false))
                {
                    await writer.WriteAsync(edgeValue);
                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // User is not root and the permissions have not been set yet. Need to wait.
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }

            using (var writer = new StreamWriter(GpioPath.EdgePath(pin), false))
                await writer.WriteAsync(edgeValue);
        }

        public async Task<bool> ReadAsync(string pin)
        {
            using (
                var reader =
                    new StreamReader(new FileStream(GpioPath.ValuePath(pin), FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite)))
            {
                var rawValue = await reader.ReadToEndAsync();
                return rawValue.StartsWith("1");
            }
        }

        public async Task WriteAsync(string pin, bool value)
        {
            using (var writer = new StreamWriter(GpioPath.ValuePath(pin), false))
                await writer.WriteAsync(value ? "1" : "0");
        }

        public bool Read(string pin)
        {
            using (
                var reader =
                    new StreamReader(new FileStream(GpioPath.ValuePath(pin), FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite)))
            {
                var rawValue = reader.ReadToEnd();
                return rawValue.StartsWith("1");
            }
        }

        public void Write(string pin, bool value)
        {
            using (var writer = new StreamWriter(GpioPath.ValuePath(pin), false))
                writer.Write(value ? "1" : "0");
        }

        public int SetupInterrupt(string pin)
        {
            var pollFd = Syscall.epoll_create(1);
            var valueFd = Syscall.open(GpioPath.ValuePath(pin), OpenFlags.O_NONBLOCK | OpenFlags.O_RDONLY);
            if (
                Syscall.epoll_ctl(pollFd, EpollOp.EPOLL_CTL_ADD, valueFd,
                    EpollEvents.EPOLLIN | EpollEvents.EPOLLET | EpollEvents.EPOLLPRI) != 0)
                throw new Exception("Error creating interupt");

            return pollFd;
        }

        public int WaitOnInterrupt(int pollFd)
        {
            var eventObject = new EpollEvent[1];
            return Syscall.epoll_wait(pollFd, eventObject, 1, -1);
        }

        public void CloseInterrupt(int pollFd)
        {
            Syscall.close(pollFd);
        }
    }
}