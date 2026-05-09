using UnityEngine;
using UnityEngine.SceneManagement;

public class TransicionEscena : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string escenaMenu;
    [SerializeField] private string escenaTutorial;
    [SerializeField] private string escenaEncuesta;
    [SerializeField] private string escenaExperiencia;

    public void IrAMenu()
    {
        CargarEscena(escenaMenu);
    }

    public void IrATutorial()
    {
        CargarEscena(escenaTutorial);
    }

    // Menú -> Encuesta previa -> Experiencia
    public void IrAEncuestaAntesDeExperiencia()
    {
        EncuestaVASFlow.PrepararAntesExperiencia(escenaExperiencia);
        CargarEscena(escenaEncuesta);
    }

    // Tutorial -> Encuesta previa -> Experiencia
    public void IrAEncuestaAntesDeExperienciaDesdeTutorial()
    {
        EncuestaVASFlow.PrepararAntesExperiencia(escenaExperiencia);
        CargarEscena(escenaEncuesta);
    }

    // Experiencia -> Encuesta final -> Menú
    public void IrAEncuestaDespuesDeExperiencia()
    {
        EncuestaVASFlow.PrepararDespuesExperiencia(escenaMenu);
        CargarEscena(escenaEncuesta);
    }

    public void IrAExperienciaDirecta()
    {
        CargarEscena(escenaExperiencia);
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