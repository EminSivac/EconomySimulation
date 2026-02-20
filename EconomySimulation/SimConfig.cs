using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomySimulation
{
    public class SimConfig
    {
        public SimulationConfig Simulation { get; set; }
        public PersonenConfig Personen { get; set; }
        public List<FirmaConfig> Firmen { get; set; }
        public StaatConfig Staat { get; set; }
        public MarktConfig Markt { get; set; }
    }

    public class SimulationConfig
    {
        public int Runden { get; set; }
        public int ZufallsSeed { get; set; }
    }

    public class PersonenConfig
    {
        public int Anzahl { get; set; }
        public double StartGeld { get; set; }
        public double Bedarf { get; set; }
        public double PreisToleranz { get; set; }
    }

    public class FirmaConfig
    {
        public string Name { get; set; }
        public double StartKapital { get; set; }
    }

    public class StaatConfig
    {
        public double Einkommenssteuersatz { get; set; }
        public double Koerperschaftssteuersatz { get; set; }
        public double Mehrwertsteuersatz { get; set; }
        public double Sozialhilfe { get; set; }
        public double Schuldengrenze { get; set; }
    }

    public class MarktConfig
    {
        public double Elastizitaet { get; set; }
        public double Marge { get; set; }
    }
}
