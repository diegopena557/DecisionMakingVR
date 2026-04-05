using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HighlightOnTouch : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField]
    private MeshRenderer highlightRenderer; // El MeshRenderer que queremos mostrar/ocultar

    [Header("Trigger Colliders")]
    [SerializeField]
    private Collider[] controllerColliders; // Lista de colliders que actuar�n como controladores

    [Header("Grab Settings")]
    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable; // Componente XRGrabInteractable del objeto

    private int touchingObjects = 0; // Contador de objetos en contacto

    void Start()
    {
        // Verificar referencias iniciales
        if (highlightRenderer != null)
        {
            highlightRenderer.enabled = false; // Asegurarnos de que est� oculto al inicio
        }
        else
        {
            Debug.LogWarning("Highlight Renderer no asignado. Por favor, arrastra el MeshRenderer al inspector.");
        }

        if (controllerColliders == null || controllerColliders.Length == 0)
        {
            Debug.LogWarning("No se han asignado colliders de controladores. Aseg�rate de configurarlos en el inspector.");
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
        else
        {
            Debug.LogWarning("El componente XRGrabInteractable no est� asignado.");
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsControllerCollider(other) && !IsBeingGrabbed())
        {
            touchingObjects++;
            UpdateHighlight(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsControllerCollider(other))
        {
            touchingObjects--;
            if (touchingObjects <= 0 && !IsBeingGrabbed())
            {
                touchingObjects = 0; // Evitar valores negativos
                UpdateHighlight(false);
            }
        }
    }

    private void UpdateHighlight(bool show)
    {
        if (highlightRenderer != null)
        {
            highlightRenderer.enabled = show;
        }
    }

    private bool IsControllerCollider(Collider other)
    {
        // Verifica si el collider est� en la lista de colliders de controladores
        foreach (Collider controllerCollider in controllerColliders)
        {
            if (controllerCollider == other)
            {
                return true;
            }
        }
        return false;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Desactivar el highlight cuando el objeto es agarrado
        UpdateHighlight(false);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        // Reactivar el highlight si hay objetos tocando
        if (touchingObjects > 0)
        {
            UpdateHighlight(true);
        }
    }

    private bool IsBeingGrabbed()
    {
        // Verifica si el objeto est� siendo agarrado
        return grabInteractable != null && grabInteractable.isSelected;
    }
}
