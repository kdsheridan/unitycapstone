using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class UnityUDP : MonoBehaviour
{

    public Camera CameraSingle = null;
    public float height, theta, distance, deg;
    public float[] initialDist, initialHeight, initialHorizontal;
    public int stop = 0;
    public int number = 1;
    public float Px, Py, Pz, Rx, Ry, Rz, Cx, Cy, Cz, Dx, Dy, Dz;
    public float[] Ax, Ay, Az;
    public GameObject[] ringset;
    public Material greenMat;
    public bool[] foundRing;
    public int[] ringDelay;
    

    System.IO.StreamReader file;
    public Camera[] Cameras = null;

    IPEndPoint unityIPEP;
    IPEndPoint kinectIPEP;
    UdpClient socket;
    bool connected = false;

    byte[] udpMessage;

    Thread receiveThread;

    Queue<string> messageQueue = new Queue<string>();

    // Use this for initialization
    void Start()
    {
        print("Start");

        //file = new System.IO.StreamReader(@"C:\Users\Kevin\Documents\New Unity Project\Assets\Data.txt");
        CameraSingle = Camera.main;
        //initialDist = CameraSingle.transform.position.z;
        //initalHeight = CameraSingle.transform.position.y;
        CreateCameras();
        receiveThread = new Thread(new ThreadStart(ReceiveUDP));
        receiveThread.IsBackground = true;
        receiveThread.Start();

    }

    // Update is called once per frame (30fps)
    void FixedUpdate()
    {
        if (stop == 0 || connected == true)
        {
            int i = 0;
            readStdin();
            //ReceiveUDP();
            foreach (Camera Camera in Cameras)
            {
                setCam(i);
                i++;
            }
        }
    }

    private void ConnectUDP()
    {
        print("Connect UDP");
        unityIPEP = new IPEndPoint(IPAddress.Any, 64582);
        kinectIPEP = new IPEndPoint(IPAddress.Any, 0);
        socket = new UdpClient(unityIPEP);

        print("Waiting for Initial Message");

        udpMessage = socket.Receive(ref kinectIPEP);
        print("Initial Message Received");
        connected = true;
    }


    private void ReceiveUDP()
    {
        
        if (!connected)
        {
            ConnectUDP();
        }

        while (true)
        {
            udpMessage = socket.Receive(ref kinectIPEP);


            string message;
            //line = Console.ReadLine();
            //line = file.ReadLine();
            message = System.Text.Encoding.ASCII.GetString(udpMessage);
            print("Receive UDP " + message);

            messageQueue.Enqueue(message);
        }

    }


    private void CreateCameras()
    {
        Cameras = new Camera[number];
        initialDist = new float[number];
        initialHeight = new float[number];
        initialHorizontal = new float[number];
        Ax = new float[8];
        Ay = new float[8];
        Az = new float[8];
        float x = 0, y = 0;
        foundRing = new bool[4] {false, false, false, false};

        ringDelay = new int[4]{0,0,0,0};

        Cameras[0] = CameraSingle;
        initialDist[0] = CameraSingle.transform.position.z;
        initialHeight[0] = CameraSingle.transform.position.y;
        initialHorizontal[0] = CameraSingle.transform.position.x;

        CameraSingle.rect = new Rect(x, 0.0f, 1.0f / number, 1.0f);
        for (int i = 1; i < number; i++)
        {

            Cameras[i] = Instantiate(CameraSingle, CameraSingle.transform.position, Quaternion.identity) as Camera;

            if (null == Cameras[i])
            {

                Debug.LogError("null == Cameras[i]");

                return;

            }
            x = x + 1.0f / number;
            y = y + 1.0f / number;
            Cameras[i].rect = new Rect(x, 0.0f, y, 1.0f);

            Cameras[i].gameObject.name = "0" + i + "Camera";

            Cameras[i].transform.parent = CameraSingle.transform.parent;
            initialDist[i] = CameraSingle.transform.position.z;
            initialHeight[i] = CameraSingle.transform.position.y;
            initialHorizontal[i] = CameraSingle.transform.position.x;

            Cameras[i].transform.position = new Vector3(CameraSingle.transform.position.x + (1.0f * i), CameraSingle.transform.position.y, CameraSingle.transform.position.z);
            //Cameras[i].transform.rotation = ;

        }
    }

    float tofloat(double input)
    {
        float result = (float)input;
        if (float.IsPositiveInfinity(result))
        {
            result = float.MaxValue;
        }
        else if (float.IsNegativeInfinity(result))
        {
            result = float.MinValue;
        }
        return result;
    }

    void readStdin()
    {
        string chars = " ,:\tabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()/?><";
        char[] delimiterChars = chars.ToCharArray();



        if (messageQueue.Count == 0)
        {
            //print("Empty");
            //stop = 1;
            return;
        }

        string line = messageQueue.Dequeue();


        string[] words = line.Split(delimiterChars);
        
        foreach (string word in words)
        {
            print(word);
        }
        double x, y, z;
        double.TryParse(words[0], out x);
        double.TryParse(words[1], out y);
        double.TryParse(words[2], out z);

        Dx = tofloat(x);
        Dy = tofloat(y);
        Dz = tofloat(z);

        // Shift averaging array back
        Array.Copy(Az, 1, Az, 0, 7);
        Array.Copy(Ay, 1, Ay, 0, 7);
        Array.Copy(Ax, 1, Ax, 0, 7);
        Az[7] = tofloat(z);
        Ay[7] = tofloat(y);
        Ax[7] = tofloat(x);

        bool filterenable = true;

        Cz = (filterenable) ? (Az.Average()) : (tofloat(z));
        Cy = (filterenable) ? (Ay.Average()) : (tofloat(y));
        Cx = (filterenable) ? (Ax.Average()) : (tofloat(x));


        Cy = tofloat(Cy + .25);

        Px = tofloat(Cx * 3.5);
        Py = tofloat(Cy * 10);
        //Pz = tofloat((1 / z) * (-13));
        Pz = tofloat(Cz);

        Rx = tofloat(Math.Atan(Cy / Cz) * (180 / Math.PI) * (3));
        Ry = tofloat(Math.Atan(Cx / Cz) * (180 / Math.PI) * (-1.5));
        // Rz = tofloat(Math.Atan(y / x) * (180 / Math.PI));






        ringset = new GameObject[3];

        // Game Math
        // Ring Set A
        if (Rx > 5 && Rx < 12 && Ry > -35 && Ry < -19 && foundRing[0] == false)
        {
            ringDelay[0]++;
        }
        else
        {
            ringDelay[0] = 0;
        }
        if (ringDelay[0] >= 90)
        {
            foundRing[0] = true;
            ringset = GameObject.FindGameObjectsWithTag("ringgroup" + "A");
            foreach (GameObject g in ringset)
            {
                g.renderer.materials[0].color = Color.green;
            }
        }


        // Ring Set B
        if (Rx > -20 && Rx < -6 && Ry > 25 && Ry < 35 && foundRing[1] == false)
        {
            ringDelay[1]++;
        }
        else
        {
            ringDelay[1] = 0;
        }
        if (ringDelay[1] >= 90)
        {
            foundRing[1] = true;
            ringset = GameObject.FindGameObjectsWithTag("ringgroup" + "B");
            foreach (GameObject g in ringset)
            {
                g.renderer.materials[0].color = Color.green;
            }
        }


        // Ring Set C
        if (Rx > -6 && Rx < 3 && Ry > 4 && Ry < 17 && foundRing[2] == false)
        {
            ringDelay[2]++;
        }
        else
        {
            ringDelay[2] = 0;
        }
        if (ringDelay[2] >= 90)
        {
            foundRing[2] = true;
            ringset = GameObject.FindGameObjectsWithTag("ringgroup" + "C");
            foreach (GameObject g in ringset)
            {
                g.renderer.materials[0].color = Color.green;
            }
        }


        // Ring Set D
        if (Rx > -24 && Rx < -13 && Ry > 30 && Ry < 35 && foundRing[3] == false)
        {
            ringDelay[3]++;
        }
        else
        {
            ringDelay[3] = 0;
        }
        if (ringDelay[3] >= 90)
        {
            foundRing[3] = true;
            ringset = GameObject.FindGameObjectsWithTag("ringgroup" + "D");
            foreach (GameObject g in ringset)
            {
                g.renderer.materials[0].color = Color.green;
            }
        }




    }

    void setCam(int i)
    {
        //Cameras[i].transform.position = new Vector3(Cameras[i].transform.position.x, initialHeight[0] + height, initialDist[0] - distance );
        //Cameras[i].transform.localEulerAngles = new Vector3(Cameras[i].transform.localEulerAngles.x, -90.0f + 180 - theta, Cameras[i].transform.localEulerAngles.z);
        Cameras[i].transform.position = new Vector3(initialHorizontal[0] + Px, initialHeight[0] + Py, initialDist[0]);
        Cameras[i].transform.localEulerAngles = new Vector3(Rx, Ry, 0);
        double preFOV = ((((100.0/Pz) - 25) * 0.7) + 25);
        Cameras[i].fieldOfView = tofloat((preFOV > 100) ? 100 : ((preFOV < 25) ? (25) : (preFOV)));







    }
}

