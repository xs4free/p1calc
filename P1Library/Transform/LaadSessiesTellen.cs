using P1Library.Data;

namespace P1Library.Transform
{
    public static class LaadSessiesTellen
    {
        public static async Task<int> Transform(IAsyncEnumerable<EnergyUsage> p1Data)
        {
            var p1DataList = await p1Data.ToListAsync();

            int sessies = 0;
            bool laadpaalActief = false;

            foreach (var item in p1DataList)
            {
                if (item.LaadpaalActief)
                {
                    if (!laadpaalActief)
                    {
                        sessies++;
                        laadpaalActief = true;
                    }
                }
                else
                {
                    laadpaalActief = false;
                }
            }

            return sessies;
        }
    }
}
