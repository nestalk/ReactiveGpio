namespace ReactiveGpio
{
    public static class GpioPath
    {
        const string BasePath = "/sys/class/gpio/";

        public static string Path(int pin)
        {
            return string.Format("{0}gpio{1}", BasePath, pin);
        }

        public static string ExportPath(string pin)
        {
            return string.Format("{0}/export", BasePath);
        }

        public static string UnExportPath(string pin)
        {
            return string.Format("{0}/unexport", BasePath);
        }

        public static string EdgePath(string pin)
        {
            return string.Format("{0}gpio{1}/edge", BasePath, pin);
        }

        public static string DirectionPath(string pin)
        {
            return string.Format("{0}gpio{1}/direction", BasePath, pin);
        }

        public static string ValuePath(string pin)
        {
            return string.Format("{0}gpio{1}/value", BasePath, pin);
        }
    }
}