using UnityEngine;
using UnityEngine.EventSystems;

public class VASButtonOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private EncuestaVASManager encuestaManager;
    [SerializeField] private int score;
    [SerializeField] private GameObject glowVisual;

    [Header("Activación por toque")]
    [SerializeField] private string handTag = "Hand";
    [SerializeField] private float activacionCooldown = 0.2f;

    private float ultimoTiempoActivacion = -999f;

    public void Seleccionar()
    {
        if (encuestaManager == null)
            return;

        if (encuestaManager.RespuestaEnviada)
            return;

        if (Time.time - ultimoTiempoActivacion < activacionCooldown)
            return;

        ultimoTiempoActivacion = Time.time;
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

    private void OnTriggerEnter(Collider other)
    {
        if (encuestaManager != null && encuestaManager.RespuestaEnviada)
            return;

        if (!other.CompareTag(handTag))
            return;

        if (glowVisual != null)
            glowVisual.SetActive(true);

        Seleccionar();
    }

    private void OnTriggerStay(Collider other)
    {
        if (encuestaManager != null && encuestaManager.RespuestaEnviada)
            return;

        if (!other.CompareTag(handTag))
            return;

        if (glowVisual != null)
            glowVisual.SetActive(true);

        Seleccionar();
    }

    private void OnTriggerExit(Collider other)
    {
        if (encuestaManager != null && encuestaManager.RespuestaEnviada)
            return;

        if (!other.CompareTag(handTag))
            return;

        if (glowVisual != null)
            glowVisual.SetActive(false);
    }
}