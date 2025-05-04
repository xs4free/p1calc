using P1Library.Data;

namespace P1Library.Transform
{
    public static class HomeWizardP1toEnergyUsagePerHour
    {
        public static async IAsyncEnumerable<EnergyUsage> Convert(IAsyncEnumerable<P1Data> data)
        {
            P1Data? previousData = null;

            await foreach(var item in data)
            {
                if (previousData == null)
                {
                    previousData = item;
                    continue;
                }

                // gebruik > om wissel naar zomertijd op te vangen
                if (item.DateTime >= previousData.DateTime.AddHours(1))
                {
                    yield return new EnergyUsage
                    {
                        DateTime = item.DateTime,
                        Import = (item.ImportT1 - previousData.ImportT1) + (item.ImportT2 - previousData.ImportT2),
                        Export = (item.ExportT1 - previousData.ExportT1) + (item.ExportT2 - previousData.ExportT2)
                    };
                    previousData = item;
                }
            }
        }
    }
}
