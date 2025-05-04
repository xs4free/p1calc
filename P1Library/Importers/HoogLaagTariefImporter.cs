using P1Library.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace P1Library.Importers
{
    internal class HoogLaagTarieven
    {
        public List<HoogLaagTarief> Tarieven { get; set; } = [];
    }

    internal class HoogLaagTarief
    {
        public string? Type { get; set; }
        public decimal Prijs { get; set; }
        public string? Omschrijving { get; set; }
        public int Prioriteit { get; set; }
        public List<HoogLaagMoment> Momenten { get; set; } = [];
    }

    internal class HoogLaagMoment
    {
        public string? Description { get; set; }
        public List<string> Dagen { get; set; } = [];
        public List<DateOnly> Datums { get; set; } = [];
        public TimeOnly? Start { get; set; }
        public TimeOnly? Einde { get; set; }
    }

    public class HoogLaagTariefImporter
    {
        public static async IAsyncEnumerable<Tarief> Import(string fileName)
        {
            var content = await File.ReadAllTextAsync(fileName);
            var tarieven = JsonSerializer.Deserialize<HoogLaagTarieven>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CustomDateOnlyConverter(), new CustomTimeOnlyConverter() }
            });

            var laatsteDag = new DateTime(2024, 12, 31, 23, 59, 59);

            for (DateTime dag = new DateTime(2024, 01, 01, 0, 0, 0); dag <= laatsteDag; dag = dag.AddHours(1))
            {
                var dagString = dag.ToString("dddd").ToLower();
                yield return new Tarief
                {
                    Start = dag,
                    Einde = dag.AddHours(1) - TimeSpan.FromSeconds(1),
                    Prijs = ZoekTarief(tarieven, dag)
                };
            }
        }

        private static decimal ZoekTarief(HoogLaagTarieven? tarieven, DateTime dag)
        {
            if (tarieven == null)
            {
                throw new ArgumentNullException(nameof(tarieven), "Tarieven zijn niet geladen.");
            }

            var dagText = dag.ToString("dddd").ToLower();
            var alleenDatum = DateOnly.FromDateTime(dag);

            foreach (var tarief in tarieven.Tarieven.OrderByDescending(t => t.Prioriteit))
            {
                foreach (var moment in tarief.Momenten)
                {
                    if (moment.Dagen.Contains(dagText) 
                        || moment.Datums.Contains(alleenDatum))
                    {
                        if (moment.Start.HasValue && moment.Einde.HasValue)
                        {
                            var start = new DateTime(dag.Year, dag.Month, dag.Day, moment.Start.Value.Hour, moment.Start.Value.Minute, 0);
                            var einde = new DateTime(dag.Year, dag.Month, dag.Day, moment.Einde.Value.Hour, moment.Einde.Value.Minute, 0);
                            if (dag >= start && dag <= einde)
                            {
                                return tarief.Prijs;
                            }
                        }
                    }
                }
            }
            
            throw new InvalidDataException($"Tarief niet gevonden voor {dag}.");
        }
    }

    public class CustomDateOnlyConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateOnly.ParseExact(reader.GetString(), "dd-MM-yyyy", null);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("dd-MM-yyyy"));
        }
    }

    public class CustomTimeOnlyConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateOnly.ParseExact(reader.GetString(), "hh:mm:ss", null);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("hh:mm:ss"));
        }
    }

}
