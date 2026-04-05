using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ReturnToOriginalPositionWithTriggerEffect : MonoBehaviour
{
    private Vector3 localOriginalPosition;
    private Quaternion localOriginalRotation;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    [Header("Return Settings")]
    [SerializeField]
    private float returnSpeed = 10f; // Velocidad de retorno ajustable
    private bool isReturning = false;

    [Header("Parent Settings")]
    [SerializeField]
    private Transform parentTransform; // Objeto padre asignable desde el inspector

    [Header("Material and Light Settings")]
    [SerializeField]
    private Light targetLight;
    [SerializeField]
    private Material activatedMaterial;
    [SerializeField]
    private Material originalMaterial;
    [SerializeField]
    private Renderer lightRenderer;

    [Header("Sound Settings")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip activationSound;

    [Header("Input Settings")]
    [SerializeField]
    private InputActionReference primaryButtonActionRight; // Bot�n primario del controlador derecho
    [SerializeField]
    private InputActionReference primaryButtonActionLeft;  // Bot�n primario del controlador izquierdo

    [Header("Interactor Settings")]
    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor rightHandInteractor; // Interactor de la mano derecha
    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor leftHandInteractor;  // Interactor de la mano izquierda

    private bool isPrimaryButtonHeld = false;
    private float buttonHoldTime = 0f;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor currentInteractor = null; // Interactor actualmente sosteniendo el objeto

    void Start()
    {
        // Guardar la posici�n y rotaci�n iniciales locales
        UpdateOriginalPosition();

        // Obtener el componente XRGrabInteractable
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
        else
        {
            Debug.LogWarning("XRGrabInteractable no encontrado en el objeto.");
        }

        // Configurar las acciones de los botones
        if (primaryButtonActionRight != null)
        {
            primaryButtonActionRight.action.started += OnPrimaryButtonPressedRight;
            primaryButtonActionRight.action.canceled += OnPrimaryButtonReleased;
        }

        if (primaryButtonActionLeft != null)
        {
            primaryButtonActionLeft.action.started += OnPrimaryButtonPressedLeft;
            primaryButtonActionLeft.action.canceled += OnPrimaryButtonReleased;
        }

        // Validar configuraciones de luz
        if (targetLight == null || lightRenderer == null || originalMaterial == null || activatedMaterial == null)
        {
            Debug.LogWarning("Aseg�rate de asignar todos los par�metros de luz y materiales.");
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }

        if (primaryButtonActionRight != null)
        {
            primaryButtonActionRight.action.started -= OnPrimaryButtonPressedRight;
            primaryButtonActionRight.action.canceled -= OnPrimaryButtonReleased;
        }

        if (primaryButtonActionLeft != null)
        {
            primaryButtonActionLeft.action.started -= OnPrimaryButtonPressedLeft;
            primaryButtonActionLeft.action.canceled -= OnPrimaryButtonReleased;
        }
    }

    void Update()
    {
        if (isReturning)
        {
            // Interpolar la posici�n y rotaci�n hacia la original
            Vector3 targetPosition = parentTransform ? parentTransform.TransformPoint(localOriginalPosition) : localOriginalPosition;
            Quaternion targetRotation = parentTransform ? parentTransform.rotation * localOriginalRotation : localOriginalRotation;

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * returnSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * returnSpeed);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f &&
                Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
                isReturning = false;

                // Actualizar la posici�n original despu�s de completar el retorno
                UpdateOriginalPosition();
            }
        }

        if (isPrimaryButtonHeld)
        {
            buttonHoldTime += Time.deltaTime;

            if (buttonHoldTime >= 2f)
            {
                ActivateLightEffect();
                isPrimaryButtonHeld = false;
            }
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Determinar qu� interactor est� sosteniendo el objeto
        if (args.interactorObject.Equals(rightHandInteractor))
        {
            currentInteractor = rightHandInteractor;
        }
        else if (args.interactorObject.Equals(leftHandInteractor))
        {
            currentInteractor = leftHandInteractor;
        }

        // Actualizar la posici�n local para evitar desfase
        UpdateOriginalPosition();
    }

    private void OnPrimaryButtonPressedRight(InputAction.CallbackContext context)
    {
        if (currentInteractor == rightHandInteractor)
        {
            isPrimaryButtonHeld = true;
            buttonHoldTime = 0f;
        }
    }

    private void OnPrimaryButtonPressedLeft(InputAction.CallbackContext context)
    {
        if (currentInteractor == leftHandInteractor)
        {
            isPrimaryButtonHeld = true;
            buttonHoldTime = 0f;
        }
    }

    private void OnPrimaryButtonReleased(InputAction.CallbackContext context)
    {
        isPrimaryButtonHeld = false;
        buttonHoldTime = 0f;

        // Restaurar el material despu�s de 2 segundos
        Invoke(nameof(RestoreLightEffect), 2f);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isReturning = true;
        currentInteractor = null; // Liberar el interactor actual
    }

    private void ActivateLightEffect()
    {
        if (lightRenderer != null && activatedMaterial != null)
        {
            lightRenderer.material = activatedMaterial;
        }

        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
    }

    private void RestoreLightEffect()
    {
        if (lightRenderer != null && originalMaterial != null)
        {
            lightRenderer.material = originalMaterial;
        }
    }

    private void UpdateOriginalPosition()
    {
        if (parentTransform)
        {
            localOriginalPosition = parentTransform.InverseTransformPoint(transform.position);
            localOriginalRotation = Quaternion.Inverse(parentTransform.rotation) * transform.rotation;
        }
        else
        {
            localOriginalPosition = transform.position;
            localOriginalRotation = transform.rotation;
        }
    }
}