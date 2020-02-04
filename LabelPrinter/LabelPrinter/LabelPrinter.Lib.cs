using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.IO.Ports;
using System.Threading;

namespace LabelPrinter.Lib
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
    public enum Tables : byte
    {
        Standard = 0,
        Eastern_uropean = 1,
        Western_european = 2,
        Spare = 3
    }

    public enum Rotation : byte
    {
        Rotate = 0x31,
        Normal = 0x30,
    }

    public enum Bold : byte
    {
        On = 0x45,
        Off = 0x46,
    }

    public enum Italic : byte
    {
        On = 0x34,
        Off = 0x35,
    }
    public enum DoubleStrike : byte
    {
        On = 0x47,
        Off = 0x48,
    }

    public enum DoubleWidth : byte
    {
        On = 0x31,
        Off = 0x30,
    }

    public enum CompressedChar : byte
    {
        On = 0x0f,
        Off = 0x12,
    }

    public enum Underline : byte
    {
        On = 0x31,
        Off = 0x30,
    }

    public enum CharSize : byte
    {
        Auto = 0x30,
        Size4Point = 0x31,
        Size6Point = 0x32,
        Size9Point = 0x33,
        Size12Point = 0x34,
        Size18Point = 0x35,
        Size24Point = 0x36
    }

    public enum Font : byte
    {
        Brougham = 0x00,
        Lettergothicbold = 0x01,
        Brusselsbit = 0x02,
        Helsinkibit = 0x03,
        Sandiego = 0x04,
        Lettergothic = 0x09
    }

    public enum CharStyle : byte
    {
        Normal = 0x00,
        Outline = 0x01,
        Shadow = 0x02,
        Outlineshadow = 0x03
    }



    public class BrotherPrint
    {

        // System Commands & Settings
        public readonly byte[] command_mode = { 0x1b, 0x69, 0x61, 0x00 }; // ESC/P mode 
        public readonly byte[] raster_mode = { 0x1b, 0x69, 0x61, 0x01 };
        public readonly byte[] template_mode = { 0x1b, 0x69, 0x61, 0x33 };
        public readonly byte[] initialize = { 0x1b, 0x40 };
        public readonly byte[] page_feed = { 0x0c }; // print
        public readonly byte[] cut_setting = { 0x1b, 0x69, 0x43 }; // add one byte parameter "CutSetting" to array, 
        public readonly byte[] select_charset = { 0x1b, 0x52 };  // add one byte parameter "Charsets" to array, 
        public readonly byte[] select_char_code_table = { 0x1b, 0x74 };  // add one byte parameter "Tables" to array. The desired character code table. Choose from 'standard', 'eastern european', 'western european', and 'spare'
        // Format Commands
        public readonly byte[] rotated_printing = { 0x1b, 0x69, 0x4c };  // add one byte parameter "Rotation" to array. The desired printing orientation. 'rotate' enables rotated printing. 'normal' disables rotated printing.

        // Print Operations
        public readonly byte[] forward_feed = { 0x1b, 0x4a };  // add one byte parameter 0 <= n <= 255

        // Text Printing Commands
        public readonly byte[] bold = { 0x1b };  // add one byte parameter "Bold" to array.
        public readonly byte[] italic = { 0x1b };  // add one byte parameter "Italic" to array.
        public readonly byte[] double_strike = { 0x1b };  // add one byte parameter "DoubleStrike" to array.
        public readonly byte[] double_width = { 0x1b, 0x57 };  // add one byte parameter "DoubleWidth" to array.
        public readonly byte[] compressed_char = { 0x00 };  // add one byte parameter "CompressedChar" to array.
        public readonly byte[] underline = { 0x1b, 0x2d };  // add one byte parameter "Underline" to array.
        public readonly byte[] char_size = { 0x1b, 0x58 };  // add one byte parameter "CharSize" to array.
        public readonly byte[] select_font = { 0x1b, 0x6b };  // add one byte parameter "Fonts" to array.
        public readonly byte[] char_style = { 0x1b, 0x71 };  // add one byte parameter "CharStyle" to array.

        public readonly byte[] number_copies = { 0x5e, 0x43, 0x4e, 0x30, 0x30 };
        // Template Commands
        public readonly byte[] init_template = { 0x5E, 0x49, 0x49 };
        public readonly byte[] choose_template = { 0x5e, 0x54, 0x53, 0x30 };  // add two byte parameter template number to array, 
        public readonly byte[] print_template = { 0x5E, 0x46, 0x46 };
        public readonly byte[] select_object = { 0x5E, 0x4F, 0x4E };
        public readonly byte[] insert_into_object = { 0x5e, 0x44, 0x49 };
        public readonly byte[] direct_object_insertion = { 0x5E, 0x44, 0x49, 0x13, 0x00 };

        // Common requests
        public readonly byte[] version_request = { 0x5E, 0x56, 0x52 };
        public readonly byte[] status_request = { 0x5E, 0x53, 0x52 };


        private static TcpClient tcpClient = null;
        private static NetworkStream networkStream = null;
        private IPAddress ipAddress = null;
        private IPEndPoint ipEndPoint = null;

        private static SerialPort port = null;
        private string portName = null;
        private int baudRate = 9600;

        Interface printerInterface = Interface.None;

        public BrotherPrint(string ipAddressString, string ipPortString)
        {
            this.ipAddress = IPAddress.Parse(ipAddressString);
            this.ipEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(ipPortString));
            this.printerInterface = Interface.Tcp;
        }

        public BrotherPrint(string port, int baudRate)
        {
            this.portName = port;
            this.baudRate = baudRate;
            this.printerInterface = Interface.Serial;
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
                if (port != null)
                    port.Close();

                port = new SerialPort(this.portName, this.baudRate, Parity.None, 8, StopBits.One);
               
                try
                {
                    port.Open();
                    Console.WriteLine("Serial Port: {0} Baud Rate: {1}", this.portName, this.baudRate);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e);
                    return true;
                }

                return false;
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
                    Console.WriteLine("Exception: {0}", e);
                }
            }
            if (printerInterface == Interface.Serial)
            {
                port.Close();
                port.Dispose();
            }

        }

        public bool send(string strToSend)
        {

            byte[] arr = Encoding.ASCII.GetBytes(strToSend);
            if (printerInterface == Interface.Tcp)
            {
                networkStream.Write(arr, 0, arr.Length);
            }
            else if (printerInterface == Interface.Serial)
            {
                port.Write(arr, 0, arr.Length);
            }

            return false;
        }

        public bool sendCommand(byte[] arr)
        {
            if (printerInterface == Interface.Tcp)
            {
                networkStream.Write(arr, 0, arr.Length);
            }
            else if (printerInterface == Interface.Serial)
            {
                port.Write(arr, 0, arr.Length);
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
            else if (printerInterface == Interface.Serial)
            {
                port.Write(arr, 0, arr.Length);
            }

            return false;
        }

        public bool sendCommand(byte[] arr, byte param) // Command and One Byte to Array
        {
            this.sendCommand(arr, Enumerable.Repeat((byte)param, 1).ToArray());
            return false;
        }

        public bool sendCommand(byte[] arr, string param) // Command and String to Array
        {
            this.sendCommand(arr, Encoding.ASCII.GetBytes(param));
            return false;
        }

        public bool sendCommand(string param) // String to Array
        {
            this.sendCommand(Encoding.ASCII.GetBytes(param));
            return false;
        }

        public byte[] sendCommandWithRequest(byte[] arr, int bytesToRead)
        {
            if (printerInterface == Interface.Tcp)
            {
                // TODO ...

                //byte[] buf = new byte[bytesToRead];
                //try
                //{
                //    networkStream.Write(arr, 0, arr.Length);
                //    Thread.Sleep(50);
                //    var receivedBytesCnt = networkStream.Read(buf, 0, buf.Length);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine("Requesting: TCP exception {0}", e.ToString());
                //}

                Console.WriteLine("Requesting not supported over TCP.");
                return null;
            }
            else if (printerInterface == Interface.Serial)
            {
                byte[] buf = new byte[bytesToRead];
                try
                {
                    port.Write(arr, 0, arr.Length);
                    Thread.Sleep(100);
                    int numOfbytes = port.BytesToRead;

                    if (numOfbytes != bytesToRead)
                    {
                        Console.WriteLine("Requesting: Length of received data is mismatch: Received:{0} , To Read:{1}", numOfbytes, bytesToRead);
                        
                        return null;
                    }
                    else
                    {
                        port.Read(buf, 0, bytesToRead);

                        return buf;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Requesting: Serial exception {0}", e.ToString());

                    return null;
                }
            }

            return null;
        }

        public string statusRequest(int bytesToRead)
        {
            byte[] status = new byte[bytesToRead];

            status[8] = 0xff;
            status[9] = 0xff;

            status = this.sendCommandWithRequest(this.status_request, bytesToRead);

            if (status == null)
            {
                return null;
            }
            else if (status[8] != 0x00 || status[9] != 0x00)
            {
                return "ERR";
            }

            return "OK";
        }

        public string versionRequest(int bytesToRead)
        {
            byte[] version = new byte[bytesToRead];

            version = this.sendCommandWithRequest(this.version_request, bytesToRead);

            if (version == null)
            {
                return null;
            }

            return System.Text.Encoding.UTF8.GetString(version, 0, version.Length); ;
        }


        // Template Functions
        public bool chooseTemplate(byte templateId)
        {
            byte[] buf = new byte[2];
            buf[0] = (byte)(templateId / 10);
            buf[1] = (byte)(templateId % 10);
            this.sendCommand(this.choose_template, buf);
            
            return false;
        }
        public bool insertIntoObject(string strToInsert)
        {

            byte[] buf = new byte[strToInsert.Length + 2];
            buf[0] = (byte)(strToInsert.Length / 256);
            buf[1] = (byte)(strToInsert.Length % 256);
            Encoding.ASCII.GetBytes(strToInsert, 0, strToInsert.Length, buf, 2);
            this.sendCommand(this.insert_into_object, buf);

            return false;
        }


    }
}
