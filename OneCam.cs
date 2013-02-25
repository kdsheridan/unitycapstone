using UnityEngine;
using System.Collections;
using System;

public class OneCam : MonoBehaviour {
    public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;
    public float x, y, z, deg;
    System.IO.StreamReader file;
	// Use this for initialization
	void Start () {
       // minitial = transform.position;
       // mrot = transform.localEulerAngles;
        action();
        //mainCamera.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
        action();
	}

    void readStdin()
    {
        //char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
        string chars = " ,.:\tabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()/?><";
        char[] delimiterChars = chars.ToCharArray();
        //char[] delimiterChars = { ' ', ',', '.', ':', '\t', 'a', 'b', 'c', 'd', 'e'};
        string line;
        //line = Console.ReadLine();
        file =  new System.IO.StreamReader(@"C:\Users\Kevin\Documents\New Unity Project\Assets\Data.txt");
        line = file.ReadLine();
        string[] words = line.Split(delimiterChars);
        x = Single.Parse(words[0]);
        y = Single.Parse(words[1]);
        z = Single.Parse(words[2]);
        deg = Single.Parse(words[3]);
    }

    void setCam(float a, float b, float c, float d)
    {
        transform.position = new Vector3(a, b, c);
        transform.localEulerAngles = new Vector3(0.0f, 90.0f - d, 0.0f);
    }
    void action()
    {
        readStdin();
        setCam(x, y, z, deg);
    }
}
