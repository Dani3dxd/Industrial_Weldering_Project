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
    [SerializeField] private GameObject tutorialTrapecio;
    [SerializeField] private GameObject tutorialFigL;
    private Vector3 pos = Vector3.zero;
    private Vector3[] posTutorial;
    private string filePath;
    private string fileName = "datos_experimento.csv";
    private int numeroPractica = 1;
    public ExecuteTrajectories trayectoriasEjecutadas;
    public ControladorValoresPanel controlValoresPanel;

    void Awake()
    {
        Debug.Log("PersistentDataPath: " + Application.persistentDataPath);
        Debug.Log("StreamingAssetsPath: " + Application.streamingAssetsPath);
        Debug.Log("TemporaryCachePath: " + Application.temporaryCachePath);

#if UNITY_ANDROID && !UNITY_EDITOR

    string folder = "/storage/emulated/0/Documents";

    try
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        filePath = Path.Combine(folder, fileName);
    }
    catch (System.Exception e)
    {
        Debug.LogError("Error creando carpeta: " + e);
    }

#else

        filePath = Path.Combine(Application.persistentDataPath, fileName);

#endif

        Debug.Log("Ruta seleccionada: " + filePath);

        if (!File.Exists(filePath))
        {
            CrearArchivo();
        }
    }

    void CrearArchivo()
    {
        try
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SIMULADOR DE SOLDADURA CON COBOT");
            sb.AppendLine("============================================");
            sb.AppendLine();

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

            Debug.Log("Archivo creado: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public void GuardarInformacion()
    {
        float voltaje = controlValoresPanel.valorDisplay[0].dato;
        float corriente = controlValoresPanel.valorDisplay[1].dato;

        string voltajeTxt = controlValoresPanel.valorDisplay[0].valorTexto.text;
        string corrienteTxt = controlValoresPanel.valorDisplay[1].valorTexto.text;

        // Determinar configuración según tutorial activo
        int puntos = 0;
        float vMin = 0, vMax = 0, cMin = 0, cMax = 0;
        //string material = "";
        posTutorial = null;

        if (tutorialTrapecio.activeSelf)
        {
            puntos = 2;
            vMin = 20; vMax = 26; cMin = 7; cMax = 14;
            //material = "acero inoxidable";
            posTutorial = new Vector3[]
            {
            new Vector3(0.12f, 0.63f, 0.3f),
            new Vector3(0.12f, 0.4f, 0.3f)
            };
        }
        else if (tutorialFigL.activeSelf)
        {
            puntos = 3;
            vMin = 24; vMax = 30; cMin = 11; cMax = 18;
            //material = "aluminio";
            posTutorial = new Vector3[]
            {
            new Vector3(-0.35f, 0.63f, 0.3f),
            new Vector3(-0.12f, 0.63f, 0.3f),
            new Vector3(-0.12f, 0.86f, 0.3f)
            };
        }
        else
        {
            puntos = trayectoriasEjecutadas.trajectory.Count;
            vMin = 20; vMax = 30; cMin = 7; cMax = 18;
        }

        // Validación de valores
        bool valoresCorrectos = voltaje > vMin && voltaje < vMax &&
                                corriente >= cMin && corriente <= cMax;

        textosDatos.text = $"Valor de voltaje de: {voltajeTxt} y corriente de: {corrienteTxt}";

        if (!valoresCorrectos)
        {
            /*string recomendacion = material != "" ?
                $"para el {material} entre: {vMin}V a {vMax}V y {cMin}A a {cMax}A" :
                $"entre: {vMin}V a {vMax}V y {cMin}A a {cMax}A";*/

            textosDatos.text += $"\nSe recomienda usar valores entre: {vMin}V a {vMax}V y {cMin}A a {cMax}A";
        }

        // Limpiar contenido anterior
        foreach (Transform contenedor in contenidoContainer)
            Destroy(contenedor.gameObject);

        // Generar puntos
        for (int i = 0; i < puntos; i++)
        {
            GameObject newText = Instantiate(prefabTexto, contenidoContainer);
            Vector3 pos = trayectoriasEjecutadas.trajectory[i];

            string texto = $"Punto_{i + 1}: X ({pos.x:F2}), Y ({pos.y:F2}), Z({pos.z:F2})";
            newText.GetComponent<TMP_Text>().text = texto;
            if (posTutorial != null)
            {
                int index = (i < posTutorial.Length) ? i : i % posTutorial.Length;

                if (Vector3.Distance(pos, posTutorial[index]) >= 0.03f)
                {
                    GameObject warningText = Instantiate(prefabTexto, contenidoContainer);
                    Vector3 refPos = posTutorial[index];
                    texto = $"Punto de Ref_{i + 1}: X({refPos.x:F2}), Y ({refPos.y:F2}), Z({refPos.z:F2})";
                    warningText.GetComponent<TMP_Text>().text = texto;
                }
            }

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

        sb.AppendLine("===============================================");
        sb.AppendLine($"PRACTICA {numeroPractica}");
        sb.AppendLine($"Fecha: {System.DateTime.Now:dd/MM/yyyy}");
        sb.AppendLine($"Hora : {System.DateTime.Now:HH:mm:ss}");
        sb.AppendLine();

        sb.AppendLine("Punto,X,Y,Z");

        for (int i = 0; i < trayectoriasEjecutadas.trajectory.Count; i++)
        {
            pos = trayectoriasEjecutadas.trajectory[i];

            sb.AppendLine(
                $"{i + 1}," +
                $"{pos.x:F3}," +
                $"{pos.y:F3}," +
                $"{pos.z:F3}");
        }

        sb.AppendLine();

        sb.AppendLine($"Tiempo Total (s),{trayectoriasEjecutadas.finalTime:F2}");

        float voltaje = controlValoresPanel.valorDisplay[0].dato;
        float corriente = controlValoresPanel.valorDisplay[1].dato;

        sb.AppendLine($"Voltaje (V),{voltaje:F1}");
        sb.AppendLine($"Corriente (A),{corriente:F1}");

        sb.AppendLine();

        try
        {
            File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);

            numeroPractica++;

            Debug.Log("Trayectoria guardada en: " + filePath);

            trayectoriasEjecutadas.textoTiempo.text = "Datos guardados correctamente";
            textosDatos.text = filePath;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());

            trayectoriasEjecutadas.textoTiempo.text = "Error al guardar";
            textosDatos.text = e.Message;
        }
    }
}
