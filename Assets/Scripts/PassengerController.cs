using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controla el movimiento y estado de un pasajero individual.
/// Se instancia desde PassengerQueue.
/// </summary>
public class PassengerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] public float moveSpeed = 1.4f;

    [Header("Sonido - Rechazo")]
    [SerializeField] private AudioSource rejectAudio; // sonido cuando es expulsado

    // Estados posibles del pasajero
    public enum PassengerState { InQueue, MovingToScanner, AtScanner, Approved, Rejected }
    public PassengerState State { get; private set; } = PassengerState.InQueue;

    // Accion que notifica a la cola cuando este pasajero termino su ciclo
    public Action onDone;

    private Coroutine moveCoroutine;

    // -------------------------------------------------------
    // API publica llamada desde PassengerQueue
    // -------------------------------------------------------

    /// Mueve el pasajero a la posicion de fila asignada (sin notificar al terminar)
    public void MoveToQueueSlot(Vector3 position)
    {
        StopCurrentMove();
        moveCoroutine = StartCoroutine(MoveTo(position, null));
    }

    /// Manda al pasajero al escaner; al llegar avisa con onArrived
    public void MoveToScanner(Vector3 scannerPos, Action onArrived)
    {
        State = PassengerState.MovingToScanner;
        StopCurrentMove();
        moveCoroutine = StartCoroutine(MoveTo(scannerPos, () =>
        {
            State = PassengerState.AtScanner;
            onArrived?.Invoke();
        }));
    }

    /// Pasajero aprobado: camina al exitPoint y se destruye
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

    /// Pasajero rechazado: suena el audio, camina al rejectExit y se destruye
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

    // -------------------------------------------------------
    // Internos
    // -------------------------------------------------------

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
        // Rotar hacia el destino al arrancar
        Vector3 dir = (target - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = target;
        onReached?.Invoke();
    }
}