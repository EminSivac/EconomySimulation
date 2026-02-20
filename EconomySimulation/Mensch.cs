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
        public double Bedarf;        // wie viel er pro Tick will
        public double PreisToleranz; // wie stark Preis ihn abschreckt

        public Firma? Arbeitgeber;
        public double Lohn;

        public double sparQuote; // wie viel er von seinem Geld spart
    }
}
