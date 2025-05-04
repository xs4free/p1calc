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

var kosten = new Kosten();

var totaalHoogLaagMetAps = await kosten.Bereken(energyUsage, tarievenHoogLaag);
var totaalHoogLaagZonderAps = await kosten.Bereken(energyUsageZonderZonnepanelen, tarievenHoogLaag);

var totaalDynamischMetAps = await kosten.Bereken(energyUsage, tarievenDynamisch);
var totaalDynamischZonderAps = await kosten.Bereken(energyUsageZonderZonnepanelen, tarievenDynamisch);

(decimal importP1, decimal exportP1) = await TotaalImportExport(energyUsage);
(decimal _, decimal exportAPS) = await TotaalImportExport(energyProductieZonnepanelen);
(decimal importZonderAPS, decimal exportZonderAPS) = await TotaalImportExport(energyUsageZonderZonnepanelen);

Console.WriteLine($"P1:");
Console.WriteLine($"Afname: {Math.Round(importP1)} kWh");
Console.WriteLine($"Terugleveren: {Math.Round(exportP1)} kWh");
Console.WriteLine();

Console.WriteLine($"Zonnepanelen (APS):");
Console.WriteLine($"Totaal geproduceerd: {exportAPS} kWh");
Console.WriteLine($"Eigenverbruik: {exportAPS-exportP1} kWh ({Math.Round((exportAPS - exportP1) / exportAPS * 100)}%)");
Console.WriteLine();

Console.WriteLine($"Scenario met zonnepanelen:");
Console.WriteLine($"Afname: {Math.Round(importP1)} kWh");
Console.WriteLine($"Terugleveren: {Math.Round(exportP1)} kWh");
Console.WriteLine($"Kosten dynamische contract excl belastingen: {totaalDynamischMetAps:C}");
Console.WriteLine($"Kosten hoog/laag contract excl belastingen: {totaalHoogLaagMetAps:C}");
Console.WriteLine();

Console.WriteLine($"Scenario zonder zonnepanelen:");
Console.WriteLine($"Afname: {Math.Round(importZonderAPS)} kWh");
Console.WriteLine($"Terugleveren: {Math.Round(exportZonderAPS)} kWh");
Console.WriteLine($"Kosten hoog/laag contract excl belastingen: {totaalHoogLaagZonderAps:C}");
Console.WriteLine($"Kosten dynamische contract excl belastingen: {totaalDynamischZonderAps:C}");
Console.WriteLine();

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