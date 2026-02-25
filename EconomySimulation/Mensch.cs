using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomySimulation
{
    public class Mensch
    {
        public double Geld;

        // alter Gesamtbedarf (kannst du behalten für Debug / Skalierung)
        public double Bedarf;

        public double PreisToleranz;

        public Firma? Arbeitgeber;
        public double Lohn;

        public enum Beduerfnis
        {
            Essen,
            Wohnen,
            Mobilitaet,
            Freizeit,
            Sozial
        }

        public Dictionary<Beduerfnis, double> BedarfVektor = new();

        public Dictionary<Beduerfnis, double> Erfuellt = new();

        public void ResetBeduerfnisse()
        {
            foreach (var key in BedarfVektor.Keys.ToList())
                Erfuellt[key] = 0;
        }
    }
}
