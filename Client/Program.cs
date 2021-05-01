using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Connect to a server
            TcpClient client = new TcpClient("127.0.0.1", 200);

            //New thread for printing data from server
            Thread t = new Thread(new ParameterizedThreadStart(WriteToConsole));
            t.Start(client);

            NetworkStream stream = client.GetStream();

            while (true)
            {
                string thing = System.Console.ReadLine();

                //When we read a new line of text from console, we encode it to UTF-8,
                byte[] thingByte = Encoding.UTF8.GetBytes(thing);

                //and send all data to our server
                stream.Write(thingByte, 0, thingByte.Length);
            }
        }

        private static void WriteToConsole(object clientObject)
        {
            //Initialize client and stream variables.
            TcpClient client = (TcpClient)clientObject;
            NetworkStream stream = client.GetStream();

            while (true)
            {
                //This variable will contain text we got from client
                string dataString;

                //data is our buffer
                byte[] data = new byte[1024];

                //We are using MemoryStream for saving recieved bytes which is muc easier to use that arrays
                using (MemoryStream ms = new MemoryStream())
                {
                    //This int holds how many bytes we have read. It can be less than the data size
                    int numBytesRead;

                    do
                    {
                        numBytesRead = stream.Read(data, 0, data.Length);
                        ms.Write(data, 0, numBytesRead);//We write all bytes from 0 to numBytesRead and save them in memory stream

                    } while (stream.DataAvailable); //If we have more bytes that our buffer, thet this is set to true!

                    //When we are done with reading data, then we can get UTF-8 string from our memory stream
                    dataString = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\nServer message: " + dataString + "\n");
                Console.ResetColor();
            }
        }


    }
}
