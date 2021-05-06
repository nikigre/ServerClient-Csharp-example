using System;
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
        UdpClient serverUDP = null;

        byte[] welcomeMessage = Encoding.UTF8.GetBytes("Wellcome client! :)");

        /// <summary>
        /// Creates a new instance of server
        /// </summary>
        /// <param name="port">On which port you want to start server</param>
        public Server(int port)
        {
            //Initialize variable
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
            serverUDP = new UdpClient(ipep);
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
            try
            {
                while (true)
                {
                    //This variable will hold info about our client that sent and UDP packet
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                    Console.WriteLine("Waiting for a connection...");

                    //Here we are waiting for a client to send us something. 
                    //Notice, that we used ref sender. sender variable parameters will be changed depending on the new client
                    byte[] dataFromClient = serverUDP.Receive(ref sender); //Here we are waiting for a new connection

                    Console.WriteLine("Message from UDP IP: " + sender);

                    //We convert receieved bytes to string
                    string dataString = Encoding.UTF8.GetString(dataFromClient);

                    //Here, we encode received text back to UTF-8 bytes
                    byte[] content = Encoding.UTF8.GetBytes(dataString);

                    //And here, we send the same data back.
                    serverUDP.Send(content, content.Length, sender);
                }
            }
            catch (SocketException e) //If anything goes wrong, then we print an error to the terminal and start listening again
            {
                Console.WriteLine("An error happened.\nSocketException: {0}", e.Message);
                StartListening();
            }
        }
    }
}
