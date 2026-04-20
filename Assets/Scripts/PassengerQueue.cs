using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la fila de pasajeros:
///   - Instancia pasajeros al inicio llenando los slots visibles
///   - Cuando el primero avanza al escaner, los demas avanzan un slot
///   - Cuando termina el ciclo del pasajero activo, instancia uno nuevo al final
///
/// SETUP EN UNITY:
///   1. Crea un GameObject vacio "PassengerQueue" y agrega este script.
///   2. Crea Transforms vacios en la escena para cada slot de la fila
///      (ej: QueueSlot_0 mas cerca del escaner, QueueSlot_1, QueueSlot_2...).
///      Arrastralos al array queueSlots en el Inspector.
///   3. Asigna el prefab del pasajero (con PassengerController).
///   4. Asigna scannerPoint, passengerExitPoint, passengerRejectPoint.
///   5. Llama a ApproveCurrentPassenger() o RejectCurrentPassenger()
///      desde ConveyorController segun la decision tomada.
/// </summary>
public class PassengerQueue : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private List<GameObject> passengerPrefabs = new List<GameObject>();
    [SerializeField] private bool randomizeOrder = true;

    [Header("Slots de la fila (ordenados: 0 = frente, ultimo = atras)")]
    [SerializeField] private Transform[] queueSlots;

    [Header("Posiciones clave")]
    [SerializeField] private Transform scannerPoint;         // donde espera frente al escaner
    [SerializeField] private Transform passengerExitPoint;   // hacia donde va si es aprobado
    [SerializeField] private Transform passengerRejectPoint; // hacia donde va si es rechazado

    [Header("Movimiento")]
    [SerializeField] private float passengerMoveSpeed = 1.4f;
    [SerializeField] private float queueAdvanceDelay = 0.15f; // pausa entre cada persona al avanzar

    // Lista de prefabs mezclados para usar en orden
    private Queue<GameObject> prefabPool = new Queue<GameObject>();

    // Pasajeros actualmente en los slots (null si el slot esta vacio)
    private PassengerController[] slotOccupants;

    // Pasajero que esta en el escaner ahora mismo
    private PassengerController activePassenger;

    // -------------------------------------------------------
    void Start()
    {
        BuildPrefabPool();
        slotOccupants = new PassengerController[queueSlots.Length];
        FillQueue();
        AdvanceFirstToScanner();
    }

    // -------------------------------------------------------
    // API publica (llamar desde ConveyorController)
    // -------------------------------------------------------

    public void ApproveCurrentPassenger()
    {
        if (activePassenger == null) return;
        activePassenger.Approve(passengerExitPoint.position);
        activePassenger.onDone = OnActiveDone;
        activePassenger = null;
    }

    public void RejectCurrentPassenger()
    {
        if (activePassenger == null) return;
        activePassenger.Reject(passengerRejectPoint.position);
        activePassenger.onDone = OnActiveDone;
        activePassenger = null;
    }

    // -------------------------------------------------------
    // Internos
    // -------------------------------------------------------

    private void BuildPrefabPool()
    {
        if (passengerPrefabs.Count == 0)
        {
            Debug.LogWarning("PassengerQueue: no hay prefabs asignados.");
            return;
        }

        List<GameObject> list = new List<GameObject>(passengerPrefabs);

        if (randomizeOrder)
            Shuffle(list);

        // Repetir la lista ciclicamente para que nunca se acabe
        // (duplicamos varias veces para tener stock suficiente)
        for (int i = 0; i < 10; i++)
            foreach (var p in list)
                prefabPool.Enqueue(p);
    }

    private void FillQueue()
    {
        for (int i = 0; i < queueSlots.Length; i++)
            SpawnAtSlot(i);
    }

    private void SpawnAtSlot(int slotIndex)
    {
        if (prefabPool.Count == 0) return;
        if (slotIndex < 0 || slotIndex >= queueSlots.Length) return;

        GameObject prefab = prefabPool.Dequeue();
        GameObject go = Instantiate(prefab, queueSlots[slotIndex].position, queueSlots[slotIndex].rotation);

        PassengerController pc = go.GetComponent<PassengerController>();
        if (pc == null) pc = go.AddComponent<PassengerController>();

        pc.moveSpeed = passengerMoveSpeed;
        slotOccupants[slotIndex] = pc;
    }

    /// El primero de la fila avanza al punto del escaner
    private void AdvanceFirstToScanner()
    {
        if (slotOccupants.Length == 0) return;

        PassengerController first = slotOccupants[0];
        if (first == null) return;

        // Sacar del slot
        slotOccupants[0] = null;

        activePassenger = first;
        first.MoveToScanner(scannerPoint.position, null);

        // El resto de la fila avanza un slot
        StartCoroutine(AdvanceQueue());
    }

    /// Mueve cada pasajero de la fila un slot hacia adelante y spawna uno nuevo al final
    private IEnumerator AdvanceQueue()
    {
        for (int i = 0; i < slotOccupants.Length - 1; i++)
        {
            if (slotOccupants[i + 1] != null)
            {
                slotOccupants[i] = slotOccupants[i + 1];
                slotOccupants[i + 1] = null;
                slotOccupants[i].MoveToQueueSlot(queueSlots[i].position);
                yield return new WaitForSeconds(queueAdvanceDelay);
            }
        }

        // Spawnar nuevo pasajero al final de la fila
        int lastSlot = slotOccupants.Length - 1;
        SpawnAtSlot(lastSlot);
    }

    /// Cuando el pasajero activo termina su salida, mandar al siguiente al escaner
    private void OnActiveDone()
    {
        // Pequena pausa para que no sea inmediato
        StartCoroutine(NextPassengerDelay());
    }

    private IEnumerator NextPassengerDelay()
    {
        yield return new WaitForSeconds(0.3f);
        AdvanceFirstToScanner();
    }

    // -------------------------------------------------------
    // Utilidades
    // -------------------------------------------------------

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}