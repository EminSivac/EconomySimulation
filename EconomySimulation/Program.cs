using System;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace EconomySimulation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Firmen erzeugen

            Firma firma1 = new Firma("HolzProdukt", 100000);

            Firma firma2 = new Firma("HolzProdukt2", 150000);

            Firma firma3 = new Firma("HolzProdukt3", 80000);

            List<Firma> firmen = new List<Firma>() { firma1, firma2, firma3 };

            // 100 Personen mit Geld, Bedarf und PreisToleranz
            List<Mensch> personen = new();

            for (int i = 0; i < 1000; i++)
            {
                personen.Add(new Mensch
                {
                    Geld = 1000,
                    Bedarf = 1,
                    PreisToleranz = 0.5
                });
            }

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
            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine($"Runde {i + 1}");

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

            Console.WriteLine("Simulation beendet.");
            Console.WriteLine("Endpreise der Firmen:");
            foreach (var firma in firmen)
            {
                Console.WriteLine($"{firma.Name}: Kapital = {Math.Round(firma.Kapital, 2)}, Mitarbeiter = {firma.Mitarbeiter.Count}");
            }
            foreach (var person in personen)
            {
                Console.WriteLine($"Person: Geld = {Math.Round(person.Geld, 2)}, Arbeitgeber = {(person.Arbeitgeber != null ? person.Arbeitgeber.Name : "Arbeitslos")}");
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

        private static void FirmenVerkaufen(
            List<Firma> firmen,
            List<Mensch> personen,
            Markt markt)
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


        private static void FirmenReagieren(
            List<Firma> firmen,
            List<Mensch> personen,
            Markt markt)
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
    }
}
