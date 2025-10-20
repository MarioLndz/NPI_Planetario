using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public GameObject objeto;
    public float rotationSpeed = 50.0f;
    

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(objeto.transform.position, objeto.transform.up, rotationSpeed * Time.deltaTime);

    }
}