using UnityEngine;
using UnityEngine.EventSystems;

public class VASButtonOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private EncuestaVASManager encuestaManager;
    [SerializeField] private int score;
    [SerializeField] private GameObject glowVisual;

    public void Seleccionar()
    {
        if (encuestaManager == null)
            return;

        encuestaManager.RegistrarRespuesta(score, glowVisual);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (encuestaManager != null && encuestaManager.RespuestaEnviada)
            return;

        if (glowVisual != null)
            glowVisual.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (encuestaManager != null && encuestaManager.RespuestaEnviada)
            return;

        if (glowVisual != null)
            glowVisual.SetActive(false);
    }
}