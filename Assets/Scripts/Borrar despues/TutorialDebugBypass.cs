using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialDebugBypass : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NarrativeTutorialManager tutorialManager;
    [SerializeField] private ConveyorController conveyorController;

    [Header("IDs del tutorial")]
    [SerializeField] private int actionIdLlegarBanda = 1;
    [SerializeField] private int actionIdBotonVerde = 2;
    [SerializeField] private int actionIdBotonRojo = 3;

    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (tutorialManager != null)
                tutorialManager.CompleteExpectedStep(actionIdLlegarBanda);
        }

        if (Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            if (conveyorController != null)
                conveyorController.OnContinue();

            if (tutorialManager != null)
                tutorialManager.CompleteExpectedStep(actionIdBotonVerde);
        }

        if (Keyboard.current != null && Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            if (conveyorController != null)
                conveyorController.OnReport();

            if (tutorialManager != null)
                tutorialManager.CompleteExpectedStep(actionIdBotonRojo);
        }
#endif
    }
}