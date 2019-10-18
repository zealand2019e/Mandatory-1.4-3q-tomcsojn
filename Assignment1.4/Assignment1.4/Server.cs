using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assignment1._4

{
    class Server
    {
        static List<Book> books = new List<Book>()
        {
            new Book("The Hobbit",". R. R. Tolkien",340,"2348573648576"),
            new Book("The Da Vinci Code","Dan Brown",200,"7656463756765"),
            new Book("The Hunger Games","Suzanne Collins",123,"5748395776566"),
            new Book("Romeo and Juliet","William Shaekspeare",55,"4444568965985")
        };

        public void Start()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener
                Int32 port = 4646;
                IPAddress localAddr = IPAddress.Loopback;

                int clientNumber = 0;

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    Task.Run(() => HandleStream(client, ref clientNumber));
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        public void HandleStream(TcpClient client, ref int clientNumber)
        {
            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;
            clientNumber++;

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            int i;

            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.UTF8.GetString(bytes, 0, i).Trim();
                Console.WriteLine("Received: {0} from client {1}", data, clientNumber);

                // Process the data sent by the client.

                string mess = "not valid command";
                string[] words = data.ToLower().Split(' ');
                if (words[0]=="getall")
                {
                    mess = JsonConvert.SerializeObject(books);
                }
                if (words[0] == "get")
                {
                    mess = JsonConvert.SerializeObject(books.Find(e => e.Isbn13 == words[1]));
                }
                if (words[0] == "save")
                {
                    string myjson = data.Split("{")[1].Split("}")[0];
                    myjson = "{" + myjson + "}";
                    //string myjson = "{ \"Title\":\"The Hobbit\",\"Author\":\". R. R. Tolkien\",\"Pagenumber\":340,\"Isbn13\":\"2348573648576\"}";
                books.Add(JsonConvert.DeserializeObject<Book>(myjson));
                    mess = "";
                }

               
                //encode message
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mess);

                Thread.Sleep(1000);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", mess);
            }

            // Shutdown and end connection
            client.Close();
            clientNumber--;
        }

    }
}
