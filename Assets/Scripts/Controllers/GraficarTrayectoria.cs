using System.Collections.Generic;
using UnityEngine;

public class GraficarTrayectoria : MonoBehaviour
{
    [Header("Panel donde se dibuja")]
    [SerializeField] private RectTransform panelGrafica;

    [Header("Lineas")]
    [SerializeField] private LineRenderer lineaIdeal;
    [SerializeField] private LineRenderer lineaUsuario;

    public void DibujarTrayectorias(List<Vector3> trayectoriaUsuario, List<Vector3> trayectoriaIdeal)
    {
        if (trayectoriaUsuario == null || trayectoriaUsuario.Count == 0)
            return;

        if (trayectoriaIdeal == null || trayectoriaIdeal.Count == 0)
            return;

        // Calcular límites usando ambas trayectorias
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (Vector3 p in trayectoriaUsuario)
        {
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);

            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        foreach (Vector3 p in trayectoriaIdeal)
        {
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);

            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        // Evitar división entre cero
        if (Mathf.Abs(maxX - minX) < 0.001f)
            maxX += 0.001f;

        if (Mathf.Abs(maxY - minY) < 0.001f)
            maxY += 0.001f;

        DibujarLinea(lineaUsuario, trayectoriaUsuario, minX, maxX, minY, maxY);

        DibujarLinea(lineaIdeal, trayectoriaIdeal, minX, maxX, minY, maxY);
    }

    private void DibujarLinea(LineRenderer linea,
                              List<Vector3> puntos,
                              float minX,
                              float maxX,
                              float minY,
                              float maxY)
    {
        float ancho = panelGrafica.rect.width;
        float alto = panelGrafica.rect.height;

        linea.useWorldSpace = false;
        linea.positionCount = puntos.Count;

        for (int i = 0; i < puntos.Count; i++)
        {
            float x = Mathf.InverseLerp(minX, maxX, puntos[i].x);
            float y = Mathf.InverseLerp(minY, maxY, puntos[i].y);

            x = (x - 0.5f) * ancho;
            y = (y - 0.5f) * alto;

            linea.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}