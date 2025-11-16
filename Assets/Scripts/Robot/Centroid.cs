using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Centroid : MonoBehaviour
{
    public Transform objetoCentro;     // A
    public Transform objetoReferencia; // C
    public bool usarRotacion = true;

    void Update()
    {
        if (objetoCentro == null || objetoReferencia == null)
            return;

        // 1️⃣ Mantener posición en el centro del objeto A
        transform.position = objetoCentro.position;

        // 2️⃣ Copiar solo la rotación del objeto C
        if (usarRotacion)
            transform.rotation = objetoReferencia.rotation;
    }
}
