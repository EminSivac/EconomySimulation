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

                person.Arbeitgeber = firma;
                person.Lohn = firma.LohnProMitarbeiter;

                firma.Mitarbeiter.Add(person);

                firmaIndex = (firmaIndex + 1) % firmen.Count;
            }

            // Markt
            Markt markt = new Markt();

            markt.BasisPreis = firmen.Average(f => f.Kosten) * 1.4;
            markt.Preis = markt.BasisPreis;
            markt.Angebot = firmen.Sum(f => f.Produktion);
            markt.Nachfrage = MenschenReagieren(personen, markt);

            AusgabeMarkt(markt);

            //Simulation
            Simulation(firmen, personen, markt, _config.Simulation.Runden);
        }

        private static void Simulation(List<Firma> firmen, List<Mensch> personen, Markt markt, int runden)
        {
            for (int i = 0; i < runden; i++)
            {
                Console.WriteLine($"Monat {i + 1}.");

                // 1. Angebot aus Firmen
                markt.Angebot = firmen.Sum(f => f.Produktion);

                // 2. Nachfrage aus Personen
                markt.Nachfrage = MenschenReagieren(personen, markt);

                // 3. Preis anpassen
                markt.updatePrice();

                // 4. Firmen verkaufen zum neuen Preis
                FirmenVerkaufen(firmen, personen, markt);

                // 5 Firmen zahlen Löhne
                FirmenZahlenLoehne(firmen);

                // 6. Firmen reagieren
                FirmenReagieren(firmen, personen, markt);

                AusgabeMarkt(markt);
            }
        }

        private static void AusgabeMarkt(Markt markt)
        {
            Console.WriteLine(
                $"Angebot: {Math.Round(markt.Angebot, 2)}" +
                $"\nNachfrage: {Math.Round(markt.Nachfrage, 2)}" +
                $"\nAlter Preis: {Math.Round(markt.BasisPreis, 2)}" +
                $"\nNeuer Preis: {Math.Round(markt.Preis, 2)}\n");
        }

        private static void FirmenVerkaufen(List<Firma> firmen, List<Mensch> personen, Markt markt)
        {
            double restNachfrage = markt.Nachfrage;

            foreach (var firma in firmen)
            {
                if (restNachfrage <= 0)
                    break;

                double verkaufbareMenge = firma.Produktion;

                foreach (var kunde in personen)
                {
                    if (restNachfrage <= 0 || verkaufbareMenge <= 0)
                        break;

                    if (kunde.Geld <= 0)
                        continue;

                    double maxKauf = Math.Min(kunde.Bedarf, verkaufbareMenge);
                    double kosten = maxKauf * markt.Preis;

                    if (kosten > kunde.Geld)
                    {
                        maxKauf = kunde.Geld / markt.Preis;
                        kosten = maxKauf * markt.Preis;
                    }

                    kunde.Geld -= kosten;
                    firma.Kapital += kosten;

                    verkaufbareMenge -= maxKauf;
                    restNachfrage -= maxKauf;
                }
            }
        }

        private static void FirmenReagieren(List<Firma> firmen, List<Mensch> personen, Markt markt)
        {
            foreach (var firma in firmen)
            {
                double marktLage = markt.Nachfrage - markt.Angebot;

                // EINSTELLEN
                if (marktLage > 0)
                {
                    var arbeitsloser = personen
                        .FirstOrDefault(p => p.Arbeitgeber == null);

                    if (arbeitsloser != null)
                    {
                        arbeitsloser.Arbeitgeber = firma;
                        arbeitsloser.Lohn = firma.LohnProMitarbeiter;
                        firma.Mitarbeiter.Add(arbeitsloser);
                    }
                }

                // FEUERN
                else if (marktLage < 0 && firma.Mitarbeiter.Count > 0)
                {
                    var mitarbeiter = firma.Mitarbeiter[^1];

                    mitarbeiter.Arbeitgeber = null;
                    mitarbeiter.Lohn = 0;

                    firma.Mitarbeiter.RemoveAt(firma.Mitarbeiter.Count - 1);
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

                double kaufMenge = p.Bedarf * kaufFaktor;

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
