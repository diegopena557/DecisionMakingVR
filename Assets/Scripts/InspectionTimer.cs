using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Coloca este script en un GameObject vacio en la escena.
/// En el Inspector asigna un Image con Image Type = Filled (Fill Method = Horizontal)
/// para que funcione como barra que se vacia de derecha a izquierda.
/// 
/// Colores:
///   Verde  -> mas del 50% de tiempo restante
///   Amarillo -> entre 25% y 50%
///   Rojo   -> menos del 25%
/// </summary>
public class InspectionTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image timerBar;          // Image de tipo Filled
    [SerializeField] private GameObject timerPanel;   // Panel/Canvas que contiene la barra (para mostrar/ocultar)

    [Header("Colores de la barra")]
    [SerializeField] private Color colorFull = new Color(0.18f, 0.80f, 0.44f); // verde
    [SerializeField] private Color colorMid = new Color(1f, 0.76f, 0f);     // amarillo
    [SerializeField] private Color colorLow = new Color(0.91f, 0.30f, 0.24f); // rojo

    [Header("Pulso al quedar poco tiempo")]
    [SerializeField] private float pulseThreshold = 0.25f;  // porcentaje desde el que empieza el pulso
    [SerializeField] private float pulseSpeed = 4f;

    private float totalTime;
    private float remainingTime;
    private bool isRunning;
    private Action onExpired;

    private Coroutine timerCoroutine;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        HideBar();
    }

    // Inicia el temporizador. duration = segundos totales. expired = callback al terminarse.
    public void StartTimer(float duration, Action expired)
    {
        StopTimer();

        totalTime = duration;
        remainingTime = duration;
        onExpired = expired;
        isRunning = true;

        ShowBar();
        UpdateBar(1f);

        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    // Detiene y oculta la barra (llamar al tomar una decision)
    public void StopTimer()
    {
        isRunning = false;

        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }
        if (pulseCoroutine != null) { StopCoroutine(pulseCoroutine); pulseCoroutine = null; }

        HideBar();
    }

    private IEnumerator TimerRoutine()
    {
        bool pulsing = false;

        while (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            float ratio = Mathf.Clamp01(remainingTime / totalTime);

            UpdateBar(ratio);

            // Arrancar pulso cuando queda poco tiempo
            if (ratio <= pulseThreshold && !pulsing)
            {
                pulsing = true;
                pulseCoroutine = StartCoroutine(PulseRoutine());
            }

            yield return null;
        }

        // Tiempo agotado
        UpdateBar(0f);
        StopTimer();
        onExpired?.Invoke();
    }

    // Parpadea el alpha de la barra para llamar la atencion (no toca la escala)
    private IEnumerator PulseRoutine()
    {
        while (isRunning)
        {
            float alpha = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed));
            Color c = timerBar.color;
            timerBar.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        // Restaurar alpha completo al terminar
        Color final = timerBar.color;
        timerBar.color = new Color(final.r, final.g, final.b, 1f);
    }

    private void UpdateBar(float ratio)
    {
        if (timerBar == null) return;

        timerBar.fillAmount = ratio;

        // Cambiar color segun el porcentaje restante
        if (ratio > 0.5f) timerBar.color = colorFull;
        else if (ratio > 0.25f) timerBar.color = Color.Lerp(colorMid, colorFull, (ratio - 0.25f) / 0.25f);
        else timerBar.color = Color.Lerp(colorLow, colorMid, ratio / 0.25f);
    }

    private void ShowBar()
    {
        if (timerPanel != null) timerPanel.SetActive(true);
        else if (timerBar != null) timerBar.gameObject.SetActive(true);
    }

    private void HideBar()
    {
        if (timerPanel != null) timerPanel.SetActive(false);
        else if (timerBar != null) timerBar.gameObject.SetActive(false);
    }
}