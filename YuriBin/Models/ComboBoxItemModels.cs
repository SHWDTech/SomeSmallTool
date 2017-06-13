namespace YuriBin.Models
{
    class ComboBoxItemModels
    {
        public class TargetObjectItem
        {
            public string Name { get; set; }

            public byte Value { get; set; }
        }

        public class UpdateModeItem
        {
            public string Name { get; set; }

            public byte[] Value { get; set; }
        }
    }
}
