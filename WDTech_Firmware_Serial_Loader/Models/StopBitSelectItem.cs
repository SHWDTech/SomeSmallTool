using System.IO.Ports;

namespace WDTech_Firmware_Serial_Loader.Models
{
    public class StopBitSelectItem
    {
        public string Name { get; set; }

        public StopBits Value { get; set; }
    }
}
