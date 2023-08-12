using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTextToCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        PointText();
    }

    //Caso a rotação da câmera mude entre um frame e outro,
    //a rotação irá modificar para comportar a alteração.
    private void PointText()
    {
        if (transform.rotation != Camera.main.transform.rotation)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}