using P1Library.Data;

namespace P1Library.Kosten
{
    public class Kosten
    {
        private List<DateTime> sortedTarieven = [];
        private Dictionary<DateTime, Tarief> tarievenIndex = [];

        public async Task<decimal> Bereken(IAsyncEnumerable<EnergyUsage> energyUsage, IAsyncEnumerable<Tarief> tarieven)
        {
            decimal kosten = 0;
            decimal import = 0, export = 0;

            await CreateSortedTarieven(tarieven);

            var energyUsageList = await energyUsage.ToListAsync();
            foreach (var usage in energyUsageList)
            {
                var closestTariefDate = FindClosestDate(usage.DateTime);
                var tarief = tarievenIndex[closestTariefDate];

                if (tarief != null)
                {
                    import += usage.Import;
                    export += usage.Export;
                    kosten += (usage.Import * tarief.Prijs) - (usage.Export * tarief.Prijs);
                }
            }
            return kosten;
        }

        private async Task<List<Tarief>> CreateSortedTarieven(IAsyncEnumerable<Tarief> tarieven)
        {
            sortedTarieven.Clear();
            tarievenIndex.Clear();

            var tarievenList = await tarieven.ToListAsync();
            foreach (var tarief in tarievenList)
            {
                sortedTarieven.Add(tarief.Start);
                tarievenIndex[tarief.Start] = tarief;
            }
            sortedTarieven.Sort();
            return tarievenList;
        }

        private DateTime FindClosestDate(DateTime target)
        {
            // Use binary search to find the closest date
            int index = sortedTarieven.BinarySearch(target);

            if (index >= 0)
            {
                // Exact match found
                return sortedTarieven[index];
            }
            else
            {
                // BinarySearch returns a negative index if no exact match is found.
                // The bitwise complement (~) of the index gives the insertion point.
                index = ~index;

                // Determine the closest date
                DateTime before = index > 0 ? sortedTarieven[index - 1] : DateTime.MinValue;
                DateTime after = index < sortedTarieven.Count ? sortedTarieven[index] : DateTime.MaxValue;

                if (before == DateTime.MinValue) return after;
                if (after == DateTime.MaxValue) return before;

                return (target - before) <= (after - target) ? before : after;
            }
        }
    }

    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            var list = new List<T>();
            await foreach (var item in source)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
