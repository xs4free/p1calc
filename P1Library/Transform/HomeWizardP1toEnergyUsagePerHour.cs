using P1Library.Data;

namespace P1Library.Transform
{
    public static class HomeWizardP1toEnergyUsagePerHour
    {
        public static async IAsyncEnumerable<EnergyUsage> Convert(IAsyncEnumerable<P1Data> data)
        {
            List<P1Data> previousData = [];

            await foreach(var item in data)
            {
                // gebruik > om wissel naar zomertijd op te vangen
                if (previousData.Any() &&
                    item.DateTime >= previousData.First().DateTime.AddHours(1))
                {
                    yield return new EnergyUsage
                    {
                        DateTime = previousData.First().DateTime,
                        Import = (item.ImportT1 - previousData.First().ImportT1) + (item.ImportT2 - previousData.First().ImportT2),
                        Export = (item.ExportT1 - previousData.First().ExportT1) + (item.ExportT2 - previousData.First().ExportT2),
                        P1Lines = previousData
                    };
                    previousData = [item];
                }
                else
                {
                    previousData.Add(item);
                }
            }
        }
    }
}
