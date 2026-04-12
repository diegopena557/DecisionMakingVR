using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public TMP_Text instructionText;

    [TextArea(2, 4)]
    public string[] instructions;

    private int currentStep = 0;
    public int CurrentStep => currentStep;

    void Start()
    {
        UpdateInstruction();
    }

    void UpdateInstruction()
    {
        if (instructionText != null && currentStep < instructions.Length)
            instructionText.text = instructions[currentStep];
        else if (instructionText != null)
            instructionText.text = "Tutorial completado.";
    }

    public void CompleteExpectedStep(int expectedStep)
    {
        if (currentStep != expectedStep)
            return;

        currentStep++;
        UpdateInstruction();
    }
}