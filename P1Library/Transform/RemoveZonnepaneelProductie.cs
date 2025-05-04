using P1Library.Data;

namespace P1Library.Transform
{
    public class RemoveZonnepaneelProductie
    {
        public static async IAsyncEnumerable<EnergyUsage> Tranform(
            IAsyncEnumerable<EnergyUsage> p1Data,
            IAsyncEnumerable<EnergyUsage> apsData)
        {
            var apsIndex = await apsData.ToDictionaryAsync(x => x.DateTime);

            await foreach (var p1Item in p1Data)
            {
                if (apsIndex.TryGetValue(p1Item.DateTime, out var apsItem))
                {
                    var newExport = 0;
                    var selfConsumption = apsItem.Export - p1Item.Export;
                    var newImport = p1Item.Import + selfConsumption;

                    yield return new EnergyUsage
                    {
                        DateTime = p1Item.DateTime,
                        Import = newImport,
                        Export = newExport
                    };
                }
                else
                {
                    yield return p1Item;
                }
            }
        }
    }
}