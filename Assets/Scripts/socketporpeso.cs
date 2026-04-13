using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketStabilizer : MonoBehaviour
{
    public XRSocketInteractor socket;
    public Transform trayRoot;

    private Rigidbody currentRb;
    private Collider[] currentObjectColliders;
    private Collider[] trayColliders;

    private bool previousUseGravity;
    private bool previousIsKinematic;

    void Awake()
    {
        if (socket == null)
            socket = GetComponent<XRSocketInteractor>();

        if (trayRoot != null)
            trayColliders = trayRoot.GetComponentsInChildren<Collider>(true);
    }

    void OnEnable()
    {
        if (socket == null) return;

        socket.selectEntered.AddListener(OnSocketEnter);
        socket.selectExited.AddListener(OnSocketExit);
    }

    void OnDisable()
    {
        if (socket == null) return;

        socket.selectEntered.RemoveListener(OnSocketEnter);
        socket.selectExited.RemoveListener(OnSocketExit);
    }

    private void OnSocketEnter(SelectEnterEventArgs args)
    {
        GameObject obj = args.interactableObject.transform.gameObject;

        currentRb = obj.GetComponent<Rigidbody>();
        currentObjectColliders = obj.GetComponentsInChildren<Collider>(true);

        if (currentRb != null)
        {
            previousUseGravity = currentRb.useGravity;
            previousIsKinematic = currentRb.isKinematic;

            currentRb.linearVelocity = Vector3.zero;
            currentRb.angularVelocity = Vector3.zero;
            currentRb.useGravity = false;
            currentRb.isKinematic = true;
        }

        IgnoreTrayCollisions(true);
    }

    private void OnSocketExit(SelectExitEventArgs args)
    {
        if (currentRb != null)
        {
            currentRb.useGravity = previousUseGravity;
            currentRb.isKinematic = previousIsKinematic;
        }

        IgnoreTrayCollisions(false);

        currentRb = null;
        currentObjectColliders = null;
    }

    private void IgnoreTrayCollisions(bool ignore)
    {
        if (trayColliders == null || currentObjectColliders == null)
            return;

        foreach (var trayCol in trayColliders)
        {
            foreach (var objCol in currentObjectColliders)
            {
                if (trayCol == null || objCol == null) continue;
                if (trayCol == objCol) continue;

                Physics.IgnoreCollision(trayCol, objCol, ignore);
            }
        }
    }
}