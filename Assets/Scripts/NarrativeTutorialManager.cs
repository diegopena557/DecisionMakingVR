using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NarrativeTutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class NarrativeTutorialStep
    {
        [Header("Contenido")]
        [TextArea(2, 6)]
        public string subtitle;
        public AudioClip voiceClip;

        [Header("Flujo")]
        public bool requiresAction = false;

        [Tooltip("ID que debe reportar otro script llamando TryHandleActionAttempt(id).")]
        public int expectedActionId = -1;

        [Tooltip("Si este paso no requiere acción, avanza solo después de este tiempo.")]
        public float autoAdvanceDelay = 1f;

        [Header("Ayudas visuales")]
        public GameObject indicator;
        public GameObject lightIndicator;

        [Header("Permisos del jugador en este paso")]
        public bool allowMovement = false;
        public bool allowDecisionInput = false;

        [Header("Reintento opcional por tiempo")]
        public bool playRetryOnce = false;
        public float retryAfterSeconds = 8f;

        [TextArea(2, 4)]
        public string retrySubtitle;
        public AudioClip retryClip;

        [Header("Feedback por acción incorrecta")]
        public bool playWrongActionFeedback = true;
        public float wrongActionCooldown = 1.25f;

        [TextArea(2, 4)]
        public string wrongActionSubtitle;
        public AudioClip wrongActionClip;

        [Header("Eventos opcionales")]
        public UnityEvent onStepStarted;
        public UnityEvent onStepCompleted;
    }

    [Header("UI")]
    public TMP_Text subtitleText;

    [Header("Pasos")]
    public NarrativeTutorialStep[] steps;

    [Header("Audio")]
    public AudioSource voiceSource;
    public AudioSource sfxSource;
    public AudioClip stepCompleteClip;
    public AudioClip tutorialCompleteClip;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    [Range(0f, 1f)] public float stepVolume = 1f;
    [Range(0f, 1f)] public float finalVolume = 1f;

    [Header("Sistemas a bloquear/desbloquear")]
    public Behaviour[] movementBehaviours;
    public Behaviour[] decisionBehaviours;
    public GameObject[] movementObjects;
    public GameObject[] decisionObjects;

    [Header("Canvas fijo que solo rota hacia el usuario")]
    public Transform floatingUIRoot;
    public Transform followTarget;
    public float rotateSmooth = 10f;
    public bool yawOnly = true;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Finalización")]
    public bool markTutorialCompleted = true;
    public string playerPrefsKey = "AirportTutorialCompleted";
    public string sceneToLoadOnFinish = "";
    public float finishDelay = 1.5f;

    private int currentStep = -1;
    public int CurrentStep => currentStep;

    private Coroutine tutorialRoutine;
    private Coroutine temporaryFeedbackRoutine;

    private bool waitingForAction = false;
    private bool actionCompleted = false;
    private bool retryPlayed = false;
    private float lastWrongActionTime = -999f;

    void Start()
    {
        AutoAssignReferences();
        SnapUIRotationToTarget();
        BeginTutorial();
    }

    void LateUpdate()
    {
        RotateFloatingUIOnly();
    }

    public void BeginTutorial()
    {
        if (tutorialRoutine != null)
            StopCoroutine(tutorialRoutine);

        tutorialRoutine = StartCoroutine(RunTutorial());
    }

    IEnumerator RunTutorial()
    {
        if (steps == null || steps.Length == 0)
        {
            if (subtitleText != null)
                subtitleText.text = "No hay pasos configurados.";
            yield break;
        }

        for (int i = 0; i < steps.Length; i++)
        {
            currentStep = i;
            waitingForAction = false;
            actionCompleted = false;
            retryPlayed = false;
            lastWrongActionTime = -999f;

            NarrativeTutorialStep step = steps[i];

            ApplyStepState(step);
            RefreshIndicators();
            SetSubtitle(step.subtitle);
            step.onStepStarted?.Invoke();

            yield return PlayVoice(step.voiceClip);

            if (step.requiresAction)
            {
                waitingForAction = true;
                float waitTimer = 0f;

                while (!actionCompleted)
                {
                    waitTimer += Time.deltaTime;

                    if (step.playRetryOnce && !retryPlayed && waitTimer >= step.retryAfterSeconds)
                    {
                        retryPlayed = true;
                        yield return PlayRetry(step);
                    }

                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(step.autoAdvanceDelay);
            }

            step.onStepCompleted?.Invoke();
            PlayStepFeedback(i < steps.Length - 1);
        }

        yield return FinishTutorial();
    }

    void ApplyStepState(NarrativeTutorialStep step)
    {
        SetBehavioursState(movementBehaviours, step.allowMovement);
        SetBehavioursState(decisionBehaviours, step.allowDecisionInput);
        SetObjectsState(movementObjects, step.allowMovement);
        SetObjectsState(decisionObjects, step.allowDecisionInput);
    }

    void SetBehavioursState(Behaviour[] behaviours, bool enabledState)
    {
        if (behaviours == null) return;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
                behaviours[i].enabled = enabledState;
        }
    }

    void SetObjectsState(GameObject[] objects, bool activeState)
    {
        if (objects == null) return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(activeState);
        }
    }

    void RefreshIndicators()
    {
        for (int i = 0; i < steps.Length; i++)
        {
            bool active = (i == currentStep);

            if (steps[i].indicator != null)
                steps[i].indicator.SetActive(active);

            if (steps[i].lightIndicator != null)
                steps[i].lightIndicator.SetActive(active);
        }
    }

    IEnumerator PlayVoice(AudioClip clip)
    {
        if (voiceSource == null || clip == null)
            yield break;

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.volume = voiceVolume;
        voiceSource.Play();

        while (voiceSource.isPlaying)
            yield return null;
    }

    IEnumerator PlayRetry(NarrativeTutorialStep step)
    {
        if (!string.IsNullOrEmpty(step.retrySubtitle))
            SetSubtitle(step.retrySubtitle);

        if (voiceSource != null && step.retryClip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = step.retryClip;
            voiceSource.volume = voiceVolume;
            voiceSource.Play();

            while (voiceSource.isPlaying)
                yield return null;
        }

        SetSubtitle(step.subtitle);
    }

    void PlayStepFeedback(bool isIntermediateStep)
    {
        if (sfxSource == null)
            return;

        if (isIntermediateStep)
        {
            if (stepCompleteClip != null)
                sfxSource.PlayOneShot(stepCompleteClip, stepVolume);
        }
        else
        {
            if (tutorialCompleteClip != null)
                sfxSource.PlayOneShot(tutorialCompleteClip, finalVolume);
        }
    }

    IEnumerator FinishTutorial()
    {
        if (markTutorialCompleted)
        {
            PlayerPrefs.SetInt(playerPrefsKey, 1);
            PlayerPrefs.Save();
        }

        SetSubtitle("Tutorial completado.");

        SetBehavioursState(movementBehaviours, true);
        SetBehavioursState(decisionBehaviours, true);
        SetObjectsState(movementObjects, true);
        SetObjectsState(decisionObjects, true);

        yield return new WaitForSeconds(finishDelay);

        if (!string.IsNullOrEmpty(sceneToLoadOnFinish))
        {
            EncuestaVASFlow.PrepararAntesExperiencia("SampleScene");

            if (SceneFader.Instance != null)
                SceneFader.Instance.LoadSceneWithFade(sceneToLoadOnFinish);
        }
    }

    // Sigue funcionando para acciones correctas directas.
    public void CompleteExpectedStep(int reportedActionId)
    {
        if (!waitingForAction)
            return;

        if (currentStep < 0 || currentStep >= steps.Length)
            return;

        NarrativeTutorialStep step = steps[currentStep];

        if (!step.requiresAction)
            return;

        if (step.expectedActionId != reportedActionId)
            return;

        actionCompleted = true;
        waitingForAction = false;
    }

    // Nuevo: manejar intento de acción y devolver si se permite continuar con la lógica externa.
    public bool TryHandleActionAttempt(int reportedActionId)
    {
        if (currentStep < 0 || currentStep >= steps.Length)
            return true;

        NarrativeTutorialStep step = steps[currentStep];

        // Si aún no toca una acción, se bloquea cualquier intento de "saltarse".
        if (!waitingForAction || !step.requiresAction)
        {
            TriggerWrongActionFeedback(step);
            return false;
        }

        // Acción correcta: deja avanzar.
        if (step.expectedActionId == reportedActionId)
        {
            actionCompleted = true;
            waitingForAction = false;
            return true;
        }

        // Acción incorrecta: no avanza y recuerda la instrucción.
        TriggerWrongActionFeedback(step);
        return false;
    }

    void TriggerWrongActionFeedback(NarrativeTutorialStep step)
    {
        if (!step.playWrongActionFeedback)
            return;

        if (Time.time - lastWrongActionTime < step.wrongActionCooldown)
            return;

        lastWrongActionTime = Time.time;

        if (temporaryFeedbackRoutine != null)
            StopCoroutine(temporaryFeedbackRoutine);

        temporaryFeedbackRoutine = StartCoroutine(ShowTemporaryWrongFeedback(step));
    }

    IEnumerator ShowTemporaryWrongFeedback(NarrativeTutorialStep step)
    {
        string previousSubtitle = step.subtitle;

        if (!string.IsNullOrEmpty(step.wrongActionSubtitle))
            SetSubtitle(step.wrongActionSubtitle);

        if (voiceSource != null && step.wrongActionClip != null)
            voiceSource.PlayOneShot(step.wrongActionClip, voiceVolume);

        float waitTime = 1f;
        if (step.wrongActionClip != null)
            waitTime = Mathf.Max(0.75f, step.wrongActionClip.length);

        yield return new WaitForSeconds(waitTime);

        if (currentStep >= 0 && currentStep < steps.Length && waitingForAction)
            SetSubtitle(previousSubtitle);

        temporaryFeedbackRoutine = null;
    }

    public bool IsWaitingForAction()
    {
        return waitingForAction;
    }

    public int GetExpectedActionId()
    {
        if (currentStep < 0 || currentStep >= steps.Length)
            return -1;

        return steps[currentStep].expectedActionId;
    }

    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    void SetSubtitle(string message)
    {
        if (subtitleText != null)
            subtitleText.text = message;
    }

    void AutoAssignReferences()
    {
        if (followTarget == null && Camera.main != null)
            followTarget = Camera.main.transform;

        if (floatingUIRoot == null && subtitleText != null)
        {
            Canvas parentCanvas = subtitleText.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                floatingUIRoot = parentCanvas.transform;
            else
                floatingUIRoot = subtitleText.transform;
        }
    }

    void RotateFloatingUIOnly()
    {
        if (floatingUIRoot == null || followTarget == null)
            return;

        Vector3 lookDir = followTarget.position - floatingUIRoot.position;

        if (yawOnly)
            lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f)
            return;

        Quaternion desiredRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        desiredRotation *= Quaternion.Euler(rotationOffset);

        float rotT = 1f - Mathf.Exp(-rotateSmooth * Time.deltaTime);
        floatingUIRoot.rotation = Quaternion.Slerp(floatingUIRoot.rotation, desiredRotation, rotT);
    }

    void SnapUIRotationToTarget()
    {
        if (floatingUIRoot == null || followTarget == null)
            return;

        Vector3 lookDir = followTarget.position - floatingUIRoot.position;

        if (yawOnly)
            lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f)
            return;

        Quaternion snapRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        floatingUIRoot.rotation = snapRotation * Quaternion.Euler(rotationOffset);
    }
}