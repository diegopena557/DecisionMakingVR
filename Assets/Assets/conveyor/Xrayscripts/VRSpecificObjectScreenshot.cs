using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;


[ExecuteAlways]
public class VRSpecificObjectScreenshot : MonoBehaviour
{
    public Camera renderCamera;
    public InputActionReference primaryButtonActionRight; // Bot�n primario del controlador derecho
    public InputActionReference primaryButtonActionLeft;  // Bot�n primario del controlador izquierdo
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor rightHandInteractor;        // Interactor de la mano derecha
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor leftHandInteractor;         // Interactor de la mano izquierda
    public GameObject requiredObject;
    public Material targetMaterial; // Material principal
    public Material secondaryMaterial; // Material del segundo objeto
    public bool enableEmission = true;

    [Header("Resolution Settings")]
    public int captureWidth = 1440;
    public int captureHeight = 1080;

    private bool isCorrectObjectGrabbed = false;
    private bool isRightHandHolding = false; // Indica si la mano derecha sostiene el objeto
    private string lastSavedPath = ""; // Ruta de la �ltima imagen guardada
    private string previousSavedPath = ""; // Ruta de la imagen anterior

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip clic;

    void OnEnable()
    {
        // Vincula los eventos para las acciones
        if (primaryButtonActionRight != null)
        {
            primaryButtonActionRight.action.performed += OnPrimaryButtonPressed;
        }
        if (primaryButtonActionLeft != null)
        {
            primaryButtonActionLeft.action.performed += OnPrimaryButtonPressed;
        }

        if (rightHandInteractor != null)
        {
            rightHandInteractor.selectEntered.AddListener(OnGrabStartedRightHand);
            rightHandInteractor.selectExited.AddListener(OnGrabEnded);
        }

        if (leftHandInteractor != null)
        {
            leftHandInteractor.selectEntered.AddListener(OnGrabStartedLeftHand);
            leftHandInteractor.selectExited.AddListener(OnGrabEnded);
        }
    }

    void OnDisable()
    {
        // Desvincula los eventos de las acciones
        if (primaryButtonActionRight != null)
        {
            primaryButtonActionRight.action.performed -= OnPrimaryButtonPressed;
        }
        if (primaryButtonActionLeft != null)
        {
            primaryButtonActionLeft.action.performed -= OnPrimaryButtonPressed;
        }

        if (rightHandInteractor != null)
        {
            rightHandInteractor.selectEntered.RemoveListener(OnGrabStartedRightHand);
            rightHandInteractor.selectExited.RemoveListener(OnGrabEnded);
        }

        if (leftHandInteractor != null)
        {
            leftHandInteractor.selectEntered.RemoveListener(OnGrabStartedLeftHand);
            leftHandInteractor.selectExited.RemoveListener(OnGrabEnded);
        }
    }

    private void OnGrabStartedRightHand(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.gameObject == requiredObject)
        {
            isCorrectObjectGrabbed = true;
            isRightHandHolding = true;
            Debug.Log($"Correct object grabbed with Right Hand: {requiredObject.name}");
        }
    }

    private void OnGrabStartedLeftHand(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.gameObject == requiredObject)
        {
            isCorrectObjectGrabbed = true;
            isRightHandHolding = false;
            Debug.Log($"Correct object grabbed with Left Hand: {requiredObject.name}");
        }
    }

    private void OnGrabEnded(SelectExitEventArgs args)
    {
        isCorrectObjectGrabbed = false;
        Debug.Log("Object released!");
    }

    private void OnPrimaryButtonPressed(InputAction.CallbackContext context)
    {
        if (!isCorrectObjectGrabbed)
        {
            Debug.LogWarning("You must grab the correct object to take a screenshot.");
            return;
        }

        // Verifica cu�l bot�n fue presionado seg�n la mano que sostiene el objeto
        if ((isRightHandHolding && context.action == primaryButtonActionRight.action) ||
            (!isRightHandHolding && context.action == primaryButtonActionLeft.action))
        {
            CaptureAndSaveScreenshot();
            audioSource.PlayOneShot(clic);
            Debug.Log($"Screenshot taken with {(isRightHandHolding ? "Right controller" : "Left controller")}.");
        }
    }

    public void CaptureAndSaveScreenshot()
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

        // Usar resoluci�n personalizada
        int width = captureWidth;
        int height = captureHeight;

        // Crea una nueva textura temporal para la captura
        Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Renderiza manualmente la c�mara
        RenderTexture tempRenderTexture = new RenderTexture(width, height, 24);
        renderCamera.targetTexture = tempRenderTexture;
        RenderTexture.active = tempRenderTexture;

        renderCamera.Render();

        // Captura los p�xeles de la Render Texture activa
        screenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshotTexture.Apply();

        // Genera la ruta del archivo
        string fileName = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        string folderPath = Path.Combine(Application.persistentDataPath, "Screenshots");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, fileName);

        // Guarda la textura como archivo PNG
        byte[] bytes = screenshotTexture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Screenshot saved to: {filePath}");

        // Actualiza las rutas de las im�genes
        previousSavedPath = lastSavedPath;
        lastSavedPath = filePath;

        // Asigna la textura al material principal
        AssignTextureToMaterials(screenshotTexture);

        // Limpia las referencias para evitar problemas
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(tempRenderTexture);
    }

    private void AssignTextureToMaterials(Texture2D newTexture)
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetTexture("_BaseMap", newTexture);

            if (enableEmission)
            {
                targetMaterial.EnableKeyword("_EMISSION");
                targetMaterial.SetTexture("_EmissionMap", newTexture);
            }
            else
            {
                targetMaterial.DisableKeyword("_EMISSION");
            }
        }

        if (secondaryMaterial != null && !string.IsNullOrEmpty(previousSavedPath) && File.Exists(previousSavedPath))
        {
            byte[] previousImageBytes = File.ReadAllBytes(previousSavedPath);
            Texture2D previousTexture = new Texture2D(2, 2);
            previousTexture.LoadImage(previousImageBytes);

            secondaryMaterial.SetTexture("_BaseMap", previousTexture);

            if (enableEmission)
            {
                secondaryMaterial.EnableKeyword("_EMISSION");
                secondaryMaterial.SetTexture("_EmissionMap", previousTexture);
            }
            else
            {
                secondaryMaterial.DisableKeyword("_EMISSION");
            }
        }
    }
}
