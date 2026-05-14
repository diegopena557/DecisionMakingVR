using UnityEngine;

// Coloca este componente en el botón ROJO (rechazar / reportar)
public class RejectButton : MonoBehaviour
{
    [SerializeField] private ConveyorController conveyorSystem;
    [SerializeField] private TutorialDecisionGate tutorialDecisionGate;

    private AudioSource audioButton;

    private void Awake()
    {
        audioButton = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand"))
            return;

        if (tutorialDecisionGate != null && !tutorialDecisionGate.TryActivate())
            return;

        if (conveyorSystem != null)
            conveyorSystem.OnReport();

        if (audioButton != null)
            audioButton.Play();
    }
}