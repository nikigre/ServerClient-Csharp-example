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
            //Creates a new server object
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

        /// <summary>
        /// Wellcome message for every new client
        /// </summary>
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
                Console.WriteLine("There was a problem starting a web server. " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// This method starts listening for new clients
        /// </summary>
        private void StartListening()
        {
            //We TRY to start the server. If we fail, that means that the coosen port is ocupied with another process.
            try
            {

                serverTCP.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem starting a web server.\n" + ex.Message);
                Environment.Exit(1);
            }

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");

                    TcpClient client = serverTCP.AcceptTcpClient(); //Here we are waiting for a new connection

                    Console.WriteLine("Connected to IP: " + client.Client.RemoteEndPoint);

                    //Creates a new thread for a client and starts it
                    Thread t = new Thread(new ParameterizedThreadStart(HandleRequest));
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
        public void HandleRequest(object obj)
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

                        } while (stream.DataAvailable); //If we have more bytes that our buffer, then this is set to true!

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
