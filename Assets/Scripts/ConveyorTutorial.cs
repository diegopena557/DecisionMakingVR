using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ConveyorTutorial : MonoBehaviour
{
    public Transform trayStartPoint;
    public Transform trayEndPoint;
    public float moveSpeed = 1.2f;

    [Header("Ajuste de bandeja en la cinta")]
    public Vector3 trayPositionOffset = Vector3.zero;
    public Vector3 trayRotationOffset = Vector3.zero;

    private GameObject currentTray;
    private bool isMoving = false;
    private Quaternion snappedRotation;

    public void PlaceTray(GameObject tray)
    {
        if (tray == null) return;

        currentTray = tray;

        snappedRotation = trayStartPoint.rotation * Quaternion.Euler(trayRotationOffset);

        currentTray.transform.position = trayStartPoint.position + trayPositionOffset;
        currentTray.transform.rotation = snappedRotation;

        Rigidbody rb = currentTray.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        XRGrabInteractable grab = currentTray.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.enabled = false;
        }
    }

    public void SendTray()
    {
        if (currentTray == null || isMoving) return;
        StartCoroutine(MoveTray());
    }

    private IEnumerator MoveTray()
    {
        isMoving = true;

        Vector3 endPos = trayEndPoint.position + trayPositionOffset;

        while (currentTray != null &&
               Vector3.Distance(currentTray.transform.position, endPos) > 0.02f)
        {
            currentTray.transform.position = Vector3.MoveTowards(
                currentTray.transform.position,
                endPos,
                moveSpeed * Time.deltaTime
            );

            currentTray.transform.rotation = snappedRotation;

            yield return null;
        }

        isMoving = false;
    }
}