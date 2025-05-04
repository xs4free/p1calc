using P1Library.Data;
using System.Globalization;

namespace P1Library.Importers
{
    internal record P1MeterValues(decimal T1Import, decimal T2Import, decimal T1Export, decimal T2Export);

    public static class HomeWizardP1Importer
    {
        public static async IAsyncEnumerable<P1Data> Import(string fileName)
        {
            bool headerParsed = false;

            await foreach(var line in File.ReadLinesAsync(fileName))
            {
                if (!headerParsed)
                {
                    ValidateHeader(line);
                    headerParsed = true;
                    continue;
                }

                yield return ParseP1Data(line);
            }
        }

        private static void ValidateHeader(string line)
        {
            var splitHeader = line.Split([','], StringSplitOptions.RemoveEmptyEntries);
            if (splitHeader.Length != 8)
            {
                throw new InvalidDataException("Expected header to contain 8 columns: time,Import T1 kWh,Import T2 kWh,Export T1 kWh,Export T2 kWh,L1 max W,L2 max W,L3 max W.");
            }
        }

        private static P1Data ParseP1Data(string line)
        {
            var split = line.Split([','], StringSplitOptions.RemoveEmptyEntries);

            return new P1Data
            {
                DateTime = DateTime.ParseExact(split[0], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                ImportT1 = Decimal.Parse(split[1], CultureInfo.InvariantCulture),
                ImportT2 = Decimal.Parse(split[2], CultureInfo.InvariantCulture),
                ExportT1 = Decimal.Parse(split[3], CultureInfo.InvariantCulture),
                ExportT2 = Decimal.Parse(split[4], CultureInfo.InvariantCulture),
                PowerL1 = int.Parse(split[5], CultureInfo.InvariantCulture),
                PowerL2 = int.Parse(split[6], CultureInfo.InvariantCulture),
                PowerL3 = int.Parse(split[7], CultureInfo.InvariantCulture)
            };
        }
    }
}
