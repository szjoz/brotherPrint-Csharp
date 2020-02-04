using System;
using System.Text;
using System.Threading;
using LabelPrinter.Lib;
using System.Linq;

namespace LabelPrinterExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello in Brother Printer Example");

            LabelPrinter.Lib.BrotherPrint brother = new LabelPrinter.Lib.BrotherPrint("192.168.100.109", "9100"); //Set Printer IP Addresss and Port. Default Port is 9100.
            //LabelPrinter.Lib.BrotherPrint brother = new LabelPrinter.Lib.BrotherPrint("COM6", 9600); //Set Printer Serial Port Name and Baud Rate. Default Port is 9600.

            brother.Open(); // Open ports

            // ******************** EXAMPLE: PRINT SIMPLE LABEL ********************
            brother.sendCommand(brother.template_mode);
            brother.sendCommand(brother.init_template);

            Console.WriteLine("Brother Printer Status: {0}", brother.statusRequest(32));

            Console.WriteLine("Brother Printer Version: {0}", brother.versionRequest(16));

            brother.sendCommand(brother.command_mode);

            brother.sendCommand(brother.initialize);
            
            //brother.sendCommand(brother.select_font, (byte)Font.Helsinkibit);

            //brother.sendCommand(brother.char_size, (byte)CharSize.Size12Point);

            //brother.sendCommand(brother.bold, (byte)Bold.On);

            brother.sendCommand(brother.italic, (byte)Italic.On);

            //brother.sendCommand(brother.underline, (byte)Underline.On);

            //brother.sendCommand(brother.double_width, (byte)DoubleWidth.On);

            brother.sendCommand("Hello from C#");

            //brother.sendCommand(brother.cut_setting, (byte)CutSetting.Half);

            brother.sendCommand(brother.page_feed);
            // ******************** EXAMPLE END: PRINT SIMPLE LABEL ********************



            // ******************** EXAMPLE: PRINT FROM STORED TEMPLATE ********************
            //brother.sendCommand(brother.template_mode);

            ////brother.sendCommandWithRequest(brother.status_request);

            //brother.sendCommand(brother.init_template);

            //brother.chooseTemplate(1);

            //brother.sendCommand(brother.select_object, "TEXT1"); // Object selection in template

            //brother.insertIntoObject("TEST1");

            ////brother.sendCommand(brother.direct_object_insertion, Encoding.ASCII.GetBytes("Test 0001")); //Example with direct object insertion

            //brother.sendCommand(brother.number_copies, 1); // Number of copies

            //brother.sendCommand(brother.print_template);
            // ******************** EXAMPLE END: PRINT FROM STORED TEMPLATE ********************


            Thread.Sleep(100);

            brother.Close(); //Close ports

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }
    }
}
