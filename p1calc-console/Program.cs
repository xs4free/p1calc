using P1Library.Importers;
using P1Library.Kosten;
using P1Library.Transform;

string fileNameP1 = @"..\..\..\..\Data\p1e-totaal-2024.csv";
string fileNameTarievenDynamisch = @"..\..\..\..\Data\jeroen_punt_nl_dynamische_stroomprijzen_jaar_2024.csv";
string fileNameTarievenHoogLaag = @"..\..\..\..\Data\hoog-laag-tarief.json";

var homeWizardData = HomeWizardP1Importer.Import(fileNameP1);
var energyUsage = HomeWizardP1toEnergyUsagePerHour.Convert(homeWizardData);
var tarievenDynamisch = JeroenDynamischeTarievenImporter.Import(fileNameTarievenDynamisch);
var tarievenHoogLaag = HoogLaagTariefImporter.Import(fileNameTarievenHoogLaag);

decimal import = 0, export = 0;

await foreach(var line in energyUsage)
{
    import += line.Import;
    export += line.Export;
}

Console.WriteLine($"Total Import: {import} - Total Export: {export}");


var kosten = new Kosten();

var totaalDynamisch = await kosten.Bereken(energyUsage, tarievenDynamisch);
Console.WriteLine($"Total dynamische kosten excl belastingen: {totaalDynamisch}");

var totaalHoogLaag = await kosten.Bereken(energyUsage, tarievenHoogLaag);
Console.WriteLine($"Total hoog/laag kosten excl belastingen: {totaalHoogLaag}");