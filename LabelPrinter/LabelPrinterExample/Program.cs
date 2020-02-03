using System;
using System.Text;
using System.Threading;
using LabelPrinter.Lib;

namespace LabelPrinterExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello in Brother Printer Example");

            LabelPrinter.Lib.BrotherPrint brother = new LabelPrinter.Lib.BrotherPrint("192.168.100.109", "9100"); //Set Printer IP Addresss and Port. Default Port is 9100.

            brother.Open();
            brother.sendCommand(brother.command_mode);
            brother.sendCommand(brother.initialize);
            brother.sendCommand(Encoding.ASCII.GetBytes("Hello from C#"));
            brother.print_page();

            Thread.Sleep(100);

            brother.Close();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }
    }
}
