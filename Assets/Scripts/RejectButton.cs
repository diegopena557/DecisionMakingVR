using UnityEngine;

// Coloca este componente en el boton ROJO (rechazar / reportar)
public class RejectButton : MonoBehaviour
{
    [SerializeField] private ConveyorController conveyorSystem;
    [SerializeField] private TutorialDecisionReporter tutorialReporter;

    private AudioSource audioButton;

    private void Awake()
    {
        audioButton = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand"))
            return;

        if (conveyorSystem != null)
            conveyorSystem.OnReport();

        if (tutorialReporter != null)
            tutorialReporter.ReportarAccionTutorial();

        if (audioButton != null)
            audioButton.Play();
    }
}