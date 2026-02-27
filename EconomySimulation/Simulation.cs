using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomySimulation
{
    public class Simulation
    {

        public Simulation(List<Firma> firmen, List<Mensch> personen, Markt markt, Staat staat, int runden)
        {
            for (int i = 0; i < runden; i++)
            {
                Console.WriteLine($"Monat {i + 1}.");

                // Verkaufsmenge zurücksetzen
                firmen.ForEach(f => f.VerkaufteMenge = 0);

                // KapitalLetzterMonat aktualisieren
                firmen.ForEach(f => f.KapitalLetzterMonat = f.Kapital);

                // 1. Basispreis aktualisieren [Kosten * Marge]
                markt.BasisPreis = UpdateBasisPreis(firmen, _config.Markt.Marge);

                // 2. Angebot aus Firmen
                markt.Angebot = firmen.Sum(f => f.Produktion);

                // 3. Nachfrage aus Personen
                markt.Nachfrage = MenschenReagieren(personen, markt);

                // 4. Preis anpassen
                markt.updatePrice();

                // 6. Firmen zahlen Löhne
                FirmenZahlenLoehne(firmen);

                // 5. Firmen verkaufen zum neuen Preis
                FirmenVerkaufen(firmen, personen, markt, staat);//Es fehlt die MwSt.

                // Insolvenz prüfen
                foreach (var firma in firmen.Where(f => f.Kapital < -f.Kosten * 2).ToList())
                {
                    Console.WriteLine($"{firma.Name} ist insolvent!");

                    if (firma.Kapital < 0)
                    {
                        staat.Budget += firma.Kapital; // Staat trägt Schulden
                        firma.Kapital = 0;
                    }

                    foreach (var m in firma.Mitarbeiter)
                    {
                        m.Arbeitgeber = null;
                        m.Lohn = 0;
                    }

                    firmen.Remove(firma);
                }
                if (firmen.Count == 0) break;

                // 7. Einkommenssteuer
                staat.EinkommenVersteuern(personen);

                // 8. Körperschaftssteuer
                staat.UnternehmensVersteuern(firmen);

                // 9. Sozialhilfen an Arbeitslose
                staat.SozialhilfeAnArbeitslose(personen);

                // 10. Firmen reagieren (Einstellen/Feuern)
                FirmenReagieren(firmen, personen);

                // 11. Augabe
                AusgabeMarkt(markt);
                //AusgabeFirmen(firmen);
            }

            Console.WriteLine($"Staatbudget:       {Math.Round(staat.Budget, 2)}");
            Console.WriteLine($"Firmenkapital:     {Math.Round(firmen.Sum(f => f.Kapital), 2)}");
            Console.WriteLine($"Personengeld:      {Math.Round(personen.Sum(p => p.Geld), 2)}");
            Console.WriteLine($"─────────────────────────────");
            Console.WriteLine($"Geld in Umlauf:    {Math.Round(GeldInUmlauf(firmen, personen, staat), 2)}");
            Console.WriteLine();

            personen.Select(p => p.Arbeitgeber != null ? p.Arbeitgeber.Name : "Arbeitslos").GroupBy(a => a).ToList().ForEach(g =>
            {
                Console.WriteLine($"{g.Key}: {g.Count()}");
            });

            //AusgabePersonen(personen);
        }




    }
}
