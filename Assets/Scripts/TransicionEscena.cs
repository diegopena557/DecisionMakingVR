using UnityEngine;
using UnityEngine.SceneManagement;

public class TransicionEscena : MonoBehaviour
{
    // ── Nombres de escena fijos ──────────────────────────────────────────────
    private const string ESCENA_MENU = "MenúManos";
    private const string ESCENA_TUTORIAL = "TutorialBotones";
    private const string ESCENA_ENCUESTA = "EncuestaVAS";

    private static readonly string[] ESCENAS_EXPERIENCIA = new string[]
    {
        "AirportsceneHappy",
        "AirportsceneNeutral",
        "AirportsceneSad",
        "AirportsceneStressed"
    };

    // ── Selección aleatoria ──────────────────────────────────────────────────
    private static string ObtenerEscenaExperienciaAleatoria()
    {
        int index = Random.Range(0, ESCENAS_EXPERIENCIA.Length);
        string escena = ESCENAS_EXPERIENCIA[index];
        Debug.Log($"[TransicionEscena] Escena de experiencia seleccionada: {escena}");
        return escena;
    }

    // ── Métodos de navegación ────────────────────────────────────────────────
    public void IrAMenu()
    {
        CargarEscena(ESCENA_MENU);
    }

    public void IrATutorial()
    {
        CargarEscena(ESCENA_TUTORIAL);
    }

    // Menú -> Encuesta previa -> Experiencia
    public void IrAEncuestaAntesDeExperiencia()
    {
        string escenaExperiencia = ObtenerEscenaExperienciaAleatoria();
        EncuestaVASFlow.PrepararAntesExperiencia(escenaExperiencia);
        CargarEscena(ESCENA_ENCUESTA);
    }

    // Tutorial -> Encuesta previa -> Experiencia
    public void IrAEncuestaAntesDeExperienciaDesdeTutorial()
    {
        string escenaExperiencia = ObtenerEscenaExperienciaAleatoria();
        EncuestaVASFlow.PrepararAntesExperiencia(escenaExperiencia);
        CargarEscena(ESCENA_ENCUESTA);
    }

    // Experiencia -> Encuesta final -> Menú
    public void IrAEncuestaDespuesDeExperiencia()
    {
        EncuestaVASFlow.PrepararDespuesExperiencia(ESCENA_MENU);
        CargarEscena(ESCENA_ENCUESTA);
    }

    // Ir directo a experiencia sin encuesta previa (también aleatoria)
    public void IrAExperienciaDirecta()
    {
        CargarEscena(ObtenerEscenaExperienciaAleatoria());
    }

    public void IrAEscena(string nombreEscena)
    {
        CargarEscena(nombreEscena);
    }

    public void ReiniciarEscenaActual()
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.ReloadCurrentScene();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ── Carga interna ────────────────────────────────────────────────────────
    private void CargarEscena(string nombreEscena)
    {
        if (string.IsNullOrEmpty(nombreEscena))
        {
            Debug.LogWarning("TransicionEscena: nombre de escena vacío.");
            return;
        }

        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade(nombreEscena);
        else
            SceneManager.LoadScene(nombreEscena);
    }
}