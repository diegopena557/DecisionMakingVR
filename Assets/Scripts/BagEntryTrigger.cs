using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BagEntryTrigger : MonoBehaviour
{
    public TutorialManager manager;
    public int expectedStep = 0;
    public GameObject targetBag;
    public BagConveyorTutorial conveyor;
    public bool onlyOnce = true;

    private bool used = false;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used || targetBag == null || conveyor == null)
            return;

        if (other.gameObject != targetBag && other.transform.root.gameObject != targetBag)
            return;

        conveyor.PlaceBag(targetBag);

        if (manager != null)
            manager.CompleteExpectedStep(expectedStep);

        if (onlyOnce)
            used = true;
    }
}