using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialZoneAdvance : MonoBehaviour
{
    public TutorialManager manager;
    public int expectedStep = 0;
    private bool used = false;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (used || manager == null)
            return;

        Camera cam = other.GetComponentInChildren<Camera>();
        bool isPlayer = cam != null || other.GetComponent<CharacterController>() != null;

        if (!isPlayer && other.GetComponentInParent<Camera>() == null && other.GetComponentInParent<CharacterController>() == null)
            return;

        manager.CompleteExpectedStep(expectedStep);
        used = true;
    }
}