using P1Library.Data;

namespace P1Library.Transform
{
    public class RemoveEnkelfaseLaadpaal
    {
        public static async IAsyncEnumerable<EnergyUsage> Transform(
            IAsyncEnumerable<EnergyUsage> p1Data,
            int fase,
            int minimumPower,
            TimeSpan minimumDuur)
        {
            var p1DataList = await p1Data.ToListAsync();
            var p1Lines = p1DataList.SelectMany(item => item.P1Lines).ToList();
            HashSet<DateTime> laadpaalTimes = [];

            for (int index = 0; index < p1Lines.Count; index++)
            {
                var line = p1Lines[index];
                var power = fase == 1 ? line.PowerL1 : fase == 2 ? line.PowerL2 : line.PowerL3;
                if (power > minimumPower && index < p1Lines.Count - 1)
                {
                    var possibleLaadpaalTimes = new HashSet<DateTime>
                    {
                        line.DateTime
                    };

                    for (int index2 = index+1; index2 < p1Lines.Count; index2++)
                    {
                        var nextLine = p1Lines[index2];
                        var nextPower = fase == 1 ? nextLine.PowerL1 : fase == 2 ? nextLine.PowerL2 : nextLine.PowerL3;
                        if (nextPower < minimumPower)
                        {
                            var duration = nextLine.DateTime - line.DateTime;
                            if (duration >= minimumDuur)
                            {
                                laadpaalTimes.UnionWith(possibleLaadpaalTimes);
                            }
                            index = index2; // Skip to the next line after the current one
                            break;
                        }
                        else
                        {
                            possibleLaadpaalTimes.Add(nextLine.DateTime);
                        }
                    }
                }
            }

            foreach (var item in p1DataList)
            {
                var linesLaadpaalActief = item.P1Lines.Where(line => laadpaalTimes.Contains(line.DateTime)).ToList();

                if (linesLaadpaalActief.Any())
                {
                    var duration = linesLaadpaalActief.Last().DateTime - linesLaadpaalActief.First().DateTime;
                    var averagePower = linesLaadpaalActief.Average(d => fase == 1 ? (decimal)d.PowerL1 : fase == 2 ? d.PowerL2 : d.PowerL3);
                    var powerUsedByLaadpaal = ((decimal)duration.TotalMinutes + 15) / 60 * averagePower;

                    yield return new EnergyUsage
                    {
                        DateTime = item.DateTime,
                        Import = item.Import - (powerUsedByLaadpaal / 1000),
                        Export = item.Export,
                        P1Lines = item.P1Lines,
                        LaadpaalActief = true,
                    };
                }
                else
                {
                    yield return item;
                }
            }
        }
    }
}
