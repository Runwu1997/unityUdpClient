using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCube : MonoBehaviour
{
    public float angularSpeed = 100;
    public string id = string.Empty;
   
    void Start()
    {
    }


  
    void Update()
    {
        var angle = angularSpeed * Time.deltaTime;
        this.transform.Rotate(0,angle,0);
    }

    public void ChangeColor(float r, float g, float b)
    {
        this.gameObject.GetComponent<Renderer>().material.color = new Color(r , g , b , 1.0f);        

    }
}
