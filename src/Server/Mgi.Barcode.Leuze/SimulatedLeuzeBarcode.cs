using System;
using System.Text;
using System.Threading;

namespace Mgi.Barcode.Leuze
{
    public class SimulatedLeuzeBarcode : ILeuzeBarcode
    {
        public SimulatedLeuzeBarcode()
        {

        }
        public void Close()
        {
            Thread.Sleep(100);
        }

        public string ConsequentTrigger()
        {
            Thread.Sleep(10);
            return RandomBarcode();
        }

        public string GetVersion()
        {
            return "Simulated_1.0";
        }

        public void Open()
        {
            Thread.Sleep(100);
        }

        public string SingleTrigger()
        {
            Thread.Sleep(10);
            return RandomBarcode();
        }

        private static readonly string barcodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        static readonly Random random = new Random();
        private string RandomBarcode()
        {
            //if (random.Next(1, 100) > 90)
            //{
            //    throw new Exception("test exception");
            //}
            var sb = new StringBuilder();
            if (random.Next(1, 100) > 90)
            {
                return string.Empty;
            }
            for (int i = 0; i < 16; i++)
            {
                sb.Append(barcodeChars[random.Next(0, barcodeChars.Length)]);
            }
            return sb.ToString();
        }

        public void BeginConsequentTrigger()
        {

        }

        public string EndConsequentTrigger()
        {
            return RandomBarcode();
        }
    }
}
