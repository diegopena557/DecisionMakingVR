using UnityEngine;

public class TutorialDecisionReporter : MonoBehaviour
{
    [SerializeField] private NarrativeTutorialManager tutorialManager;
    [SerializeField] private int actionId;

    public void ReportarAccionTutorial()
    {
        if (tutorialManager == null)
            return;

        tutorialManager.CompleteExpectedStep(actionId);
    }
}