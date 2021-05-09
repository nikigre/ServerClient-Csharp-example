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
            TcpClient client = null;

            try
            {
                //Connect to a server
                client = new TcpClient("127.0.0.1", 200);
            }
            catch (Exception ex) //If we fail, that means that port 200 is ocupied or server is not avalible
            {
                Console.WriteLine("There was a problem connecting to a remote server. Maybe it is offline?\nException: " + ex.Message);
                Environment.Exit(1);
            }

            //New thread for printing data from server
            Thread t = new Thread(new ParameterizedThreadStart(WriteToConsole));
            t.Start(client);

            //stream will be the variable for sending data to our server
            NetworkStream stream = client.GetStream();

            while (true)
            {
                //When user presses Enter, thig will hold entered text
                string thing = System.Console.ReadLine();

                //Converts entered text to bytes
                byte[] thingByte = Encoding.UTF8.GetBytes(thing);

                //We check, if we have an active connection to our server.
                if (client.Connected)
                    //Here we send all bytes to our server.
                    stream.Write(thingByte, 0, thingByte.Length);
            }
        }

        private static void WriteToConsole(object clientObject)
        {
            //Initialize client and stream variables.
            TcpClient client = (TcpClient)clientObject;
            NetworkStream stream = client.GetStream();

            try
            {
                while (true)
                {
                    //This variable will contain text we got from server
                    string dataString;

                    //data is our buffer. We could adjust the buffer size acording to how many data we would want to send/receive. Bur 1024 is okay for most communications
                    byte[] data = new byte[1024];

                    //We are using MemoryStream for saving recieved bytes which is much easier to use that arrays.
                    //But we could use List<byte> for example. But MemoryStream is designed for dealing with buffers                    
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //This int holds how many bytes we have read. It can be less than the data size
                        int numBytesRead;

                        //We use do-while loop because we will read et lest once!
                        do
                        {
                            //Read() returns how many bytes we have read. Data is saved into array data
                            numBytesRead = stream.Read(data, 0, data.Length);

                            //We write all bytes from 0 to numBytesRead and save them into memory stream
                            ms.Write(data, 0, numBytesRead);

                        } while (stream.DataAvailable); //If we have more bytes that our buffer, thet this is set to true!

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
                Console.WriteLine("There was a problem communicating with server. Connection is dropped.\nException: " + ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
