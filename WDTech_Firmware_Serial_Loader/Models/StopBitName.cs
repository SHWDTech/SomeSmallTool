namespace WDTech_Firmware_Serial_Loader.Models
{
    public class StopBitName
    {
        public const string None = "无";

        public const string One = "1";

        public const string Two = "2";

        public const string OnePointFive = "1.5";

        public static string GetStopBitName(string name)
        {
            switch (name)
            {
                case "None":
                    return None;
                case "One":
                    return One;
                case "Two":
                    return Two;
                case "OnePointFive":
                    return OnePointFive;
                default:
                    return string.Empty;
            }
        }
    }
}
