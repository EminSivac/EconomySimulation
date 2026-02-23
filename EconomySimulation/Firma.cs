using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EconomySimulation
{
    public class Firma
    {
        public string Name;

        public double Kapital;

        public double KapitalLetzterMonat;

        public double Produktion => Mitarbeiter.Count * 1;

        public double Kosten => Mitarbeiter.Count * LohnProMitarbeiter;

        public double KostenProEinheit => Produktion > 0 ? Kosten / Produktion : 0;

        public List<Mensch> Mitarbeiter = new();

        public double LohnProMitarbeiter = 50;
        public int VerkaufteMenge = 0;

        public Firma(string name, double kapital)
        {
            Name = name;
            Kapital = kapital;
        }

        public double BerechneProduktion()
        {
            return Mitarbeiter.Count * 2;
        }
    }
}
