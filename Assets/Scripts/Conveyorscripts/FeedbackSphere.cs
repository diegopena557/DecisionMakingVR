using System.Collections;
using UnityEngine;

public class FeedbackSphere : MonoBehaviour
{
    [Header("Materiales")]
    [SerializeField] private Material materialNeutro;
    [SerializeField] private Material materialCorrecto;
    [SerializeField] private Material materialIncorrecto;

    [Header("Tiempo antes de volver al neutro")]
    [SerializeField] private float duracionFeedback = 2f;

    private Renderer sphereRenderer;
    private Coroutine resetCoroutine;

    private void Awake()
    {
        sphereRenderer = GetComponent<Renderer>();

        if (sphereRenderer == null)
            Debug.LogError("[FeedbackSphere] No se encontro Renderer en la esfera.", this);
    }

    private void Start()
    {
        ApplyMaterial(materialNeutro);
    }

    // --- API publica: conectar a los UnityEvents del ConveyorController ---

    public void ShowCorrect()
    {
        ShowFeedback(materialCorrecto);
    }

    public void ShowWrong()
    {
        ShowFeedback(materialIncorrecto);
    }

    // --- Logica interna ---

    private void ShowFeedback(Material mat)
    {
        // Cancelar cualquier reset pendiente para no pisar este nuevo feedback
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        ApplyMaterial(mat);
        resetCoroutine = StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(duracionFeedback);
        ApplyMaterial(materialNeutro);
        resetCoroutine = null;
    }

    private void ApplyMaterial(Material mat)
    {
        if (mat == null || sphereRenderer == null) return;
        sphereRenderer.material = mat;
    }
}