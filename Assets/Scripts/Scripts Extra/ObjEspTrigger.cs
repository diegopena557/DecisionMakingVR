using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Collider))]
public class TrayDeliveryToConveyor : MonoBehaviour
{
    public TutorialManager manager;
    public int expectedStep = 3;
    public GameObject targetTray;
    public ConveyorTutorial conveyor;
    public bool onlyOnce = true;

    private bool used = false;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used || manager == null || targetTray == null || conveyor == null)
            return;

        if (other.gameObject != targetTray && other.transform.root.gameObject != targetTray)
            return;

        conveyor.PlaceTray(targetTray);
        manager.CompleteExpectedStep(expectedStep);

        if (onlyOnce)
            used = true;
    }
}