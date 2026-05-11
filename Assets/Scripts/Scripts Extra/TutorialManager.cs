using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text instructionText;

    [TextArea(2, 4)]
    public string[] instructions;

    [Header("Señaleticas visuales por paso")]
    public GameObject[] stepIndicators;

    [Header("Señaleticas de luz por paso")]
    public GameObject[] stepLightIndicators;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip stepCompleteClip;
    public AudioClip tutorialCompleteClip;
    [Range(0f, 1f)] public float stepVolume = 1f;
    [Range(0f, 1f)] public float finalVolume = 1f;

    [Header("Canvas fijo que solo rota hacia el usuario")]
    public Transform floatingUIRoot;
    public Transform followTarget;
    public float rotateSmooth = 10f;
    public bool yawOnly = true;
    public Vector3 rotationOffset = Vector3.zero;

    private int currentStep = 0;
    public int CurrentStep => currentStep;

    void Start()
    {
        AutoAssignReferences();
        RefreshStep();
        SnapUIRotationToTarget();
    }

    void LateUpdate()
    {
        RotateFloatingUIOnly();
    }

    void AutoAssignReferences()
    {
        if (followTarget == null && Camera.main != null)
            followTarget = Camera.main.transform;

        if (floatingUIRoot == null && instructionText != null)
        {
            Canvas parentCanvas = instructionText.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                floatingUIRoot = parentCanvas.transform;
            else
                floatingUIRoot = instructionText.transform;
        }
    }

    void RefreshStep()
    {
        if (instructionText != null)
        {
            if (currentStep < instructions.Length)
                instructionText.text = instructions[currentStep];
            else
                instructionText.text = "Tutorial completado.";
        }

        RefreshVisualIndicators();
        RefreshLightIndicators();
    }

    void RefreshVisualIndicators()
    {
        for (int i = 0; i < stepIndicators.Length; i++)
        {
            if (stepIndicators[i] != null)
                stepIndicators[i].SetActive(i == currentStep);
        }
    }

    void RefreshLightIndicators()
    {
        for (int i = 0; i < stepLightIndicators.Length; i++)
        {
            if (stepLightIndicators[i] != null)
                stepLightIndicators[i].SetActive(i == currentStep);
        }
    }

    public void CompleteExpectedStep(int expectedStep)
    {
        if (currentStep != expectedStep)
            return;

        currentStep++;

        PlayStepSound();
        RefreshStep();
    }

    void PlayStepSound()
    {
        if (audioSource == null)
            return;

        bool reachedFinalState = instructions != null &&
                                 instructions.Length > 0 &&
                                 currentStep >= instructions.Length - 1;

        if (reachedFinalState)
        {
            if (tutorialCompleteClip != null)
                audioSource.PlayOneShot(tutorialCompleteClip, finalVolume);
        }
        else
        {
            if (stepCompleteClip != null)
                audioSource.PlayOneShot(stepCompleteClip, stepVolume);
        }
    }

    void RotateFloatingUIOnly()
    {
        if (floatingUIRoot == null || followTarget == null)
            return;

        Vector3 lookDir = followTarget.position - floatingUIRoot.position;

        if (yawOnly)
            lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f)
            return;

        Quaternion desiredRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        desiredRotation *= Quaternion.Euler(rotationOffset);

        float rotT = 1f - Mathf.Exp(-rotateSmooth * Time.deltaTime);
        floatingUIRoot.rotation = Quaternion.Slerp(floatingUIRoot.rotation, desiredRotation, rotT);
    }

    void SnapUIRotationToTarget()
    {
        if (floatingUIRoot == null || followTarget == null)
            return;

        Vector3 lookDir = followTarget.position - floatingUIRoot.position;

        if (yawOnly)
            lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f)
            return;

        Quaternion snapRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        floatingUIRoot.rotation = snapRotation * Quaternion.Euler(rotationOffset);
    }

    public void IrAEscena(string nombreEscena)
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade(nombreEscena);
    }
}