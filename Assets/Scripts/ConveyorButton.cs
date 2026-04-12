using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ConveyorButton : MonoBehaviour
{
    [SerializeField] private ConveyorController conveyorSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
            conveyorSystem.OnContinue();
    }
}
