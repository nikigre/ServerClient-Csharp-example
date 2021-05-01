using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create new server object
            Server server = new Server(200);

            server.RunServer(); //We run our server
        }
    }

    class Server
    {
        /// <summary>
        /// TcpListener that is waiting for clients to connect
        /// </summary>
        TcpListener serverTCP = null;

        byte[] welcomeMessage = Encoding.UTF8.GetBytes("Wellcome client! :)");

        /// <summary>
        /// Creates a new instance of server
        /// </summary>
        /// <param name="port">On which port you want to start server</param>
        public Server(int port)
        {
            //Initialize variable
            serverTCP = new TcpListener(IPAddress.Any, port);
        }

        /// <summary>
        /// Method runs a server
        /// </summary>
        public void RunServer()
        {
            try
            {
                //Starts new thread that listens for clients
                Thread thread = new Thread(new ThreadStart(StartListening));
                thread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem starting a web server. Is server on port " + this.serverTCP.ToString() + " already running?\nError: " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// This method starts listening for new clients
        /// </summary>
        private void StartListening()
        {
            serverTCP.Start();

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");

                    TcpClient client = serverTCP.AcceptTcpClient(); //Here we are waiting for a new connection

                    Console.WriteLine("Connected to IP: " + client.Client.RemoteEndPoint);

                    //Creates new thread for a client and starts it
                    Thread t = new Thread(new ParameterizedThreadStart(HandleRequestTCP));
                    t.Start(client);
                }
            }
            catch (SocketException e) //If anything goes wrong, then we print an error to the terminal and start listening again
            {
                Console.WriteLine("An error happened.\nSocketException: {0}", e.Message);
                StartListening();
            }
        }


        /// <summary>
        /// Method handles client's request
        /// </summary>
        /// <param name="obj">TcpClient in an object form</param>
        public void HandleRequestTCP(object obj)
        {
            //Gets the client and the stream for the client
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();

            //We send a welcome message to a client
            stream.Write(welcomeMessage, 0, welcomeMessage.Length);

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
                    System.Console.WriteLine("\nClient " + client.Client.RemoteEndPoint + " message: " + dataString + "\n");
                    Console.ResetColor();
                    //Here, we encode received text back to UTF-8 bytes
                    byte[] content = Encoding.UTF8.GetBytes(dataString);

                    //And here, we send the same data back.
                    stream.Write(content, 0, content.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection with client has been lost.\nException: {0}", ex.Message);
            }
        }
    }
}
