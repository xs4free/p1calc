using P1Library.Data;
using System.Globalization;

namespace P1Library.Importers
{
    public class ApsImporter
    {
        private const char Delimiter = ',';
        public static async IAsyncEnumerable<EnergyUsage> Import(string fileName)
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
                yield return ParseEnergyUsage(line);
            }
        }

        private static EnergyUsage ParseEnergyUsage(string line)
        {
            var split = line.Split([Delimiter], StringSplitOptions.RemoveEmptyEntries);
            
            return new EnergyUsage
            {
                DateTime = DateTime.ParseExact(split[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                Import = 0,
                Export = Decimal.Parse(split[1], CultureInfo.InvariantCulture)
            };
        }

        private static void ValidateHeader(string line)
        {
            var splitHeader = line.Split([Delimiter], StringSplitOptions.RemoveEmptyEntries);
            if (splitHeader.Length != 2)
            {
                throw new InvalidDataException("Expected header to contain 2 columns: date,energy");
            }
        }
    }
}
