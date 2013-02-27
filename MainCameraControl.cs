using UnityEngine;
using System.Collections;
using System;

public class OneCam3 : MonoBehaviour {
    public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;
    public float height, theta, distance, deg, initialDist, initalHeight;
    public int stop = 0;
    public new Vector3 minitial;
    public Camera main;
    System.IO.StreamReader file;
	// Use this for initialization
	void Start () {
        main = Camera.main;
        minitial = main.transform.position;
        initialDist = main.transform.position.z;
        initalHeight = main.transform.position.y;
       // mrot = main.transform.localEulerAngles;
        file = new System.IO.StreamReader(@"C:\Users\Kevin\Documents\New Unity Project\Assets\Data.txt");
        action();
        //mainCamera.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
	}
	
	// FixedUpdate is called 30 times a second
	void FixedUpdate () {
        if (stop == 0)
        {
            action();
            //main.transform.position = minitial;
        }
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
        foreach (string word in words)
        {
            print(word);
        }
        double x, y, z;
        double.TryParse(words[0], out x);
        double.TryParse(words[1], out y);
        double.TryParse(words[2], out z);
        //double x = Double.Parse(words[0]);
        //double y = Double.Parse(words[1]);
        //double z = Double.Parse(words[2]);
        height = tofloat(x);
        theta = tofloat(y);
        distance = tofloat(z);
        //deg = Single.Parse(words[3]);
    }

    void setCam()
    {
        main.transform.position = new Vector3(main.transform.position.x, initalHeight+height, initialDist + distance);
        main.transform.localEulerAngles = new Vector3(main.transform.localEulerAngles.x, -90.0f+theta, main.transform.localEulerAngles.z);
        //main.transform.Rotate(0.0f, main.transform.localEulerAngles.y - b, 0.0f);
    }
    void action()
    {
        readStdin();
        setCam();
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
}
