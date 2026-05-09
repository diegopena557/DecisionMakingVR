using System.Collections;
using UnityEngine;

public class RandomAmbientSounds : MonoBehaviour
{
   
    [System.Serializable]
    public class SoundEntry
    {
        [Header("Clip")]
        public AudioClip clip;

        [Header("Intervalo aleatorio (segundos)")]
        [Tooltip("Tiempo mínimo de espera antes de volver a sonar")]
        public float minInterval = 2f;
        [Tooltip("Tiempo máximo de espera antes de volver a sonar")]
        public float maxInterval = 8f;

        [Header("Volumen aleatorio")]
        [Range(0f, 1f)] public float minVolume = 0.7f;
        [Range(0f, 1f)] public float maxVolume = 1.0f;

        [Header("Pitch aleatorio")]
        [Range(0.5f, 2f)] public float minPitch = 0.9f;
        [Range(0.5f, 2f)] public float maxPitch = 1.1f;

        [Header("Opciones")]
        [Tooltip("Reproducir este sonido al iniciar la escena (con retardo aleatorio)")]
        public bool playOnStart = true;

        [Tooltip("Retardo inicial antes del primer sonido (segundos)")]
        public float startDelay = 0f;

        
        [HideInInspector] public AudioSource source;
    }

   
    [Header("Lista de Sonidos (hasta 5)")]
    public SoundEntry[] sounds = new SoundEntry[5];

    [Header("Configuración global")]
    [Tooltip("Marcar para que todos los sonidos se pausen si el juego está pausado")]
    public bool respectTimescale = true;

   
    private void Awake()
    {
        // Crea una AudioSource por cada entrada para reproducción independiente
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i] == null) continue;

            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f; // 2D por defecto; cambia a 1f para 3D
            sounds[i].source = src;
        }
    }

    private void Start()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            SoundEntry entry = sounds[i];
            if (entry == null || entry.clip == null || entry.source == null) continue;

            if (entry.playOnStart)
            {
                float delay = entry.startDelay > 0f
                    ? entry.startDelay
                    : Random.Range(0f, entry.maxInterval); // retardo inicial aleatorio

                StartCoroutine(SoundLoop(entry, delay));
            }
        }
    }

   
    private IEnumerator SoundLoop(SoundEntry entry, float initialDelay)
    {
        // Espera inicial
        yield return Wait(initialDelay);

        while (true)
        {
            // Aplica parámetros aleatorios antes de reproducir
            entry.source.volume = Random.Range(entry.minVolume, entry.maxVolume);
            entry.source.pitch = Random.Range(entry.minPitch, entry.maxPitch);
            entry.source.clip = entry.clip;
            entry.source.Play();

            // Espera a que termine el clip + un intervalo aleatorio
            float clipDuration = entry.clip.length / Mathf.Abs(entry.source.pitch);
            float waitInterval = Random.Range(entry.minInterval, entry.maxInterval);

            yield return Wait(clipDuration + waitInterval);
        }
    }


    private IEnumerator Wait(float seconds)
    {
        if (respectTimescale)
            yield return new WaitForSeconds(seconds);
        else
            yield return new WaitForSecondsRealtime(seconds);
    }


    public void PauseAll()
    {
        foreach (var entry in sounds)
            if (entry?.source != null) entry.source.Pause();
    }


    public void ResumeAll()
    {
        foreach (var entry in sounds)
            if (entry?.source != null) entry.source.UnPause();
    }


    public void StopAll()
    {
        StopAllCoroutines();
        foreach (var entry in sounds)
            if (entry?.source != null) entry.source.Stop();
    }


    public void SetSoundActive(int index, bool active)
    {
        if (index < 0 || index >= sounds.Length) return;
        if (sounds[index]?.source == null) return;

        if (!active)
            sounds[index].source.Stop();
        else
            StartCoroutine(SoundLoop(sounds[index], 0f));
    }
}