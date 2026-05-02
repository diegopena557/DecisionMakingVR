using UnityEngine;

public class TutorialActionTrigger : MonoBehaviour
{
    [SerializeField] private NarrativeTutorialManager tutorialManager;
    [SerializeField] private int actionId = 1;
    [SerializeField] private bool triggerOnce = true;

    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (tutorialManager == null)
            return;

        tutorialManager.CompleteExpectedStep(actionId);

        if (triggerOnce)
            alreadyTriggered = true;
    }
}
