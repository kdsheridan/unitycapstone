using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Xml;
using System.IO;


public class Comm  {
     
        IPAddress IP;
        IPEndPoint endPoint;
        int UDPport, TCPport, Dataport, ClientAliveThreadID;
        TcpClient tcp;

        string initialUnityConditions, screenSize, aspectRatio;

        //TODO: Still need to gather initial data here

        // Semaphores
        bool ClientAliveContinue;

        // Communication Status
        bool connectionEstablished, initialDataReceived;

        //udp variables
        IPEndPoint unityIPEP;
        IPEndPoint kinectIPEP;
        UdpClient socket;
        bool connected = false;

        byte[] udpMessage;

        Thread receiveThread;

        Queue<string> messageQueue = new Queue<string>();



        public Comm()
        {
            IP = IPAddress.Parse("169.231.113.230");
            UDPport = 16055;
            TCPport = GetRandomUnusedPort();
            Dataport = 64580;
            endPoint = new IPEndPoint(IP, UDPport); //UDP will use first, update before TCP initiates
            ClientAliveContinue = true;
            connectionEstablished = false;
            initialDataReceived = false;

            initialUnityConditions = "";
        }
        public void run()
        {
            UnityEngine.Debug.Log("Run");
            //Comm Client = new Comm();
            UnityEngine.Debug.Log("object created");
            // Send Client Alive via the UDP Protocol to a determined port UDPport
            Thread UdpThread = new Thread(new ThreadStart(UDPClientAlive));
            UdpThread.Start();
            //Client.
            ClientAliveThreadID = UdpThread.ManagedThreadId;

            // Listen for a Sever Response over TCP
            Thread tcpListen = new Thread(new ThreadStart(TCPListen));
            tcpListen.Start();
            tcpListen.Join();

            // Kill the Client Alive Thread Once TCP Communication is detected

            // First Message from Kinect: Establish Connection
            //Client.
            TcpRecieveAndSendAck(tcp);

            // Second Message from Kinect: Receive Initial Kinect Data
            //Client.
            TcpRecieveAndSendAck(tcp);

            //Third Message from Kinect: Stop data Stream
            TcpRecieveAndSendAck(tcp);

        }
        public string getData()
        {
            if (messageQueue.Count == 0)
            {
                //UnityEngine.Debug.Log("Empty");
                //stop = 1;
                return "";
            }
            //UnityEngine.Debug.Log("sending data");
            string line = messageQueue.Dequeue();
            return line;
        }
        //static void Main(string[] args)
        //{
        //    Comm Client = new Comm();

        //    // Send Client Alive via the UDP Protocol to a determined port UDPport
        //    Thread UdpThread = new Thread(new ThreadStart(Client.UDPClientAlive));
        //    UdpThread.Start();
        //    Client.ClientAliveThreadID = UdpThread.ManagedThreadId;

        //    // Listen for a Sever Response over TCP
        //    Thread tcpListen = new Thread(new ThreadStart(Client.TCPListen));
        //    tcpListen.Start();
        //    tcpListen.Join();

        //    // Kill the Client Alive Thread Once TCP Communication is detected

        //    // First Message from Kinect: Establish Connection
        //    Client.TcpRecieveAndSendAck(Client.tcp);

        //    // Second Message from Kinect: Receive Initial Kinect Data
        //    Client.TcpRecieveAndSendAck(Client.tcp);



        //    while (true)
        //    {
                
        //    }

        //}

        private void UDPClientAlive()
        {
            System.Net.Sockets.UdpClient sock = new System.Net.Sockets.UdpClient();
            //TCPport = GetRandomUnusedPort();
            byte[] data = Encoding.ASCII.GetBytes("<unity> <header>Client Alive</header> <port>"+TCPport.ToString()+"</port> </unity>");

            while (ClientAliveContinue)
            {
                UnityEngine.Debug.Log("Tx via UDP: Client Alive");
                sock.Send(data, data.Length, endPoint);
                System.Threading.Thread.Sleep(1000);

                // TODO: send the port that can be connected to by TCP
            }

            UnityEngine.Debug.Log("Client Alive Thread Ended");

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
                    UnityEngine.Debug.Log("SEND ERROR");
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
              //      UnityEngine.Debug.Log("Server Response: " + dataReceived);

                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(dataReceived);                 
                    XmlNodeList xnList = xml.SelectNodes("/kinect");

                    string header = "";
                    string data = "";
					string screen = "";
                    string aspect = "";

                    foreach (XmlNode xn in xnList)
                    {
                       header = xn["header"].InnerText;
                       data = xn["data"].InnerText;
					   if (xn.SelectSingleNode("//screen")!=null)
							screen = xn["screen"].InnerText;
			
					   if(xn.SelectSingleNode("//aspect")!=null)
							aspect = xn["aspect"].InnerText;
                    }


                    UnityEngine.Debug.Log("Header: " + header);

                    
                    string msg;
                    byte[] buffer;
                    switch (header)
                    {
                        case ("EC"):
                            if (!connectionEstablished)
                            {
                                //msg = "ACK";
                                //buffer = encoder.GetBytes(msg);
                                buffer = Encoding.ASCII.GetBytes("<unity> <header>ACK</header> <data>0</data> </unity>");
                                clientStream.Write(buffer, 0, buffer.Length);
                                clientStream.Flush();
                                connectionEstablished = true;

                                UnityEngine.Debug.Log("Connection Established: SENT ACK");
                                UnityEngine.Debug.Log("Data Received: " + initialUnityConditions); //should be nothing
                            }
                            break;

                        case ("IUC"):   // Initial Unity Conditions are received                                   

                            if (!initialDataReceived)
                            {

                                // TODO: Receive Initial Unity Data here

                               // msg = "ACK";
                                //buffer = encoder.GetBytes(msg);
                                Dataport = GetRandomUnusedPort();
                                buffer = Encoding.ASCII.GetBytes("<unity> <header>ACK</header> <data>"+Dataport+"</data> </unity>");
                                clientStream.Write(buffer, 0, buffer.Length);
                                clientStream.Flush();
                                initialDataReceived = true;

                                //This is where unity conditions will be received and parsed
                                initialUnityConditions = data;
								screenSize = screen;
								aspectRatio = aspect;

                                UnityEngine.Debug.Log("Initial Data Received: SENT ACK");
                                UnityEngine.Debug.Log("Data Received: " + initialUnityConditions + "\t" + screen + "\t" +aspect);

                                //start the data stream
                                receiveThread = new Thread(new ThreadStart(ReceiveUDP));
                                receiveThread.IsBackground = true;
                                receiveThread.Start();
                            }
                            break;
                        case ("END"):
                                
                              buffer = Encoding.ASCII.GetBytes("<unity> <header>ACK</header> <data>0</data> </unity>");
                              clientStream.Write(buffer, 0, buffer.Length);
                              clientStream.Flush();
                              initialUnityConditions = data;
                              UnityEngine.Debug.Log("Ending Data Connection: SENT ACK");
                              UnityEngine.Debug.Log("Data Received: " + initialUnityConditions); //should be nothing

                              endConnection();
                              break;

                        
                        case ("OVR"):
                              buffer = Encoding.ASCII.GetBytes("<unity> <header>ACK</header> <data>0</data> </unity>");
                              clientStream.Write(buffer, 0, buffer.Length);
                              clientStream.Flush();
                              //tcp.shutdown();
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

                    UnityEngine.Debug.Log("TCPReceive: PreRead");
                    int nBytesReceived = clientStream.Read(received, 0, received.Length);

                    UnityEngine.Debug.Log("TCPReceive: PostRead");
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

                    UnityEngine.Debug.Log("Server Response: " + returndata);

                }
                else
                {
                    UnityEngine.Debug.Log("TCPReceive: Data Stream Not Available");
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
        private void ConnectUDP()
        {
            UnityEngine.Debug.Log("Connect UDP");
            unityIPEP = new IPEndPoint(IPAddress.Any, Dataport);
            kinectIPEP = new IPEndPoint(IPAddress.Any, 0);
            socket = new UdpClient(unityIPEP);

            UnityEngine.Debug.Log("Waiting for Initial Message");

            udpMessage = socket.Receive(ref kinectIPEP);
            UnityEngine.Debug.Log("Initial Message Received");
            connected = true;
        }


        private void ReceiveUDP()
        {
            if (!connected)
            {
                ConnectUDP();
            }
            socket.Client.ReceiveTimeout = 3000;
            while (connected)
            {
                try{
				udpMessage = socket.Receive(ref kinectIPEP);
			}
			catch(Exception e)
			{}
                //line = Console.ReadLine();
                //line = file.ReadLine();
                string message = System.Text.Encoding.ASCII.GetString(udpMessage);
                UnityEngine.Debug.Log("Receive UDP " + message);
                messageQueue.Enqueue(message);
            }
            UnityEngine.Debug.Log("Connection Ended");
        }

        public bool getConnected()
        {
            return connected;
        }
	
		public string getInitial()
		{
			return initialUnityConditions;
		}
	
		public string getScreen()
	    {
			return screenSize;
		}
	
		public string getAspect()
		{
			return aspectRatio;
		}
	
	
		public bool initialReceive()
		{	
			return initialDataReceived;
		}

        private void endConnection()
        {
            
            connected = false;
        }

        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
}
