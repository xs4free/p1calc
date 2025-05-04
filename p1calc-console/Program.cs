using P1Library.Data;
using P1Library.Importers;
using P1Library.Kosten;
using P1Library.Transform;

string fileNameP1 = @"..\..\..\..\Data\p1e-totaal-2024.csv";
string fileNameTarievenDynamisch = @"..\..\..\..\Data\jeroen_punt_nl_dynamische_stroomprijzen_jaar_2024.csv";
string fileNameTarievenHoogLaag = @"..\..\..\..\Data\hoog-laag-tarief.json";
string fileNameAps = @"..\..\..\..\Data\aps-totaal-2024.csv";

var homeWizardData = HomeWizardP1Importer.Import(fileNameP1);
var energyUsage = HomeWizardP1toEnergyUsagePerHour.Convert(homeWizardData);
var tarievenDynamisch = JeroenDynamischeTarievenImporter.Import(fileNameTarievenDynamisch);
var tarievenHoogLaag = HoogLaagTariefImporter.Import(fileNameTarievenHoogLaag);
var energyProductieZonnepanelen = ApsImporter.Import(fileNameAps);
var energyUsageZonderZonnepanelen = RemoveZonnepaneelProductie.Tranform(energyUsage, energyProductieZonnepanelen);

(decimal importP1, decimal exportP1) = await TotaalImportExport(energyUsage);
Console.WriteLine($"Total Import: {importP1} - Total Export: {exportP1} P1");

(decimal importAPS, decimal exportAPS) = await TotaalImportExport(energyProductieZonnepanelen);
Console.WriteLine($"Total Import: {importAPS} - Total Export: {exportAPS} APS");

(decimal importZonderAPS, decimal exportZonderAPS) = await TotaalImportExport(energyUsageZonderZonnepanelen);
Console.WriteLine($"Total Import: {importZonderAPS} - Total Export: {exportZonderAPS} zonder APS");

var kosten = new Kosten();

var totaalHoogLaagMetAps = await kosten.Bereken(energyUsage, tarievenHoogLaag);
Console.WriteLine($"Total hoog/laag kosten excl belastingen: {totaalHoogLaagMetAps}");
var totaalHoogLaagZonderAps = await kosten.Bereken(energyUsageZonderZonnepanelen, tarievenHoogLaag);
Console.WriteLine($"Total hoog/laag kosten excl belastingen zonder APS: {totaalHoogLaagZonderAps}");

var totaalDynamischMetAps = await kosten.Bereken(energyUsage, tarievenDynamisch);
Console.WriteLine($"Total dynamische kosten excl belastingen: {totaalDynamischMetAps}");
var totaalDynamischZonderAps = await kosten.Bereken(energyUsageZonderZonnepanelen, tarievenDynamisch);
Console.WriteLine($"Total dynamische kosten excl belastingen zonder APS: {totaalDynamischZonderAps}");

async Task<(decimal import, decimal export)> TotaalImportExport(IAsyncEnumerable<EnergyUsage> energyUsage)
{
    decimal import = 0;
    decimal export = 0;
    
    await foreach (var line in energyUsage)
    {
        import += line.Import;
        export += line.Export;
    }

    return (import, export);
}