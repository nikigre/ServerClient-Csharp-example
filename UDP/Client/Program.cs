using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpClient client = null;
            try
            {
                //Connect to a server
                client = new UdpClient("127.0.0.1", 200);
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem connecting to a remote server. Maybe it is offline?\nException: " + ex.Message);
                Environment.Exit(1);
            }

            //New thread for printing data from server
            Thread t = new Thread(new ParameterizedThreadStart(WriteToConsole));
            t.Start(client);

            while (true)
            {
                string thing = System.Console.ReadLine();

                //When we read a new line of text from console, we encode it to UTF-8,
                byte[] thingByte = Encoding.UTF8.GetBytes(thing);


                //and send all data to our server
                client.Send(thingByte, thingByte.Length);
            }
        }

        private static void WriteToConsole(object clientObject)
        {
            //Gets the client and the stream for the client
            UdpClient sender = (UdpClient)clientObject;

            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (true)
                {
                    //This variable will contain text we got from client
                    string dataString;

                    //data is our buffer
                    byte[] data = new byte[1024];

                    //We are using MemoryStream for saving recieved bytes which is muc easier to use that arrays
                    using (MemoryStream ms = new MemoryStream())
                    {

                        byte[] dataRead = sender.Receive(ref iPEndPoint);
                        ms.Write(dataRead, 0, dataRead.Length);//We write all bytes from 0 to numBytesRead and save them in memory stream

                        //When we are done with reading data, then we can get UTF-8 string from our memory stream
                        dataString = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
                    }

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("\nServer message: " + dataString + "\n");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection with client has been lost.\nException: {0}", ex.Message);
            }
        }
    }
}
