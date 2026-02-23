using System;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;

namespace EconomySimulation
{
    internal class Program
    {
        static SimConfig _config = LadeConfig();

        static void Main(string[] args)
        {
            //Firmen erzeugen
            List<Firma> firmen = _config.Firmen.Select(f => new Firma(f.Name, f.StartKapital)).ToList();

            // 100 Personen mit Geld, Bedarf und PreisToleranz
            List<Mensch> personen = new();

            for (int i = 0; i < _config.Personen.Anzahl; i++)
            {
                personen.Add(new Mensch
                {
                    Geld = _config.Personen.StartGeld,
                    Bedarf = _config.Personen.Bedarf,
                    PreisToleranz = _config.Personen.PreisToleranz
                });
            }


            // Menschen auf Firmen verteilen
            int firmaIndex = 0;

            foreach (var person in personen)
            {
                var firma = firmen[firmaIndex];

                person.Arbeitgeber = firma; //Arbeitgeber zuweisen
                person.Lohn = firma.LohnProMitarbeiter;

                firma.Mitarbeiter.Add(person); //Mitarbeiter hinzufügen

                firmaIndex = (firmaIndex + 1) % firmen.Count;
            }

            // Markt
            Markt markt = new Markt();

            markt.BasisPreis = UpdateBasisPreis(firmen, 1.4);
            markt.Preis = markt.BasisPreis;
            markt.Angebot = firmen.Sum(f => f.Produktion);
            markt.Nachfrage = MenschenReagieren(personen, markt);

            AusgabeMarkt(markt);


            //Staat
            Staat staat = new Staat();
            staat.Einkommenssteuer = _config.Staat.Einkommenssteuersatz;
            staat.Unternehmenssteuer = _config.Staat.Koerperschaftssteuersatz;
            staat.Mehrwertsteuer = _config.Staat.Mehrwertsteuersatz;
            staat.Sozialhilfe = _config.Staat.Sozialhilfe;

            //Simulation
            Simulation(firmen, personen, markt, staat, _config.Simulation.Runden);
        }

        private static void Simulation(List<Firma> firmen, List<Mensch> personen, Markt markt , Staat staat, int runden)
        {
            for (int i = 0; i < runden; i++)
            {
                Console.WriteLine($"Monat {i + 1}.");

                // Verkaufsmenge zurücksetzen
                firmen.ForEach(f => f.VerkaufteMenge = 0);

                // KapitalLetzterMonat aktualisieren
                firmen.ForEach(f => f.KapitalLetzterMonat = f.Kapital);

                // 1. Basispreis aktualisieren [Kosten * Marge]
                markt.BasisPreis = UpdateBasisPreis(firmen, 1.4);

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
                    Console.WriteLine($"💥 {firma.Name} ist insolvent!");

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

        private static void AusgabeMarkt(Markt markt)
        {
            Console.WriteLine(
                $"Angebot: {Math.Round(markt.Angebot, 2)}" +
                $"\nNachfrage: {Math.Round(markt.Nachfrage, 2)}" +
                $"\nAlter Preis: {Math.Round(markt.BasisPreis, 2)}" +
                $"\nNeuer Preis: {Math.Round(markt.Preis, 2)}\n");
        }

        private static void AusgabeFirmen(List<Firma> firmen)
        {
            firmen.ForEach(f =>
            {
                Console.WriteLine(
                    $"Firma: {f.Name}" +
                    $"\nKapital: {Math.Round(f.Kapital, 2)}" +
                    $"\nProduktion: {Math.Round(f.Produktion, 2)}" +
                    $"\nKosten: {Math.Round(f.Kosten, 2)}" +
                    $"\nKosten/Einheit: {Math.Round(f.KostenProEinheit, 2)}" +
                    $"\nMitarbeiter: {f.Mitarbeiter.Count}" +
                    $"\nLohn/Mitarbeiter: {Math.Round(f.LohnProMitarbeiter, 2)}" +
                    $"\nVerkaufte Menge: {f.VerkaufteMenge}\n");
            });
        }

        private static void AusgabePersonen(List<Mensch> personen)
        {
            personen.ForEach(p =>
            {
                Console.WriteLine(
                    $"Geld: {Math.Round(p.Geld, 2)}" +
                    $"\nBedarf: {Math.Round(p.Bedarf, 2)}" +
                    $"\nPreisToleranz: {Math.Round(p.PreisToleranz, 2)}" +
                    $"\nArbeitgeber: {(p.Arbeitgeber != null ? p.Arbeitgeber.Name : "Arbeitslos")}" +
                    $"\nLohn: {Math.Round(p.Lohn, 2)}\n");
            });
        }

        private static void FirmenVerkaufen(List<Firma> firmen,List<Mensch> personen,Markt markt,Staat staat)
        {
            foreach (var kunde in personen)
            {
                if (kunde.Geld <= 0) continue;

                double nochZuKaufen = kunde.Bedarf;

                var zufaelligeFirmen = firmen.Where(f => f.Produktion > f.VerkaufteMenge).OrderBy(_ => Random.Shared.Next()).ToList();

                foreach (var firma in zufaelligeFirmen)
                {
                    if (nochZuKaufen <= 0) break;

                    double verfuegbar = firma.Produktion - firma.VerkaufteMenge;
                    double kaufMenge = Math.Min(nochZuKaufen, verfuegbar);
                    double brutto = kaufMenge * markt.Preis;

                    // Kann sich der Kunde das leisten?
                    if (brutto > kunde.Geld)
                    {
                        kaufMenge = Math.Floor(kunde.Geld / markt.Preis);
                        brutto = kaufMenge * markt.Preis;
                    }

                    if (kaufMenge <= 0) continue;

                    // Geld aufteilen: MwSt an Staat, Rest an Firma
                    double mwst = brutto * staat.Mehrwertsteuer;
                    kunde.Geld -= brutto;
                    firma.Kapital += brutto - mwst;
                    staat.BudgetErhoehen(mwst);
                    firma.VerkaufteMenge += (int)kaufMenge;
                    nochZuKaufen -= kaufMenge;
                }
            }
        }

        private static void FirmenReagieren(List<Firma> firmen, List<Mensch> personen)
        {
            foreach (var firma in firmen)
            {
                // Nicht globale Marktlage – sondern eigene Verkaufsquote
                double auslastung = firma.VerkaufteMenge / Math.Max(firma.Produktion, 1);

                if (auslastung >= 0.9) // 90% verkauft → einstellen
                {
                    var arbeitsloser = personen.FirstOrDefault(p => p.Arbeitgeber == null);
                    if (arbeitsloser != null)
                    {
                        arbeitsloser.Arbeitgeber = firma;
                        arbeitsloser.Lohn = firma.LohnProMitarbeiter;
                        firma.Mitarbeiter.Add(arbeitsloser);
                    }
                }
                else if (auslastung < 0.5) // unter 50% verkauft → feuern
                {
                    if (firma.Mitarbeiter.Count > 1)
                    {
                        var mitarbeiter = firma.Mitarbeiter[^1];
                        mitarbeiter.Arbeitgeber = null;
                        mitarbeiter.Lohn = 0;
                        firma.Mitarbeiter.RemoveAt(firma.Mitarbeiter.Count - 1);
                    }
                }
            }

        }

        private static double MenschenReagieren(List<Mensch> personen, Markt markt)
        {
            return personen.Sum(p =>
            {
                if (p.Geld <= 0)
                    return 0;

                double kaufFaktor = 1 - (markt.Preis / markt.BasisPreis - 1) * p.PreisToleranz;

                kaufFaktor = Math.Clamp(kaufFaktor, 0, 1);

                double maximalKaufbar = p.Geld / markt.Preis;
                double kaufMenge = Math.Min(p.Bedarf * kaufFaktor, maximalKaufbar);

                return kaufMenge;
            });
        }

        private static void FirmenZahlenLoehne(List<Firma> firmen)
        {
            foreach (var firma in firmen)
            {
                foreach (var mitarbeiter in firma.Mitarbeiter)
                {
                    mitarbeiter.Geld += firma.LohnProMitarbeiter;
                    firma.Kapital -= firma.LohnProMitarbeiter;
                }
            }
        }

        private static double GeldInUmlauf(List<Firma> firmen, List<Mensch> personen, Staat staat)
        {
            return personen.Sum(p => p.Geld)
                 + firmen.Sum(f => f.Kapital)
                 + staat.Budget; // kommt in Phase 2
        }

        public static double UpdateBasisPreis(List<Firma> firmen, double marge)
        {
            var aktiveFirmen = firmen.Where(f => f.Produktion > 0).ToList();
            if (aktiveFirmen.Count == 0) return double.MaxValue;
            return aktiveFirmen.Average(f => f.KostenProEinheit) * marge;
        }

        private static SimConfig LadeConfig()
        {
            string projektPfad = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            string configPath = Path.Combine(projektPfad, "config.json");
            string json = File.ReadAllText(configPath);
            if (string.IsNullOrEmpty(json))
            {
                throw new Exception("Die Konfigurationsdatei ist leer oder konnte nicht gefunden werden.");
            }
            else
            {
                return JsonSerializer.Deserialize<SimConfig>(json);
            }
        }
    }
}
