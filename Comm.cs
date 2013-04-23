using UnityEngine;
using System.Collections;

public class Comm : MonoBehaviour {
     
        IPAddress IP;
        IPEndPoint endPoint;
        int UDPport, TCPport, ClientAliveThreadID;
        TcpClient tcp;

        string initialUnityConditions;

        //TODO: Still need to gather initial data here

        // Semaphores
        bool ClientAliveContinue;

        // Communication Status
        bool connectionEstablished, initialDataReceived;

        public Comm()
        {
            IP = IPAddress.Parse("169.231.113.102");
            UDPport = 16053;
            TCPport = 5555;
            endPoint = new IPEndPoint(IP, UDPport); //UDP will use first, update before TCP initiates
            ClientAliveContinue = true;
            connectionEstablished = false;
            initialDataReceived = false;

            initialUnityConditions = "";
        }

        static void Main(string[] args)
        {
            Communication Client = new Communication();

            // Send Client Alive via the UDP Protocol to a determined port UDPport
            Thread UdpThread = new Thread(new ThreadStart(Client.UDPClientAlive));
            UdpThread.Start();
            Client.ClientAliveThreadID = UdpThread.ManagedThreadId;

            // Listen for a Sever Response over TCP
            Thread tcpListen = new Thread(new ThreadStart(Client.TCPListen));
            tcpListen.Start();
            tcpListen.Join();

            // Kill the Client Alive Thread Once TCP Communication is detected

            // First Message from Kinect: Establish Connection
            Client.TcpRecieveAndSendAck(Client.tcp);

            // Second Message from Kinect: Receive Initial Kinect Data
            Client.TcpRecieveAndSendAck(Client.tcp);



            while (true)
            {
                
            }

        }

        private void UDPClientAlive()
        {
            System.Net.Sockets.UdpClient sock = new System.Net.Sockets.UdpClient();
            byte[] data = Encoding.ASCII.GetBytes("<Unity> <msg> Client Alive </msg> <port> 5555 </port> </Unity>");

            while (ClientAliveContinue)
            {
                Console.WriteLine("Tx via UDP: Client Alive");
                sock.Send(data, data.Length, endPoint);
                System.Threading.Thread.Sleep(1000);

                // TODO: send the port that can be connected to by TCP
            }

            Console.WriteLine("Client Alive Thread Ended");

        }

        private void TCPListen()
        {
            TcpListener listener = new System.Net.Sockets.TcpListener(IPAddress.Any, TCPport);
            listener.Start();
            tcp = listener.AcceptTcpClient();
            ClientAliveContinue = false;
        }

        private void TCPSend(string message)
        {
            while (true)
            {
                NetworkStream clientStream = tcp.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = encoder.GetBytes(message);
                try
                {
                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush();

                }
                catch
                {
                    Console.WriteLine("SEND ERROR");
                    break;
                }
            }
        }

        private void TcpRecieveAndSendAck(TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();
                //if (clientStream.DataAvailable)
   
                    Byte[] received = new Byte[512];
                    int nBytesReceived = clientStream.Read(received, 0, received.Length);
                    String dataReceived = System.Text.Encoding.ASCII.GetString(received);
                    
                    ASCIIEncoding encoder = new ASCIIEncoding();
              //      Console.WriteLine("Server Response: " + dataReceived);

                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(dataReceived);                 
                    XmlNodeList xnList = xml.SelectNodes("/kinect");

                    string header = "";
                    string data = "";

                    foreach (XmlNode xn in xnList)
                    {
                       header = xn["header"].InnerText;
                       data = xn["data"].InnerText;
                    }


                    Console.WriteLine("Header: " + header);

                    byte[] buffer;
                    string msg;

                    switch (header)
                    {
                        case "EC":
                            if (!connectionEstablished)
                            {
                                msg = "ACK";
                                buffer = encoder.GetBytes(msg);
                                clientStream.Write(buffer, 0, buffer.Length);
                                clientStream.Flush();
                                connectionEstablished = true;

                                Console.WriteLine("Connection Established: SENT ACK");
                                Console.WriteLine("Data Received: " + initialUnityConditions); //should be nothing
                            }
                            break;

                        case ("IUC"):   // Initial Unity Conditions are received                                   

                            if (!initialDataReceived)
                            {

                                // TODO: Receive Initial Unity Data here

                                msg = "ACK";
                                buffer = encoder.GetBytes(msg);
                                clientStream.Write(buffer, 0, buffer.Length);
                                clientStream.Flush();
                                initialDataReceived = true;

                                //This is where unity conditions will be received and parsed
                                initialUnityConditions = data;

                                Console.WriteLine("Initial Data Received: SENT ACK");
                                Console.WriteLine("Data Received: " + initialUnityConditions);
                            }
                            break;



                    }

        }

        private void TCPReceive(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            while (true)
            {
                //if (clientStream.DataAvailable)
                if(true)
                {
                    Byte[] received = new Byte[512];

                    Console.WriteLine("TCPReceive: PreRead");
                    int nBytesReceived = clientStream.Read(received, 0, received.Length);

                    Console.WriteLine("TCPReceive: PostRead");
                    String dataReceived = System.Text.Encoding.ASCII.GetString(received);
                    List<byte> temp = new List<byte>();
                    for (int i = 0; i < (int)tcpClient.ReceiveBufferSize; i++)
                    {
                        if ((int)received[i] == 0)
                        {
                            break;
                        }
                        temp.Add(received[i]);
                    }
                    string returndata = Encoding.UTF8.GetString(temp.ToArray());
                    temp.Clear();

                    Console.WriteLine("Server Response: " + returndata);

                }
                else
                {
                    Console.WriteLine("TCPReceive: Data Stream Not Available");
                }

            }
        }

        //private string EditString(string original)
        //{

        //    List<byte> temp = new List<byte>();
        //    for (int i = 0; i < TCPReceiveBufferSize; i++)
        //    {
        //        if ((int)original[i] == 0)
        //        {
        //            break;
        //        }
        //        temp.Add(original[i]);
        //    }
        //    string returndata = Encoding.UTF8.GetString(temp.ToArray());
        //    temp.Clear();

        //    return returndata;
        //}
 
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
