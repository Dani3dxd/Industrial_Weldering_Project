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
    [SerializeField] private GameObject objetoIniciar;
    [SerializeField] private GraficarTrayectoria grafica;
    private Vector3 pos = Vector3.zero;
    private Vector3[] posTutorial;
    private string filePath;
    private string fileName = "datos_experimento.csv";
    private int numeroPractica = 1;
    public ExecuteTrajectories trayectoriasEjecutadas;
    public ControladorValoresPanel controlValoresPanel;
    public IniciarEscena IniciarEscena;

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

            sb.AppendLine("Practica,Fecha,Hora,Material,Voltaje,Corriente,Tiempo,Puntos");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public void GuardarInformacion()
    {
        float voltaje = controlValoresPanel.valorDisplay[0].dato;
        float corriente = controlValoresPanel.valorDisplay[1].dato;

        string voltajeTxt = controlValoresPanel.valorDisplay[0].valorTexto.text;
        string corrienteTxt = controlValoresPanel.valorDisplay[1].valorTexto.text;

        // Determinar configuración según el material seleccionado
        int puntos = trayectoriasEjecutadas.trajectory.Count;
        float vMin = 0, vMax = 0, cMin = 0, cMax = 0;
        string material = "";

        posTutorial = null;

        switch (IniciarEscena.currentStep)
        {
            // Acero inoxidable
            case 0:
                material = "Acero inoxidable";
                vMin = 20;
                vMax = 26;
                cMin = 80;
                cMax = 140;
                break;

            // Acero al carbono
            case 1:
                material = "Acero al carbono";
                vMin = 20;
                vMax = 26;
                cMin = 90;
                cMax = 160;
                break;

            // Aluminio
            case 2:
                material = "Aluminio";
                vMin = 24;
                vMax = 30;
                cMin = 110;
                cMax = 180;
                break;
        }

        // Validación de valores
        bool valoresCorrectos =
            voltaje >= vMin && voltaje <= vMax &&
            corriente >= cMin && corriente <= cMax;

        textosDatos.text =
            $"Material: {material}\n" +
            $"Voltaje: {voltajeTxt} V\n" +
            $"Corriente: {corrienteTxt} A";

        if (!valoresCorrectos)
        {
            textosDatos.text +=
                $"\n\nValores recomendados para {material}:" +
                $"\nVoltaje: {vMin} - {vMax} V" +
                $"\nCorriente: {cMin} - {cMax} A";
        }
        else
        {
            textosDatos.text += "\n\n✓ Parámetros correctos.";
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

        List<Vector3> trayectoriaIdeal = new List<Vector3>();

        switch (IniciarEscena.indicepublic)
        {
            // Figura 1
            case 0:

                trayectoriaIdeal.Add(new Vector3(-0.305f, 0.779f, 0.299f));
                trayectoriaIdeal.Add(new Vector3(-0.123f, 0.657f, 0.311f));
                trayectoriaIdeal.Add(new Vector3(0.120f, 0.664f, 0.310f));
                trayectoriaIdeal.Add(new Vector3(0.348f, 0.664f, 0.311f));
                trayectoriaIdeal.Add(new Vector3(0.121f, 0.661f, 0.311f));
                trayectoriaIdeal.Add(new Vector3(0.119f, 0.649f, 0.536f));
                trayectoriaIdeal.Add(new Vector3(0.120f, 0.667f, 0.313f));
                trayectoriaIdeal.Add(new Vector3(-0.122f, 0.653f, 0.313f));
                trayectoriaIdeal.Add(new Vector3(-0.126f, 0.641f, 0.534f));

                break;

            // Figura 2
            case 1:

                trayectoriaIdeal.Add(new Vector3(-0.353f, 0.655f, 0.305f));
                trayectoriaIdeal.Add(new Vector3(0.106f, 0.667f, 0.307f));
                trayectoriaIdeal.Add(new Vector3(-0.124f, 0.656f, 0.305f));
                trayectoriaIdeal.Add(new Vector3(-0.124f, 0.861f, 0.298f));
                trayectoriaIdeal.Add(new Vector3(-0.126f, 0.659f, 0.301f));
                trayectoriaIdeal.Add(new Vector3(-0.127f, 0.643f, 0.535f));

                break;
        }

        grafica.DibujarTrayectorias(
            trayectoriasEjecutadas.trajectory,
            trayectoriaIdeal);

    }
    public void desactivarMostrarResultados()
    {
        trayectoriasEjecutadas.panelResults.SetActive(false);
    }
    public void GuardarFila()
    {
        if (trayectoriasEjecutadas.trajectory == null ||
            trayectoriasEjecutadas.trajectory.Count == 0)
            return;

        float voltaje = controlValoresPanel.valorDisplay[0].dato;
        float corriente = controlValoresPanel.valorDisplay[1].dato;

        string material = "";

        switch (IniciarEscena.currentStep)
        {
            case 0:
                material = "Acero inoxidable";
                break;

            case 1:
                material = "Acero al carbono";
                break;

            case 2:
                material = "Aluminio";
                break;
        }

        StringBuilder puntos = new StringBuilder();

        for (int i = 0; i < trayectoriasEjecutadas.trajectory.Count; i++)
        {
            Vector3 p = trayectoriasEjecutadas.trajectory[i];

            puntos.Append($"{p.x:F3}|{p.y:F3}|{p.z:F3}");

            if (i < trayectoriasEjecutadas.trajectory.Count - 1)
                puntos.Append(";");
        }

        string fila =
            $"{numeroPractica}," +
            $"{DateTime.Now:dd/MM/yyyy}," +
            $"{DateTime.Now:HH:mm:ss}," +
            $"{material}," +
            $"{voltaje:F1}," +
            $"{corriente:F1}," +
            $"{trayectoriasEjecutadas.finalTime:F2}," +
            $"\"{puntos}\"";

        File.AppendAllText(filePath, fila + Environment.NewLine, Encoding.UTF8);

        numeroPractica++;
    }
}
