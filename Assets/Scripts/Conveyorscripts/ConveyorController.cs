using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConveyorController : MonoBehaviour
{
    [Header("Maletas (en orden)")]
    [SerializeField] private List<GameObject> bagPrefabs = new List<GameObject>();

    [Header("Posiciones clave (en X)")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform inspectPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform rejectPoint;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float delayBeforeNext = 0.5f;

    [Header("Temporizador de inspeccion")]
    [SerializeField] private InspectionTimer inspectionTimer;
    [SerializeField] private float initialInspectTime = 10f;
    [SerializeField] private float timeReductionPerBag = 0.5f;
    [SerializeField] private float minimumInspectTime = 3f;

    [Header("Fila de pasajeros")]
    [SerializeField] private PassengerQueue passengerQueue;

    [Header("Animacion")]
    [SerializeField] Animator buttonsAnim;

    [Header("Sonido - Rechazo")]
    [SerializeField] private AudioSource alarmAudio;

    [Header("Feedback - Correcto")]
    [SerializeField] private UnityEvent onCorrectDecision;

    [Header("Feedback - Incorrecto")]
    [SerializeField] private UnityEvent onWrongDecision;

    // ---------------------------------------------------------------
    // NUEVO: controla si el conveyor ya fue activado por la zona
    // ---------------------------------------------------------------
    [Header("Zona de trabajo")]
    [Tooltip("Si esta activo, el conveyor NO arranca en Start(); espera la llamada a StartConveyor() " +
             "desde WorkZoneTrigger. Desmarcalo solo para pruebas rapidas en el Editor.")]
    [SerializeField] private bool waitForWorkZone = true;

    private bool conveyorStarted = false;
    // ---------------------------------------------------------------

    private int currentIndex = 0;
    private GameObject currentBag = null;
    private bool waitingForDecision = false;
    private bool isMoving = false;

    void Start()
    {
        // Solo arranca automaticamente si NO esperamos zona
        if (!waitForWorkZone)
            SpawnNextBag();
    }

    /// <summary>
    /// Llamado por WorkZoneTrigger cuando el jugador entra a la zona de trabajo.
    /// Seguro de llamar varias veces (solo ejecuta la primera).
    /// </summary>
    public void StartConveyor()
    {
        if (conveyorStarted) return;
        conveyorStarted = true;
        SpawnNextBag();
    }

    // Calcula el tiempo disponible para esta maleta (se reduce progresivamente)
    private float GetCurrentInspectTime()
    {
        float t = initialInspectTime - (currentIndex - 1) * timeReductionPerBag;
        return Mathf.Max(t, minimumInspectTime);
    }

    // --- Botones ---
    public void OnContinue()
    {
        if (!waitingForDecision || isMoving) return;
        buttonsAnim.SetTrigger("GreenButton");
        EvaluateDecision(reportPressed: false);
    }

    public void OnReport()
    {
        if (!waitingForDecision || isMoving) return;
        buttonsAnim.SetTrigger("RedButton");
        EvaluateDecision(reportPressed: true);
    }

    private void Update()
    {
        buttonsAnim.SetBool("CanChoose", waitingForDecision);
    }

    // --- Evaluacion ---
    private void EvaluateDecision(bool reportPressed)
    {
        if (inspectionTimer != null)
            inspectionTimer.StopTimer();

        BagController bag = currentBag.GetComponent<BagController>();
        bool correct = false;

        if (bag != null && bag.data != null)
            correct = (reportPressed == bag.data.hasDangerousItem);

        if (correct) onCorrectDecision.Invoke();
        else onWrongDecision.Invoke();

        if (reportPressed)
        {
            passengerQueue?.RejectCurrentPassenger();
            StartCoroutine(RejectAndNext());
        }
        else
        {
            passengerQueue?.ApproveCurrentPassenger();
            StartCoroutine(ExitAndNext());
        }
    }

    // Llamado automaticamente por InspectionTimer cuando se agota el tiempo
    public void OnTimeExpired()
    {
        if (!waitingForDecision || isMoving) return;
        waitingForDecision = false;
        onWrongDecision.Invoke();
        passengerQueue?.ApproveCurrentPassenger();
        StartCoroutine(ExitAndNext());
    }

    // --- Spawn ---
    private void SpawnNextBag()
    {
        if (currentIndex >= bagPrefabs.Count)
        {
            Debug.Log("Todas las maletas procesadas.");
            return;
        }

        currentBag = Instantiate(bagPrefabs[currentIndex], spawnPoint.position, spawnPoint.rotation);
        currentIndex++;
        StartCoroutine(MoveToInspect());
    }

    private IEnumerator MoveToInspect()
    {
        isMoving = true;
        yield return StartCoroutine(MoveTo(currentBag, inspectPoint.position));
        isMoving = false;
        waitingForDecision = true;

        if (inspectionTimer != null)
            inspectionTimer.StartTimer(GetCurrentInspectTime(), OnTimeExpired);
    }

    private IEnumerator ExitAndNext()
    {
        waitingForDecision = false;
        isMoving = true;

        yield return StartCoroutine(MoveTo(currentBag, exitPoint.position));

        Destroy(currentBag);
        currentBag = null;
        isMoving = false;

        yield return new WaitForSeconds(delayBeforeNext);
        SpawnNextBag();
    }

    private IEnumerator RejectAndNext()
    {
        waitingForDecision = false;
        isMoving = true;

        if (alarmAudio != null)
            alarmAudio.Play();

        Transform target = rejectPoint != null ? rejectPoint : spawnPoint;
        yield return StartCoroutine(MoveTo(currentBag, target.position));

        Destroy(currentBag);
        currentBag = null;
        isMoving = false;

        yield return new WaitForSeconds(delayBeforeNext);
        SpawnNextBag();
    }

    private IEnumerator MoveTo(GameObject bag, Vector3 target)
    {
        while (bag != null && Vector3.Distance(bag.transform.position, target) > 0.01f)
        {
            bag.transform.position = Vector3.MoveTowards(
                bag.transform.position,
                target,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (bag != null)
            bag.transform.position = target;
    }
}