using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCube : MonoBehaviour
{
    public float rotateSpeed = 100;
    public string id = string.Empty;

    public float movingSpeed = 10;
    public bool myCube = false;
    public Vector3 newPosition = new Vector3();
    void Start()
    {
    }

  
    void Update()
    {
        var angle = rotateSpeed * Time.deltaTime;
        this.transform.Rotate(0, angle, 0);
        if (myCube)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                newPosition += new Vector3(-movingSpeed * Time.deltaTime, 0, 0);
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                newPosition += new Vector3(movingSpeed * Time.deltaTime, 0, 0);
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                newPosition += new Vector3(0, movingSpeed * Time.deltaTime, 0);
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                newPosition += new Vector3(0, -movingSpeed * Time.deltaTime, 0);

        }
    }

    public void ChangeColor(float r, float g, float b)
    {
        this.gameObject.GetComponent<Renderer>().material.color = new Color(r , g , b , 1.0f);        

    }
}
