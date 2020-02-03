using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace LabelPrinter.Lib
{
    public class BrotherPrint
    {
        public enum CutSetting : byte
        {
            Full = 0b00000001,
            Half = 0b00000010,
            Chain = 0b00000100,
            Special = 0b00001000
        };

        public enum FontTypes : byte
        {
            Bitmap = 0x00,
            Outline = 0x01
        };

        public enum Interface
        {
            None,
            Tcp, //Wifi, LAN
            Serial //RS232
        };

        public enum Charsets : byte
        {
            USA = 0,
            France = 1,
            Germany = 2,
            UK = 3,
            Denmark = 4,
            Sweden = 5,
            Italy = 6,
            Spain = 7,
            Japan = 8,
            Norway = 9,
            Denmark2 = 10,
            Spain2 = 11,
            Latin_America = 12,
            South_Korea = 13,
            Legal = 64
        }
        public enum tables : byte
        {
            standard = 0,
            eastern_uropean = 1,
            western_european = 2,
            spare = 3

        }

        public readonly byte[] command_mode = { 0x1b, 0x69, 0x61, 0x00 };
        public readonly byte[] raster_mode = { 0x1b, 0x69, 0x61, 0x01 };
        public readonly byte[] template_mode = { 0x1b, 0x69, 0x61, 0x33 };
        public readonly byte[] initialize = { 0x1B, 0x40 };
        public readonly byte[] page_feed = { 0x0c };
        public readonly byte[] cut_setting = { 0x1b, 0x69, 0x43}; // add one byte parameter CutSetting to array, 
        public readonly byte[] select_charset = { 0x1b, 0x52};  // add one byte parameter Charsets to array, 

        private static TcpClient tcpClient = null;
        private static NetworkStream networkStream = null;
        private IPAddress ipAddress = null;
        private IPEndPoint ipEndPoint = null;

        Interface printerInterface = Interface.None;

        public BrotherPrint(string ipAddressString, string ipPortString)
        {
            this.ipAddress = IPAddress.Parse(ipAddressString);
            this.ipEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(ipPortString));
            this.printerInterface = Interface.Tcp;
        }

        public bool Open()
        {
            if (printerInterface == Interface.Tcp)
            {
                Console.WriteLine("IP Address: {0} IP Endpoint: {1}", ipAddress, ipEndPoint);
                try
                {
                    if (networkStream != null)
                        networkStream.Close();

                    if (tcpClient != null)
                        tcpClient.Close();

                    tcpClient = new TcpClient();
                    Console.WriteLine("Connecting TCP.");
                    tcpClient.Connect(ipEndPoint);
                    tcpClient.ReceiveTimeout = 2000;
                    tcpClient.SendTimeout = 2000;
                    Console.WriteLine("Connected TCP.");

                    networkStream = tcpClient.GetStream();
                    Console.WriteLine("Get stream.");
                    networkStream.ReadTimeout = 2000;
                    networkStream.WriteTimeout = 2000;
                }
                catch
                {
                    Console.WriteLine("Unable to open TCP connection.");
                    return true;
                }
                return false;
            }
            else if (printerInterface == Interface.Serial)
            {
                // TODO
                return true;
            }
            else if (printerInterface == Interface.None)
            {
                Console.WriteLine("Interface has been not selected");
                return true;
            }
            return true;
        }


        public void Close()
        {
            if (printerInterface == Interface.Tcp)
            {
                try
                {
                    if (networkStream != null)
                    {
                        networkStream.Close();
                        networkStream = null;
                    }

                    if (tcpClient != null)
                    {
                        tcpClient.Close();
                        tcpClient = null;
                    }
                }
                catch (Exception e)
                {

                }
            }

        }

        public bool send(string strToSend)
        {

            byte[] arr = Encoding.ASCII.GetBytes(strToSend);
            if (printerInterface == Interface.Tcp)
            {
                networkStream.Write(arr, 0, arr.Length);
            }

            return false;
        }

        public bool sendCommand(byte[] arr)
        {
            if (printerInterface == Interface.Tcp)
            {
                networkStream.Write(arr, 0, arr.Length);
            }

            return false;
        }

        public bool sendCommand(byte[] arr, byte[] param)
        {
            byte[] newArr = arr;

            newArr = newArr.Concat(param).ToArray();
            if (printerInterface == Interface.Tcp)
            {
                networkStream.Write(newArr, 0, newArr.Length);
            }

            return false;
        }

        public bool print_page()
        {
            this.sendCommand(this.cut_setting, Enumerable.Repeat((byte)CutSetting.Full, 1).ToArray());
            this.sendCommand(this.page_feed);

            return false;
        }



    }
}
