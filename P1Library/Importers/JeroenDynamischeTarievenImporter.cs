using P1Library.Data;
using System.Globalization;

namespace P1Library.Importers
{
    public static class JeroenDynamischeTarievenImporter
    {
        private const char Delimiter = ';';
        public static async IAsyncEnumerable<Tarief> Import(string fileName)
        {
            bool headerParsed = false;
            await foreach (var line in File.ReadLinesAsync(fileName))
            {
                if (!headerParsed)
                {
                    ValidateHeader(line);
                    headerParsed = true;
                    continue;
                }
                yield return ParseTarief(line);
            }
        }
        private static void ValidateHeader(string line)
        {
            var splitHeader = line.Split([Delimiter], StringSplitOptions.RemoveEmptyEntries);
            if (splitHeader.Length != 2)
            {
                throw new InvalidDataException("Expected header to contain 2 columns: datum;prijs_excl_belastingen");
            }
        }
        private static Tarief ParseTarief(string line)
        {
            var split = line.Split([Delimiter], StringSplitOptions.RemoveEmptyEntries);
            return new Tarief
            {
                Start = DateTime.ParseExact(split[0].Trim('"'), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                Einde = DateTime.ParseExact(split[0].Trim('"'), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + TimeSpan.FromHours(1) - TimeSpan.FromSeconds(1),
                Prijs = Decimal.Parse(split[1], CultureInfo.CreateSpecificCulture("NL-nl"))
            };
        }
    }
}
