using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BagConveyorTutorial : MonoBehaviour
{
    [Header("Puntos de la cinta")]
    public Transform bagStartPoint;
    public Transform bagScanPoint;

    [Header("Puntos para después (todavía no usados)")]
    public Transform approvedEndPoint;
    public Transform rejectedPoint;

    [Header("Movimiento")]
    public float moveSpeed = 1.2f;
    public float autoSendDelay = 0.5f;

    [Header("Ajuste de la maleta en la cinta")]
    public Vector3 bagPositionOffset = Vector3.zero;
    public Vector3 bagRotationOffset = Vector3.zero;

    [Header("Eventos opcionales")]
    public UnityEvent onBagPlacedOnBelt;
    public UnityEvent onBagReachedScan;
    public UnityEvent onBagReachedApprovedEnd;
    public UnityEvent onBagReachedRejectedPoint;

    private GameObject currentBag;
    private bool isMoving = false;
    private Quaternion snappedRotation;
    private Coroutine autoSendCoroutine;

    public GameObject CurrentBag => currentBag;
    public bool HasBag => currentBag != null;

    public void PlaceBag(GameObject bag)
    {
        if (bag == null)
            return;

        currentBag = bag;

        snappedRotation = bagStartPoint.rotation * Quaternion.Euler(bagRotationOffset);

        currentBag.transform.position = bagStartPoint.position + bagPositionOffset;
        currentBag.transform.rotation = snappedRotation;

        Rigidbody rb = currentBag.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        XRGrabInteractable grab = currentBag.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.enabled = false;

        onBagPlacedOnBelt?.Invoke();

        if (autoSendCoroutine != null)
            StopCoroutine(autoSendCoroutine);

        autoSendCoroutine = StartCoroutine(AutoMoveToScanAfterDelay());
    }

    private IEnumerator AutoMoveToScanAfterDelay()
    {
        yield return new WaitForSeconds(autoSendDelay);
        yield return MoveToPoint(bagScanPoint);
        onBagReachedScan?.Invoke();
    }

    public void ContinueApproved()
    {
        if (currentBag == null || isMoving || approvedEndPoint == null)
            return;

        StartCoroutine(MoveApproved());
    }

    public void ContinueRejected()
    {
        if (currentBag == null || isMoving || rejectedPoint == null)
            return;

        StartCoroutine(MoveRejected());
    }

    private IEnumerator MoveApproved()
    {
        yield return MoveToPoint(approvedEndPoint);
        onBagReachedApprovedEnd?.Invoke();
    }

    private IEnumerator MoveRejected()
    {
        yield return MoveToPoint(rejectedPoint);
        onBagReachedRejectedPoint?.Invoke();
    }

    private IEnumerator MoveToPoint(Transform targetPoint)
    {
        if (targetPoint == null || currentBag == null)
            yield break;

        isMoving = true;

        Vector3 targetPos = targetPoint.position + bagPositionOffset;

        while (currentBag != null &&
               Vector3.Distance(currentBag.transform.position, targetPos) > 0.02f)
        {
            currentBag.transform.position = Vector3.MoveTowards(
                currentBag.transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            currentBag.transform.rotation = snappedRotation;

            yield return null;
        }

        isMoving = false;
    }
}