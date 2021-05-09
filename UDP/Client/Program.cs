using System;
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
            UdpClient server = null;

            try
            {
                //Connect to a server
                server = new UdpClient("127.0.0.1", 200);
            }
            catch (Exception ex) //If we fail, that usualy means that port is ocupied by another process
            {
                Console.WriteLine("There was a problem connecting to a remote server.\nException: " + ex.Message);
                Environment.Exit(1);
            }

            //New thread for printing data from server
            Thread t = new Thread(new ParameterizedThreadStart(WriteToConsole));
            t.Start(server);

            while (true)
            {
                //When user presses Enter, thig will hold entered text
                string thing = System.Console.ReadLine();

                //Converts entered text to bytes
                byte[] thingByte = Encoding.UTF8.GetBytes(thing);

                //Here we just send all bytes to our server
                server.Send(thingByte, thingByte.Length);
            }
        }

        private static void WriteToConsole(object clientObject)
        {
            //Gets the server object
            UdpClient sender = (UdpClient)clientObject;

            //This wariable will hold info about who sent a UDP packet.
            //We can initialize it like this: sender = new IPEndPoint(IPAddress.Any, 0) or not. 
            IPEndPoint iPEndPoint = null;

            try
            {
                while (true)
                {
                    //Here we wait for a UDP packet
                    byte[] dataRead = sender.Receive(ref iPEndPoint);

                    //When we receive it, we can get string from byte array
                    string dataString = Encoding.UTF8.GetString(dataRead, 0, dataRead.Length);

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
