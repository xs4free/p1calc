# p1calc
Berekening electrakosten op basis van p1-meter logs


# Project overview
DataModel

Importer:
- HomeWizard
    - combineer meerdere bestanden tot 1 jaar
    - combineer kwartieren naar uren per dag
- Dynamische tarieven
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
- vast contract 
- dynamisch contract

