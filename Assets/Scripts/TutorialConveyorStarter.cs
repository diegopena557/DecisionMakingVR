using UnityEngine;

public class TutorialConveyorStarter : MonoBehaviour
{
    [SerializeField] private ConveyorController conveyorController;

    private bool yaInicio = false;

    public void IniciarBandaTutorial()
    {
        if (yaInicio)
            return;

        if (conveyorController == null)
            return;

        conveyorController.enabled = true;
        yaInicio = true;
    }
}