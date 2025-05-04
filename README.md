# p1calc
Berekening electrakosten op basis van p1-meter logs


# Project overview
DataModel

Importer:
V HomeWizard
    - combineer meerdere bestanden tot 1 jaar
    V combineer kwartieren naar uren per dag
V Dynamische tarieven
    V Jeroen.nl dynamische tarieven csv
V Vaste hoog/laag tarieven Van De Bron
- Filters
    - Laadpaal eruit (op basis van langdurig verbruik op 1 fase)
- Zonnepanelen opbrengst
    - https://apsystemsema.com/

Simulatoren:
- Zonnepanelen
- Accu
- Laadpaal 1 fase (granny-charger)
- Laadpaal 3 fase

Kosten berekenen
V vast contract 
V dynamisch contract

