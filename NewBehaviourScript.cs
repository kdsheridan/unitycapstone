using UnityEngine;
using System.Collections;



public class NewBehaviourScript : MonoBehaviour
{
    float moveSpeed = 25.0f;
    float turnSpeed = 90.0f;

    // Use this for initialization
    void Start()
    {

       

    }
    //use transform.rotate with degrees information
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -transform.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y - turnSpeed * Time.deltaTime, transform.localEulerAngles.z);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + turnSpeed * Time.deltaTime, transform.localEulerAngles.z);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x - turnSpeed * Time.deltaTime, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + turnSpeed * Time.deltaTime, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.Rotate(0,30,0);
        }
    }
  
}