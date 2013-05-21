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
    public float theta, distance, deg;
	public float Cx, Cy, Cz;
	public float[] Ax, Ay, Az;
    public float[] initialDist, initialHeight, initialHorizontal;
    public int stop = 0;
    public int number = 1;
    public GameObject[] ringset;
    public Material greenMat;
    public bool[] foundRing;
    public int[] ringDelay;
	public int filterlevels = 2;
    System.IO.StreamReader file;
    public Camera[] Cameras = null;
    public int init = 0;
    bool connected = false;
	public float Sw, Sh;
	public double ARw, ARh;
	public float screenangle;
	public double screen;
	public float LrotatedX, LrotatedY, RrotatedX, RrotatedY;

	public float LtranslatedX, LtranslatedY, RtranslatedX, RtranslatedY;
	public double initX, initY, initZ;
	public float finalX, finalY, finalZ;

	
	public Vector3 bottomLeftCorner, bottomRightCorner, topLeftCorner, topRightCorner, trackerPosition;
	
	public Matrix4x4 genProjection ;

	public Comm comm;
	
	private void startComm()
    {
        comm.run();
    }
	
    void Start()
    {
        print("Start");
        //file = new System.IO.StreamReader(@"C:\Users\Kevin\Documents\New Unity Project\Assets\Data.txt");
        CameraSingle = Camera.main;
        //initialDist = CameraSingle.transform.position.z;
        //initalHeight = CameraSingle.transform.position.y;
        //receiveThread = new Thread(new ThreadStart(ReceiveUDP));
        //receiveThread.IsBackground = true;
        //receiveThread.Start();
		
		
		Ax = new float[filterlevels];
		Ay = new float[filterlevels];
        Az = new float[filterlevels];
		
		comm = new Comm();
        Thread commStart = new Thread(new ThreadStart(startComm));
        commStart.IsBackground = true;
        commStart.Start();
	
		
    }
	
	
    // FixedUpdate is called once per frame (30fps)
    void FixedUpdate()
    {
		if (comm.initialReceive()){

            if (init == 0)
            {
                getInitialConditions();
                init = 1;
            }
            else if (comm.getRecalibrate())
            {
                getInitialConditions();
                comm.setRecalibrate();
            }
		
		}
        if (stop == 0 || comm.getConnected() == true)
        {
            int i = 0;
            getsetFrame(comm);

        }
        else
        {
            print("Connected = "+comm.getConnected());
        }
    }
	
	
	void getsetFrame(object param)
    {
        string chars = " ,:\tabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()/?><";
        char[] delimiterChars = chars.ToCharArray();
        //comm = (Comm)param;
        string line = comm.getData();
		if (line == "")
        {
            //print("Empty");
            //stop = 1;
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

		y -= 0.1;

        // Shift averaging array back
        Array.Copy(Az, 1, Az, 0, filterlevels-1);
        Array.Copy(Ay, 1, Ay, 0, filterlevels-1);
        Array.Copy(Ax, 1, Ax, 0, filterlevels-1);
        Az[filterlevels-1] = tofloat(z);
        Ay[filterlevels-1] = tofloat(y);
        Ax[filterlevels-1] = tofloat(x);
		
		
		
        bool filterenable = true;
		
        Cz = tofloat((filterenable) ? (Az.Average()) : (z));
        Cy = tofloat((filterenable) ? (Ay.Average()) : (y));
        Cx = tofloat((filterenable) ? (Ax.Average()) : (x));
		
		//float Sw = tofloat(0.324);
		//float Sh = tofloat(0.207);

		
		finalX = tofloat(Cx-initX);
		finalY = tofloat(Cy-initY);
		finalZ = tofloat(Cz-initZ);
		
		trackerPosition = new Vector3(finalX, finalY, finalZ);
		
		bottomLeftCorner = new Vector3(LtranslatedX, tofloat(-Sh/2.0), LtranslatedY);
		bottomRightCorner = new Vector3(RtranslatedX, tofloat(-Sh/2.0), RtranslatedY);
		topLeftCorner = new Vector3(LtranslatedX, tofloat(Sh/2.0), LtranslatedY);
		topRightCorner = new Vector3(RtranslatedX, tofloat(Sh/2.0), RtranslatedY);

		//topRightCorner = new Vector3(tofloat(-Sw/2.0), tofloat(-Sh/2.0), tofloat(0.0));
		//topLeftCorner = new Vector3(tofloat(Sw/2.0), tofloat(-Sh/2.0), tofloat(0.0));
		//bottomRightCorner = new Vector3(tofloat(-Sw/2.0), tofloat(Sh/2.0), tofloat(0.0));
		//bottomLeftCorner = new Vector3(tofloat(Sw/2.0), tofloat(Sh/2.0), tofloat(0.0));
	
	    //calculate projection
	
	     genProjection = GeneralizedPerspectiveProjection(bottomLeftCorner, bottomRightCorner, 
			topLeftCorner, trackerPosition, 
			CameraSingle.nearClipPlane, CameraSingle.farClipPlane);
	
	    CameraSingle.projectionMatrix = genProjection; 
		
		
    }

	
	public static Matrix4x4 GeneralizedPerspectiveProjection(Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pe, float near, float far)

    {

        Vector3 va, vb, vc;

        Vector3 vr, vu, vn;

        

        float left, right, bottom, top, eyedistance;

        

        Matrix4x4 transformMatrix;

        Matrix4x4 projectionM;

        Matrix4x4 eyeTranslateM;

        Matrix4x4 finalProjection;

        

        ///Calculate the orthonormal for the screen (the screen coordinate system

        vr = pb - pa;

        vr.Normalize();

        vu = pc - pa;

        vu.Normalize();

        vn = Vector3.Cross(vr, vu);

        vn.Normalize();

        

        //Calculate the vector from eye (pe) to screen corners (pa, pb, pc)

        va = pa-pe;

        vb = pb-pe;

        vc = pc-pe;

        

        //Get the distance;; from the eye to the screen plane

        eyedistance = -(Vector3.Dot(va, vn));

        

        //Get the varaibles for the off center projection

        left = (Vector3.Dot(vr, va)*near)/eyedistance;

        right  = (Vector3.Dot(vr, vb)*near)/eyedistance;

        bottom  = (Vector3.Dot(vu, va)*near)/eyedistance;

        top = (Vector3.Dot(vu, vc)*near)/eyedistance;

        

        //Get this projection

        projectionM = PerspectiveOffCenter(left, right, bottom, top, near, far);

        

        //Fill in the transform matrix

        transformMatrix = new Matrix4x4();

        transformMatrix[0, 0] = vr.x;

        transformMatrix[0, 1] = vr.y;

        transformMatrix[0, 2] = vr.z;

        transformMatrix[0, 3] = 0;

        transformMatrix[1, 0] = vu.x;

        transformMatrix[1, 1] = vu.y;

        transformMatrix[1, 2] = vu.z;

        transformMatrix[1, 3] = 0;

        transformMatrix[2, 0] = vn.x;

        transformMatrix[2, 1] = vn.y;

        transformMatrix[2, 2] = vn.z;

        transformMatrix[2, 3] = 0;

        transformMatrix[3, 0] = 0;

        transformMatrix[3, 1] = 0;

        transformMatrix[3, 2] = 0;

        transformMatrix[3, 3] = 1;

        

        //Now for the eye transform

        eyeTranslateM = new Matrix4x4();

        eyeTranslateM[0, 0] = 1;

        eyeTranslateM[0, 1] = 0;

        eyeTranslateM[0, 2] = 0;

        eyeTranslateM[0, 3] = -pe.x;

        eyeTranslateM[1, 0] = 0;

        eyeTranslateM[1, 1] = 1;

        eyeTranslateM[1, 2] = 0;

        eyeTranslateM[1, 3] = -pe.y;

        eyeTranslateM[2, 0] = 0;

        eyeTranslateM[2, 1] = 0;

        eyeTranslateM[2, 2] = 1;

        eyeTranslateM[2, 3] = -pe.z;

        eyeTranslateM[3, 0] = 0;

        eyeTranslateM[3, 1] = 0;

        eyeTranslateM[3, 2] = 0;

        eyeTranslateM[3, 3] = 1f;

        

        //Multiply all together

        finalProjection = new Matrix4x4();

        finalProjection = Matrix4x4.identity * projectionM*transformMatrix*eyeTranslateM;

        

        //finally return

        return finalProjection;

    }
	
	
	
	
	static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far) {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
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
	
	void getInitialConditions()
	{
		
		string chars = " ,:\tabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()/?><";
        char[] delimiterChars = chars.ToCharArray();
		string iuc = comm.getInitial();
		
		double.TryParse(comm.getScreen(), out screen);
		string ar = comm.getAspect();
		
		string[] words = iuc.Split(delimiterChars);
        foreach (string word in words)
        {
            print(word);
        }
        //double degrees, height, dist;
		double degrees, dist, height, x, y, z;
        double.TryParse(words[0], out degrees);
        double.TryParse(words[1], out dist);
        double.TryParse(words[2], out height);
		double.TryParse(words[3], out x);
		double.TryParse(words[4], out y);
		double.TryParse(words[5], out z);
		
		initX = x;
		initY = y;
		initZ = z;

		words = ar.Split(delimiterChars);
		
        double.TryParse(words[0], out ARw);
        double.TryParse(words[1], out ARh);
		
		print ("initial conditions are: " + iuc + "     " + screen.ToString() + "    " +ARw + " : " +ARh);	
		
		// Calculate Sw/Sh from ARw/ARh/Screen
		
		screenangle = tofloat(Math.Atan(ARh/ARw));
		
		Sw = tofloat(((screen * Math.Cos(screenangle)) * 2.54) / 100.0);
		Sh = tofloat(((screen * Math.Sin(screenangle)) * 2.54) / 100.0);
		
		RrotatedX = tofloat((Sw/2.0) * Math.Cos((degrees-90.0) * (3.14159/180.0)));
		RrotatedY = tofloat((Sw/2.0) * Math.Sin((degrees-90.0) * (3.14159/180.0)));
		LrotatedX = tofloat((-Sw/2.0) * Math.Cos((degrees-90.0) * (3.14159/180.0)));
		LrotatedY = tofloat((-Sw/2.0) * Math.Sin((degrees-90.0) * (3.14159/180.0)));
		
		LtranslatedX = tofloat(LrotatedX + (dist * Math.Cos(degrees * (3.14159/180.0))));
		LtranslatedY = tofloat(LrotatedY - (dist * Math.Sin(degrees * (3.14159/180.0))));
		RtranslatedX = tofloat(RrotatedX + (dist * Math.Cos(degrees * (3.14159/180.0))));
		RtranslatedY = tofloat(RrotatedY - (dist * Math.Sin(degrees * (3.14159/180.0))));

		
		
		
	}

}
