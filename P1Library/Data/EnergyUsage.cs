namespace P1Library.Data
{
    public record EnergyUsage
    {
        public DateTime DateTime { get; set; }
        public decimal Import { get; set; } //kWh
        public decimal Export { get; set; } //kWh
        public List<P1Data> P1Lines { get; set; } = [];
        public bool LaadpaalActief { get; set; } = false;
    }
}
