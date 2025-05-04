using P1Library.Importers;
using P1Library.Transform;

string fileName = @"..\..\..\..\Data\p1e-totaal-2024.csv";

var homeWizardData = HomeWizardP1Importer.Import(fileName);
var energyUsage = HomeWizardP1toEnergyUsagePerHour.Convert(homeWizardData);

decimal import = 0, export = 0;

await foreach(var line in energyUsage)
{
    Console.WriteLine($"{line.DateTime} - Import {line.Import} - Export {line.Export} ");
    import += line.Import;
    export += line.Export;
}

Console.WriteLine($"Total Import: {import} - Total Export: {export}");
