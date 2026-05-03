using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HandInputAnimator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private NearFarInteractor interactor;

    [Header("Input opcional (para headset real)")]
    [SerializeField] private InputActionReference gripAction;
    [SerializeField] private InputActionReference triggerAction;

    [Header("Modo de prueba en editor")]
    [SerializeField] private bool useSelectionOnlyForGrip = true;

    [Header("Parámetros del Animator")]
    [SerializeField] private string gripParameter = "Grip";
    [SerializeField] private string triggerParameter = "Trigger";

    [Header("Suavizado")]
    [SerializeField] private float speed = 12f;

    private float currentGrip;
    private float currentTrigger;

    void Reset()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (gripAction != null && gripAction.action != null && !gripAction.action.enabled)
            gripAction.action.Enable();

        if (triggerAction != null && triggerAction.action != null && !triggerAction.action.enabled)
            triggerAction.action.Enable();
    }

    void Update()
    {
        if (animator == null)
            return;

        float targetGrip = 0f;
        float targetTrigger = 0f;

        // Trigger sí puede seguir viniendo del input
        if (triggerAction != null && triggerAction.action != null)
            targetTrigger = triggerAction.action.ReadValue<float>();

        // Grip: en editor/simulator, usar solo si esta mano realmente está agarrando algo
        if (useSelectionOnlyForGrip)
        {
            targetGrip = (interactor != null && interactor.hasSelection) ? 1f : 0f;
        }
        else
        {
            if (gripAction != null && gripAction.action != null)
                targetGrip = gripAction.action.ReadValue<float>();
        }

        currentGrip = Mathf.MoveTowards(currentGrip, targetGrip, speed * Time.deltaTime);
        currentTrigger = Mathf.MoveTowards(currentTrigger, targetTrigger, speed * Time.deltaTime);

        animator.SetFloat(gripParameter, currentGrip);
        animator.SetFloat(triggerParameter, currentTrigger);
    }
}