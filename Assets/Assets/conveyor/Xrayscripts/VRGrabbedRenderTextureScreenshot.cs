using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el XR Input System
using UnityEngine.XR.Interaction.Toolkit; // Para trabajar con interacciones XR

[ExecuteAlways]
public class VRMaterialScreenshot : MonoBehaviour
{
    public Camera renderCamera; // C�mara que renderiza la escena
    public InputActionReference primaryButtonAction; // Acci�n del bot�n primario del control VR
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor interactor; // Interactor que verifica si est� agarrando algo
    public GameObject requiredObject; // Objeto espec�fico que debe ser agarrado
    public Material targetMaterial; // Material al que se asignar�n las texturas
    public bool enableEmission = true; // Activar o desactivar el Emission Map

    private bool isCorrectObjectGrabbed = false; // Bandera para saber si se agarr� el objeto correcto

    void OnEnable()
    {
        if (primaryButtonAction != null)
        {
            // Registra el evento para el bot�n primario
            primaryButtonAction.action.performed += OnPrimaryButtonPressed;
        }

        if (interactor != null)
        {
            // Suscribirse a eventos de agarre
            interactor.selectEntered.AddListener(OnGrabStarted);
            interactor.selectExited.AddListener(OnGrabEnded);
        }
    }

    void OnDisable()
    {
        if (primaryButtonAction != null)
        {
            // Cancela el registro del evento
            primaryButtonAction.action.performed -= OnPrimaryButtonPressed;
        }

        if (interactor != null)
        {
            // Cancelar la suscripci�n a los eventos de agarre
            interactor.selectEntered.RemoveListener(OnGrabStarted);
            interactor.selectExited.RemoveListener(OnGrabEnded);
        }
    }

    private void OnGrabStarted(SelectEnterEventArgs args)
    {
        // Verifica si el objeto agarrado es el requerido
        if (args.interactableObject.transform.gameObject == requiredObject)
        {
            isCorrectObjectGrabbed = true;
            Debug.Log($"Correct object grabbed: {requiredObject.name}");
        }
        else
        {
            isCorrectObjectGrabbed = false;
            Debug.LogWarning("Incorrect object grabbed!");
        }
    }

    private void OnGrabEnded(SelectExitEventArgs args)
    {
        // Reinicia la bandera al soltar cualquier objeto
        isCorrectObjectGrabbed = false;
        Debug.Log("Object released!");
    }

    private void OnPrimaryButtonPressed(InputAction.CallbackContext context)
    {
        if (isCorrectObjectGrabbed)
        {
            CaptureAndAssignToMaterial();
        }
        else
        {
            Debug.LogWarning("You must grab the correct object to take a screenshot.");
        }
    }

    public void CaptureAndAssignToMaterial()
    {
        if (renderCamera == null)
        {
            Debug.LogError("Render camera is not assigned.");
            return;
        }

        if (targetMaterial == null)
        {
            Debug.LogError("Target material is not assigned.");
            return;
        }

        // Configura el tama�o de la captura
        int width = Screen.width;
        int height = Screen.height;

        // Crea una textura temporal para capturar la imagen
        Texture2D capturedTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Renderiza manualmente la c�mara
        RenderTexture tempRenderTexture = new RenderTexture(width, height, 24);
        renderCamera.targetTexture = tempRenderTexture;
        RenderTexture.active = tempRenderTexture;

        renderCamera.Render(); // Renderiza la escena con post-processing

        // Captura los p�xeles de la RenderTexture activa
        capturedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        capturedTexture.Apply();

        // Asigna la textura al Base Map del material
        targetMaterial.SetTexture("_BaseMap", capturedTexture);

        // Asigna la textura al Emission Map si est� habilitado
        if (enableEmission)
        {
            targetMaterial.EnableKeyword("_EMISSION");
            targetMaterial.SetTexture("_EmissionMap", capturedTexture);
        }
        else
        {
            targetMaterial.DisableKeyword("_EMISSION");
        }

        // Limpia las referencias para evitar problemas
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(tempRenderTexture);

        Debug.Log("Screenshot texture assigned to material with post-processing applied.");
    }
}