using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text instructionText;

    [TextArea(2, 4)]
    public string[] instructions;

    [Header("Señaleticas por paso")]
    public GameObject[] stepIndicators;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip stepCompleteClip;
    public AudioClip tutorialCompleteClip;
    [Range(0f, 1f)] public float stepVolume = 1f;
    [Range(0f, 1f)] public float finalVolume = 1f;

    private int currentStep = 0;
    public int CurrentStep => currentStep;

    void Start()
    {
        RefreshStep();
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

        for (int i = 0; i < stepIndicators.Length; i++)
        {
            if (stepIndicators[i] != null)
                stepIndicators[i].SetActive(i == currentStep);
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
}