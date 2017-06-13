using System.IO.Ports;

namespace WDTech_Firmware_Serial_Loader.Models
{
    public class ParitySelectItem
    {
        public string Name { get; set; }

        public Parity Value { get; set; }
    }
}
