namespace EconomySimulation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Firma firma1 = new Firma();
            firma1.Name = "HolzProdukt";


            Firma firma2 = new Firma();
            firma2.Name = "HolzProdukt2";

            Markt markt = new Markt();
            markt.Angebot = 100;
            markt.Nachfrage = 150;

            markt.updatePrice();
        }
    }
}
