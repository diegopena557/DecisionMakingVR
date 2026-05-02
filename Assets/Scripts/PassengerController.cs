using System;
using System.Collections;
using UnityEngine;


public class PassengerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] public float moveSpeed = 1.4f;

    [Header("Animacion")]
    [SerializeField] private Animator animator;
    [SerializeField] private string walkParam = "isWalking"; // nombre del Bool en el Animator

    [Header("Sonido - Rechazo")]
    [SerializeField] private AudioSource rejectAudio;

    // Estados posibles del pasajero
    public enum PassengerState { InQueue, MovingToScanner, AtScanner, Approved, Rejected }
    public PassengerState State { get; private set; } = PassengerState.InQueue;

    public Action onDone;

    private Coroutine moveCoroutine;

    private void Awake()
    {
        // Buscar Animator automaticamente si no fue asignado en el Inspector
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        SetWalking(false); // empieza en Idle
    }

    

    public void MoveToQueueSlot(Vector3 position)
    {
        StopCurrentMove();
        moveCoroutine = StartCoroutine(MoveTo(position, () => SetWalking(false)));
    }

    public void MoveToScanner(Vector3 scannerPos, Action onArrived)
    {
        State = PassengerState.MovingToScanner;
        StopCurrentMove();
        moveCoroutine = StartCoroutine(MoveTo(scannerPos, () =>
        {
            State = PassengerState.AtScanner;
            SetWalking(false); // idle mientras espera la decision
            onArrived?.Invoke();
        }));
    }

    public void Approve(Vector3 exitPos)
    {
        State = PassengerState.Approved;
        StopCurrentMove();
        moveCoroutine = StartCoroutine(MoveTo(exitPos, () =>
        {
            onDone?.Invoke();
            Destroy(gameObject);
        }));
    }

    public void Reject(Vector3 rejectExitPos)
    {
        State = PassengerState.Rejected;
        StopCurrentMove();

        if (rejectAudio != null)
            rejectAudio.Play();

        moveCoroutine = StartCoroutine(MoveTo(rejectExitPos, () =>
        {
            onDone?.Invoke();
            Destroy(gameObject);
        }));
    }

    
    // Animacion
    

    private void SetWalking(bool walking)
    {
        if (animator == null) return;
        animator.SetBool(walkParam, walking);
    }

    
    // Movimiento
   

    private void StopCurrentMove()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
    }

    private IEnumerator MoveTo(Vector3 target, Action onReached)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;

        // Solo moverse si hay distancia real
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
            SetWalking(true);

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                // Actualizar rotacion suavemente mientras camina
                Vector3 currentDir = (target - transform.position);
                currentDir.y = 0;
                if (currentDir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(currentDir),
                        10f * Time.deltaTime
                    );

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        transform.position = target;
        SetWalking(false);
        onReached?.Invoke();
    }
}