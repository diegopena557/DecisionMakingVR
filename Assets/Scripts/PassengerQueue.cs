using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerQueue : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private List<GameObject> passengerPrefabs = new List<GameObject>();
    [SerializeField] private bool randomizeOrder = true;
    [SerializeField] private int totalPassengers = 10; // total exacto de pasajeros en toda la sesion

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

    // Cuantos pasajeros han sido spawneados en total
    private int spawnedCount = 0;

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

        // Llenar el pool hasta exactamente totalPassengers, ciclando los prefabs si hay menos
        int added = 0;
        while (added < totalPassengers)
        {
            prefabPool.Enqueue(list[added % list.Count]);
            added++;
        }
    }

    private void FillQueue()
    {
        for (int i = 0; i < queueSlots.Length; i++)
            SpawnAtSlot(i);
    }

    private void SpawnAtSlot(int slotIndex)
    {
        if (prefabPool.Count == 0) return;  // no quedan mas pasajeros
        if (slotIndex < 0 || slotIndex >= queueSlots.Length) return;

        GameObject prefab = prefabPool.Dequeue();
        GameObject go = Instantiate(prefab, queueSlots[slotIndex].position, queueSlots[slotIndex].rotation);

        PassengerController pc = go.GetComponent<PassengerController>();
        if (pc == null) pc = go.AddComponent<PassengerController>();

        pc.moveSpeed = passengerMoveSpeed;
        slotOccupants[slotIndex] = pc;
        spawnedCount++;
    }

    private void AdvanceFirstToScanner()
    {
        if (slotOccupants.Length == 0) return;

        PassengerController first = slotOccupants[0];
        if (first == null)
        {
            Debug.Log("Todos los pasajeros han sido procesados.");
            return;
        }

        slotOccupants[0] = null;
        activePassenger = first;
        first.MoveToScanner(scannerPoint.position, null);
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

        // Spawnar nuevo pasajero al final solo si quedan en el pool
        int lastSlot = slotOccupants.Length - 1;
        if (prefabPool.Count > 0)
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