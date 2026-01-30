using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class GuardarDatos : MonoBehaviour
{
    [SerializeField] private TMP_Text textosDatos;
    [SerializeField] private Transform contenidoContainer;
    [SerializeField] private GameObject prefabTexto;
    private string filePath;
    private string fileName = "datos_experimento.csv";
    public ExecuteTrajectories trayectoriasEjecutadas;
    public ControladorValoresPanel controlValoresPanel;

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
    
    public void GuardarInformacion()
    {

        if (controlValoresPanel.valorDisplay[0].dato > 75f && controlValoresPanel.valorDisplay[0].dato < 85f && controlValoresPanel.valorDisplay[1].dato >= 10f && controlValoresPanel.valorDisplay[1].dato <= 15f)
            textosDatos.text = "Valor de voltaje de: " + controlValoresPanel.valorDisplay[0].valorTexto.text + " y corriente de:" + controlValoresPanel.valorDisplay[1].valorTexto.text;
        else
            textosDatos.text = "Valor de voltaje de: " + controlValoresPanel.valorDisplay[0].valorTexto.text + " y corriente de:" + controlValoresPanel.valorDisplay[1].valorTexto.text +
                    "\nSe recomienda utilizar valores de: 80 V y 12 A";
        foreach (Transform contenedor in contenidoContainer)
            Destroy(contenedor.gameObject);

        for (int i = 0; i < trayectoriasEjecutadas.trajectory.Count; i++)
            {
                GameObject newText=Instantiate(prefabTexto, contenidoContainer);
                Vector3 pos = trayectoriasEjecutadas.trajectory[i];
                newText.GetComponent<TMP_Text>().text = $"Punto_{i+1} = X ({pos.x:F2}), Y ({pos.y:F2}), Z({pos.z:F2})";
            }
        
    }
    public void desactivarMostrarResultados()
    {
        trayectoriasEjecutadas.panelResults.SetActive(false);
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
            sb.AppendLine($"{i+1}   {pos.x:F4} {pos.y:F4} {pos.z:F4}");
        }
        sb.AppendLine("TiempoTotalDeEjecucion: ");
        sb.AppendLine($"{trayectoriasEjecutadas.finalTime:F2}");
        File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
        Debug.Log("Trayectoria guardada en: " + filePath);
        
        trayectoriasEjecutadas.textoTiempo.text = "Datos guardados correctamente en:";
        textosDatos.text = filePath;
    }
}
