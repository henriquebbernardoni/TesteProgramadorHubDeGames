using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTextToCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        PointText();
    }

    //Caso a rota��o da c�mera mude entre um frame e outro,
    //a rota��o ir� modificar para comportar a altera��o.
    private void PointText()
    {
        if (transform.rotation != Camera.main.transform.rotation)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}