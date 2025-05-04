namespace P1Library.Data
{
    public record P1Data
    {
        public DateTime DateTime { get; set; }
        public decimal ImportT1 { get; set; } //kWh
        public decimal ExportT1 { get; set; } //kWh
        public decimal ImportT2 { get; set; } //kWh
        public decimal ExportT2 { get; set; } //kWh
        public int PowerL1 { get; set; } //Watt
        public int PowerL2 { get; set; } //Watt
        public int PowerL3 { get; set; } //Watt
    }
}
