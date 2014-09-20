using System.Threading.Tasks;

namespace ReactiveGpio
{
    public interface IGpioDriver
    {
        Task AssignPin(string pin);
        Task UnAssignPin(string pin);
        Task SetDirection(string pin, GpioDirection direction);
        Task SetEdge(string pin, GpioEdge edge);
        Task<bool> ReadAsync(string pin);
        Task WriteAsync(string pin, bool value);
        bool Read(string pin);
        void Write(string pin, bool value);
        int SetupInterrupt(string pin);
        int WaitOnInterrupt(int pollFd);
        void CloseInterrupt(int pollFd);
    }
}