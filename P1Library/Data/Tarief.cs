namespace P1Library.Data
{
    public record Tarief
    {
        public DateTime Start { get; set; }
        public DateTime Einde { get; set; }
        public decimal Prijs { get; set; }
    }
}
