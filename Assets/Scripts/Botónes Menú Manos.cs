using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Collider))]
public class UIButtonTouchActivator : MonoBehaviour
{
    [SerializeField] private Button targetButton;
    [SerializeField] private string handTag = "Hand";
    [SerializeField] private float activationCooldown = 0.2f;
    [SerializeField] private bool activarSoloSiInteractable = true;

    private float ultimoTiempoActivacion = -999f;

    private void Reset()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(handTag))
            return;

        ActivarBoton();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(handTag))
            return;

        ActivarBoton();
    }

    private void ActivarBoton()
    {
        if (targetButton == null)
            return;

        if (activarSoloSiInteractable && !targetButton.interactable)
            return;

        if (Time.time - ultimoTiempoActivacion < activationCooldown)
            return;

        ultimoTiempoActivacion = Time.time;
        targetButton.onClick.Invoke();
    }
}