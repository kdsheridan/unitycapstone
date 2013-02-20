using UnityEngine;
using System.Collections;



public class NewBehaviourScript : MonoBehaviour
{
    public float moveSpeed = 25.0f;
    public float turnSpeed = 90.0f;
    public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;
    public float hor = 10.0F;
    public float ver = 10.0F;
    public Camera mainCamera;
    public Camera otherCamera;
    public new Vector3 minitial;
    public new Vector3 mrot;
    public new Vector3 oinitial;
    public new Vector3 orot;
    // Use this for initialization
    void Start()
    {
        minitial = mainCamera.transform.position;
        mrot = mainCamera.transform.localEulerAngles;
        oinitial = otherCamera.transform.position;
        orot = otherCamera.transform.localEulerAngles;
        otherCamera.enabled = false;
        mainCamera.enabled = true;

    }
    //use transform.rotate with degrees information
    // Update is called once per frame
    void Update()
    {
        cameraselect();
        if (mainCamera.enabled)
            camera1(mainCamera);
        else
            camera2(otherCamera);
        reset();
    }




    void camera1(Camera Camera)
    {
        if (mainCamera.enabled)
        {
            if (Input.GetKey(KeyCode.W))
            {
                Camera.transform.position += Camera.transform.forward * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                Camera.transform.position += -Camera.transform.forward * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                Camera.transform.position += Camera.transform.right * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                Camera.transform.position += -Camera.transform.right * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                Camera.transform.localEulerAngles = new Vector3(Camera.transform.localEulerAngles.x, Camera.transform.localEulerAngles.y - turnSpeed * Time.deltaTime, Camera.transform.localEulerAngles.z);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                Camera.transform.localEulerAngles = new Vector3(Camera.transform.localEulerAngles.x, Camera.transform.localEulerAngles.y + turnSpeed * Time.deltaTime, Camera.transform.localEulerAngles.z);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                Camera.transform.localEulerAngles = new Vector3(Camera.transform.localEulerAngles.x - turnSpeed * Time.deltaTime, Camera.transform.localEulerAngles.y, Camera.transform.localEulerAngles.z);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                Camera.transform.localEulerAngles = new Vector3(Camera.transform.localEulerAngles.x + turnSpeed * Time.deltaTime, Camera.transform.localEulerAngles.y, Camera.transform.localEulerAngles.z);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Camera.transform.Rotate(0, 30, 0);
            }
        }

    }

    void camera2(Camera Camera)
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            float v = -verticalSpeed * Input.GetAxis("Mouse Y");
            Camera.transform.Rotate (v, h, 0);

        }

        else
        {
            float h = hor * Input.GetAxis("Mouse X");
            float v = -ver * Input.GetAxis("Mouse Y");
            Camera.transform.position += Camera.transform.forward * v * Time.deltaTime;
            Camera.transform.position += Camera.transform.right * h * Time.deltaTime;
        }



    }

    void cameraselect()
    {
        if (Input.GetKey(KeyCode.C))
            {
                mainCamera.enabled = false;
                otherCamera.enabled = true;
            }
 
            if (Input.GetKey(KeyCode.V))
            {
                otherCamera.enabled = false;
                mainCamera.enabled = true;
            }
    }

    void reset()
    {
        if (Input.GetKey(KeyCode.R))
        {
            mainCamera.transform.position = minitial;
            mainCamera.transform.localEulerAngles = mrot;
            otherCamera.transform.position = oinitial;
            otherCamera.transform.localEulerAngles = orot;
        }
    }

}



