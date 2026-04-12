using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConveyorController : MonoBehaviour
{
    [Header("Maletas (en orden)")]
    [SerializeField] private List<GameObject> bagPrefabs = new List<GameObject>();
    
    [Header("Posiciones clave (en X)")]
    [SerializeField] private Transform spawnPoint;       // donde aparece la maleta
    [SerializeField] private Transform inspectPoint;     // donde se detiene
    [SerializeField] private Transform exitPoint;        // hasta donde llega antes de desaparecer

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float delayBeforeNext = 0.5f; // pausa antes de que aparezca la siguiente

    [Header("Animacion")]
    [SerializeField] Animator buttonsAnim;

    [Header("Feedback - Correcto")]
    [SerializeField] private UnityEvent onCorrectDecision;

    [Header("Feedback - Incorrecto")]
    [SerializeField] private UnityEvent onWrongDecision;

    private int currentIndex = 0;
    private GameObject currentBag = null;
    private bool waitingForDecision = false;
    private bool isMoving = false;

    void Start() => SpawnNextBag();

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
        if (waitingForDecision)
        {
            buttonsAnim.SetBool("CanChoose", true);
        }
        else
        {
            buttonsAnim.SetBool("CanChoose", false);
        }
    }
    // --- Evaluacion ---
    private void EvaluateDecision(bool reportPressed)
    {
        BagController bag = currentBag.GetComponent<BagController>();

        bool correct = false;

        if (bag != null && bag.data != null)
        {
            // Correcto si: reporto y habia peligro, o dejó pasar y no habia peligro
            correct = (reportPressed == bag.data.hasDangerousItem);
        }

        if (correct)
            onCorrectDecision.Invoke();
        else
            onWrongDecision.Invoke();

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
