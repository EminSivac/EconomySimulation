using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomySimulation
{
    public class Produkt
    {
        public string Name;

        public double Preis;

        // Wie stark erfüllt das Produkt Bedürfnisse
        public Dictionary<Mensch.Beduerfnis, double> Nutzen = new();
    }
}
