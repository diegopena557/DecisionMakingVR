using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Coloca este componente en el boton VERDE (dejar pasar)
public class ConveyorButton : MonoBehaviour
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
            conveyorSystem.OnContinue();

        if (tutorialReporter != null)
            tutorialReporter.ReportarAccionTutorial();

        if (audioButton != null)
            audioButton.Play();
    }
}