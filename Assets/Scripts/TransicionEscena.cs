using UnityEngine;

public class TransicionEscena : MonoBehaviour
{
    [SerializeField] private string escenaTutorial;
    [SerializeField] private string escenaExperiencia;

    public void IrATutorial()
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade(escenaTutorial);
    }

    public void IrAExperiencia()
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade(escenaExperiencia);
    }

    public void IrAEscena(string nombreEscena)
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade(nombreEscena);
    }

    public void ReiniciarEscenaActual()
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.ReloadCurrentScene();
    }
}