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
            // Produkte erzeugen
            List<Produkt> produkte = ErzeugeProdukte();

            //Firmen erzeugen
            List<Firma> firmen = ErzeugeFirmen();

            // 100 Personen mit Geld, Bedarf und PreisToleranz
            List<Mensch> personen = ErzeugeMenschen();

            // Menschen auf Firmen verteilen
            MenschenArbeitenGeben(firmen, personen);

            // Markt
            Markt markt = new Markt();

            markt.BasisPreis = UpdateBasisPreis(firmen, _config.Markt.Marge);
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
            Simulation simulation = new Simulation(firmen, personen, markt, staat, _config);
        }

        private static void MenschenArbeitenGeben(List<Firma> firmen, List<Mensch> personen)
        {
            int firmaIndex = 0;

            foreach (var person in personen)
            {
                var firma = firmen[firmaIndex];

                person.Arbeitgeber = firma; //Arbeitgeber zuweisen
                person.Lohn = firma.LohnProMitarbeiter;

                firma.Mitarbeiter.Add(person); //Mitarbeiter hinzufügen

                firmaIndex = (firmaIndex + 1) % firmen.Count;
            }
        }

        private static List<Mensch> ErzeugeMenschen()
        {
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

            return personen;
        }

        private static List<Produkt> ErzeugeProdukte()
        {
            return _config.Produkte.Select(p => new Produkt
            {
                Name = p.Name,
                Preis = p.Preis,
                Nutzen = p.Nutzen
            }).ToList();
        }

        private static void AusgabeMarkt(Markt markt)
        {
            Console.WriteLine(
                $"Angebot: {Math.Round(markt.Angebot, 2)}" +
                $"\nNachfrage: {Math.Round(markt.Nachfrage, 2)}" +
                $"\nAlter Preis: {Math.Round(markt.BasisPreis, 2)}" +
                $"\nNeuer Preis: {Math.Round(markt.Preis, 2)}\n");
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


        private static List<Firma> ErzeugeFirmen()
        {
            return _config.Firmen.Select(f => new Firma(f.Name, f.StartKapital)).ToList();
        }
    }
}
