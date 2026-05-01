using UnityEngine;

// Coloca este componente en el boton ROJO (rechazar / reportar)
public class RejectButton : MonoBehaviour
{
    [SerializeField] private ConveyorController conveyorSystem;
    AudioSource audioButton;

    private void Awake()
    {
        audioButton = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            conveyorSystem.OnReport();
            if (audioButton != null)
                audioButton.Play();
        }
    }
}