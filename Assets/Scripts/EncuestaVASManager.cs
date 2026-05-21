using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncuestaVASManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text preguntaText;
    [SerializeField] private TMP_Text estadoText;
    [SerializeField] private TMP_Text subtituloModoText;

    [Header("Textos")]
    [TextArea(2, 4)]
    [SerializeField] private string pregunta = "¿Cómo te sientes ahora?";
    [SerializeField] private string estadoInicial = "Selecciona la carita que mejor represente cómo te sientes ahora.";
    [SerializeField] private string formatoEstadoSeleccion = "Respuesta registrada: {0}/10";
    [SerializeField] private string textoModoAntes = "Encuesta previa a la experiencia";
    [SerializeField] private string textoModoDespues = "Encuesta posterior a la experiencia";

    // ── Escenas de experiencia ───────────────────────────────────────────────
    private static readonly string[] ESCENAS_EXPERIENCIA = new string[]
    {
        "AirportsceneHappy",
        "AirportsceneNeutral",
        "AirportsceneSad",
        "AirportsceneStressed"
    };

    private static string ObtenerEscenaExperienciaAleatoria()
    {
        int index = UnityEngine.Random.Range(0, ESCENAS_EXPERIENCIA.Length);
        string escena = ESCENAS_EXPERIENCIA[index];
        Debug.Log($"[EncuestaVASManager] Escena de experiencia seleccionada: {escena}");
        return escena;
    }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clipSeleccion;
    [SerializeField] private float retrasoAntesDeCambiarEscena = 1f;

    [Header("Halos")]
    [SerializeField] private GameObject[] todosLosGlows;

    private bool respuestaEnviada = false;
    private int valorSeleccionado = -1;

    public bool RespuestaEnviada => respuestaEnviada;

    private void Start()
    {
        if (preguntaText != null)
            preguntaText.text = pregunta;

        if (estadoText != null)
            estadoText.text = estadoInicial;

        if (subtituloModoText != null)
        {
            if (EncuestaVASFlow.Modo == EncuestaVASModo.AntesExperiencia)
                subtituloModoText.text = textoModoAntes;
            else if (EncuestaVASFlow.Modo == EncuestaVASModo.DespuesExperiencia)
                subtituloModoText.text = textoModoDespues;
            else
                subtituloModoText.text = "";
        }

        ApagarTodosLosGlows();
    }

    public void RegistrarRespuesta(int score, GameObject glowSeleccionado)
    {
        if (respuestaEnviada)
            return;

        respuestaEnviada = true;
        valorSeleccionado = score;

        ApagarTodosLosGlows();

        if (glowSeleccionado != null)
            glowSeleccionado.SetActive(true);

        if (estadoText != null)
            estadoText.text = string.Format(formatoEstadoSeleccion, valorSeleccionado);

        GuardarRespuesta();

        if (audioSource != null && clipSeleccion != null)
            audioSource.PlayOneShot(clipSeleccion);

        StartCoroutine(FinalizarEncuestaRutina());
    }

    private void ApagarTodosLosGlows()
    {
        if (todosLosGlows == null)
            return;

        for (int i = 0; i < todosLosGlows.Length; i++)
        {
            if (todosLosGlows[i] != null)
                todosLosGlows[i].SetActive(false);
        }
    }

    private IEnumerator FinalizarEncuestaRutina()
    {
        yield return new WaitForSeconds(retrasoAntesDeCambiarEscena);

        string siguienteEscena = ObtenerSiguienteEscena();
        EncuestaVASFlow.Limpiar();

        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade(siguienteEscena);
        else
            SceneManager.LoadScene(siguienteEscena);
    }

    private string ObtenerSiguienteEscena()
    {
        if (!string.IsNullOrEmpty(EncuestaVASFlow.SiguienteEscena))
            return EncuestaVASFlow.SiguienteEscena;

        return ObtenerEscenaExperienciaAleatoria();
    }

    private void GuardarRespuesta()
    {
        string prefijo = "VAS_General";

        if (EncuestaVASFlow.Modo == EncuestaVASModo.AntesExperiencia)
            prefijo = "VAS_Pre";
        else if (EncuestaVASFlow.Modo == EncuestaVASModo.DespuesExperiencia)
            prefijo = "VAS_Post";

        PlayerPrefs.SetInt(prefijo + "_Score", valorSeleccionado);
        PlayerPrefs.SetString(prefijo + "_TimestampUtc", DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();
    }
}