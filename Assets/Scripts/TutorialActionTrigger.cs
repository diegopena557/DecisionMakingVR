using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialActionTrigger : MonoBehaviour
{
    [SerializeField] private NarrativeTutorialManager tutorialManager;
    [SerializeField] private int actionId = 1;
    [SerializeField] private bool triggerOnce = true;

    [Header("Detección por zona")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private bool useMainCameraIfTargetMissing = true;
    [SerializeField] private float verticalOffset = 0f;
    [SerializeField] private bool onlyWhenExpectedStepIsActive = true;

    private BoxCollider boxCollider;
    private bool alreadyTriggered = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        if (targetTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                targetTransform = playerObj.transform;
        }

        if (targetTransform == null && useMainCameraIfTargetMissing && Camera.main != null)
            targetTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (alreadyTriggered && triggerOnce)
            return;

        if (tutorialManager == null || targetTransform == null || boxCollider == null)
            return;

        if (onlyWhenExpectedStepIsActive)
        {
            if (!tutorialManager.IsWaitingForAction())
                return;

            if (tutorialManager.GetExpectedActionId() != actionId)
                return;
        }

        Vector3 probePosition = targetTransform.position + Vector3.up * verticalOffset;

        if (boxCollider.bounds.Contains(probePosition))
        {
            tutorialManager.CompleteExpectedStep(actionId);

            if (triggerOnce)
                alreadyTriggered = true;
        }
    }
}