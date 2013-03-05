using UnityEngine;
using System.Collections;
using System;

public class CameraAll : MonoBehaviour {

    public Camera CameraSingle = null;
    public float height, theta, distance, deg;
    public float [] initialDist, initialHeight;
    public int stop = 0;
    public int Number = 3;
    System.IO.StreamReader file;
    public Camera[] Cameras = null;
	// Use this for initialization
	void Start () {
        file = new System.IO.StreamReader(@"C:\Users\Kevin\Documents\New Unity Project\Assets\Data.txt");
        CameraSingle = Camera.main;
        //initialDist = CameraSingle.transform.position.z;
        //initalHeight = CameraSingle.transform.position.y;
        CreateCameras();
	}
	
	// Update is called once per frame
    void Update()
    {
        if (stop == 0)
        {
            int i=0;
            readStdin();
            foreach (Camera Camera in Cameras)
            {
                setCam(i);
                i++;
            }
        }
    }

	private void CreateCameras()
   
    {
        Cameras = new Camera[Number];
        initialDist = new float[Number];
        initialHeight = new float[Number];
        float x = 0,y=0;
        Cameras[0] = CameraSingle;
        initialDist[0] = CameraSingle.transform.position.z;
        initialHeight[0] = CameraSingle.transform.position.y;
        CameraSingle.rect = new Rect(x, 0.0f, 1.0f/Number, 1.0f);
        for (int i = 1; i < Number; i++)
        {

            Cameras[i] = Instantiate(CameraSingle, CameraSingle.transform.position, Quaternion.identity) as Camera;

            if (null == Cameras[i])
            {

                Debug.LogError("null == Cameras[i]");

                return;

            }
            x = x + 1.0f / Number;
            y = y + 1.0f / Number;
            Cameras[i].rect = new Rect(x, 0.0f, y, 1.0f);

            Cameras[i].gameObject.name = "0" + i + "Camera";

            Cameras[i].transform.parent = CameraSingle.transform.parent;
            initialDist[i] = CameraSingle.transform.position.z;
            initialHeight[i] = CameraSingle.transform.position.y;
            Cameras[i].transform.position = new Vector3(CameraSingle.transform.position.x + (1.0f*i), CameraSingle.transform.position.y, CameraSingle.transform.position.z);
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

        string line;
        //line = Console.ReadLine();
        line = file.ReadLine();
        if (String.IsNullOrEmpty(line))
        {
            // print("asdfasdf");
            stop = 1;
            return;
        }
        string[] words = line.Split(delimiterChars);
        //foreach (string word in words)
        //{
        //    print(word);
        //}
        double x, y, z;
        double.TryParse(words[0], out x);
        double.TryParse(words[1], out y);
        double.TryParse(words[2], out z);
       
        height = tofloat(x);
        theta = tofloat(y);
        distance = tofloat(z);
        
    }

    void setCam(int i)
    {
        Cameras[i].transform.position = new Vector3(Cameras[i].transform.position.x, initialHeight[0] + height, initialDist[0] + distance);
        Cameras[i].transform.localEulerAngles = new Vector3(Cameras[i].transform.localEulerAngles.x, -90.0f + theta, Cameras[i].transform.localEulerAngles.z);
    }
}
