using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Content.Interaction;
using TMPro;

public class ControladorValoresPanel : MonoBehaviour
{
    [System.Serializable]
    public class VariablesDisplay
    {
        public XRSlider valorSlider;
        public TMP_Text valorTexto;
        public float multiplicador = 1f;
    }

    [Header("Referencias")]
    [SerializeField] private List<VariablesDisplay> valorDisplay = new();

    void Start()
    {
        foreach (var v in valorDisplay)
            if (v.valorSlider && v.valorTexto)
                v.valorSlider.onValueChange.AddListener(val =>
                    v.valorTexto.text = $"{val * v.multiplicador:F1}");
    }

    void OnDestroy()
    {
        foreach (var v in valorDisplay)
            if (v.valorSlider)
                v.valorSlider.onValueChange.RemoveAllListeners();
    }
}
