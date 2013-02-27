using UnityEngine;
using System.Collections;
using System;

public class OneCam2 : MonoBehaviour {
    public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;
    public float height, theta, distance, deg, initialDist, initalHeight;
    public int stop = 0;
    public new Vector3 minitial;
    
    System.IO.StreamReader file;
	// Use this for initialization
	void Start () {
     
        minitial = transform.position;
        initialDist = transform.position.z;
        initalHeight = transform.position.y;
       // mrot = transform.localEulerAngles;
        file = new System.IO.StreamReader(@"C:\Users\Kevin\Documents\New Unity Project\Assets\Data.txt");
        action();
        //mainCamera.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
	}
	
	// FixedUpdate is called 30 times a second
	void FixedUpdate () {
        if (stop == 0)
        {
            action();
            //transform.position = minitial;
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
        transform.position = new Vector3(transform.position.x, initalHeight+height, initialDist + distance);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, -90.0f+theta, transform.localEulerAngles.z);
        //transform.Rotate(0.0f, transform.localEulerAngles.y - b, 0.0f);
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
