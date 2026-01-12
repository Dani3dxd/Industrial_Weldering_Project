using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class GuardarDatos : MonoBehaviour
{
    private string filePath;
    private string fileName = "datos_experimento.csv";
    public ExecuteTrajectories trayectoriasEjecutadas;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        // Crear archivo con encabezados si no existe
        if (!File.Exists(filePath))
        {
            CrearArchivo();
        }
    }

    void CrearArchivo()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Indice PosicionX PosicionY PosicionZ");

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        Debug.Log("Archivo CSV creado en: " + filePath);
    }

    public void GuardarFila()
    {
        if (trayectoriasEjecutadas.trajectory == null || trayectoriasEjecutadas.trajectory.Count == 0)
        {
            Debug.LogWarning("La trayectoria está vacía");
            return;
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < trayectoriasEjecutadas.trajectory.Count; i++)
        {
            //float tiempo = trayectoriasEjecutadas.partialTime[i];
            Vector3 pos = trayectoriasEjecutadas.trajectory[i];
            sb.AppendLine($"{i} {pos.x:F4} {pos.y:F4} {pos.z:F4}");
        }
        sb.AppendLine("TiempoTotalDeEjecucion: ");
        sb.AppendLine($"{trayectoriasEjecutadas.finalTime:F2}");
        File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
    }
}
