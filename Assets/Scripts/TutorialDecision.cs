using UnityEngine;

public class TutorialDecisionGate : MonoBehaviour
{
    [SerializeField] private NarrativeTutorialManager tutorialManager;
    [SerializeField] private int actionId;

    public bool TryActivate()
    {
        if (tutorialManager == null)
            return true;

        return tutorialManager.TryHandleActionAttempt(actionId);
    }
}