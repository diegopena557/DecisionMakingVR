using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class WorkZoneTrigger : MonoBehaviour
{
    [Header("Conveyor")]
    [SerializeField] private ConveyorController conveyorController;

    [Header("Deteccion")]
    [Tooltip("Activa para filtrar por tag. Desactiva para que cualquier collider dispare la zona.")]
    [SerializeField] private bool useTag = false;
    [Tooltip("Tag del collider que debe entrar (solo si useTag esta activo).")]
    [SerializeField] private string triggerTag = "Player";

    [Header("Particulas guia")]
    [SerializeField] private ParticleSystem guideParticles;
    [Tooltip("Segundos antes de apagar las particulas tras la entrada.")]
    [SerializeField] private float particlesFadeOutDelay = 1.5f;

    [Header("Evento opcional")]
    public UnityEvent onPlayerEntered;

    private bool triggered = false;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        if (guideParticles != null && !guideParticles.isPlaying)
            guideParticles.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        // Filtrado opcional por tag
        if (useTag && !other.CompareTag(triggerTag)) return;

        triggered = true;

        if (conveyorController != null)
            conveyorController.StartConveyor();

        if (guideParticles != null)
            StartCoroutine(StopParticlesDelayed());

        onPlayerEntered.Invoke();
    }

    private IEnumerator StopParticlesDelayed()
    {
        yield return new WaitForSeconds(particlesFadeOutDelay);
        if (guideParticles != null)
            guideParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 1f, 0.4f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
            Gizmos.DrawCube(box.center, box.size);
        else if (col is SphereCollider sphere)
            Gizmos.DrawSphere(sphere.center, sphere.radius);

        Gizmos.color = new Color(0f, 1f, 0.4f, 0.7f);
        if (col is BoxCollider box2)
            Gizmos.DrawWireCube(box2.center, box2.size);
    }
#endif
}