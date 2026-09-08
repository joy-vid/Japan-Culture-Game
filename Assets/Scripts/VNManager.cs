using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum Speaker
{
    Aiko,
    Oni
}

[System.Serializable]
public class DialogueBlock
{
    public Speaker speaker;

    public string displayName;

    [TextArea(2, 5)]
    public string[] lines;
}

public class VNManager : MonoBehaviour
{
    // =========================
    // DIALOGUE UI
    // =========================

    [Header("Dialogue UI")]
    public TMP_Text nameText;
    public TMP_Text dialogueText;


    // =========================
    // PORTRAITS
    // =========================

    [Header("Portraits")]
    public CanvasGroup aikoPortrait;
    public CanvasGroup oniPortrait;

    [Header("Portrait Settings")]
    public float portraitFadeDuration = 0.2f;


    // =========================
    // DIALOGUE DATA
    // =========================

    [Header("Dialogue")]
    public DialogueBlock[] dialogueBlocks;


    // =========================
    // TYPING
    // =========================

    [Header("Typing Settings")]
    public float typingSpeed = 0.025f;

    public AudioSource typingAudioSource;
    public AudioClip typingSound;

    public int soundEveryCharacters = 2;


    // =========================
    // NEXT INDICATOR
    // =========================

    [Header("Next Indicator")]
    public GameObject nextIndicator;

    public float nextPulseSpeed = 3f;
    public float nextPulseAmount = 0.08f;


    // =========================
    // BATTLE TRANSITION
    // =========================

    [Header("Battle Transition")]
    public CanvasGroup battleTransition;
    public Image battleTransitionImage;


    // =========================
    // BATTLE SFX
    // =========================

    [Header("Battle Transition SFX")]
    public AudioSource transitionAudioSource;
    public AudioClip battleSlashSound;


    // =========================
    // WHITE FLASH
    // =========================

    [Header("White Flash")]
    public float whiteFlashInDuration = 0.035f;
    public float whiteFlashOutDuration = 0.06f;
    public float whiteFlashAlpha = 1f;


    // =========================
    // RED AFTERGLOW
    // =========================

    [Header("Red Afterglow")]
    public Color redAfterglowColor =
        new Color(0.45f, 0.02f, 0.02f, 1f);

    public float redFadeInDuration = 0.08f;
    public float redHoldDuration = 0.08f;
    public float redFadeOutDuration = 0.14f;

    public float redAfterglowAlpha = 0.4f;


    // =========================
    // BLACK FADE
    // =========================

    [Header("Black Fade")]
    public float battleFadeDuration = 0.4f;
    public float blackScreenDuration = 0.2f;


    // =========================
    // PRIVATE VARIABLES
    // =========================

    private int blockIndex = 0;
    private int lineIndex = 0;

    private Speaker? currentSpeaker = null;

    private Coroutine portraitCoroutine;
    private Coroutine typingCoroutine;
    private Coroutine nextPulseCoroutine;

    private bool isTyping = false;
    private bool isTransitioning = false;

    private string currentLine;

    private Vector3 nextIndicatorBaseScale;


    // =========================
    // START
    // =========================

    void Start()
    {
        aikoPortrait.alpha = 0f;
        oniPortrait.alpha = 0f;

        battleTransition.alpha = 0f;

        if (battleTransitionImage == null)
        {
            battleTransitionImage =
                battleTransition.GetComponent<Image>();
        }

        battleTransitionImage.color = Color.black;

        nextIndicatorBaseScale =
            nextIndicator.transform.localScale;

        nextIndicator.SetActive(false);

        ShowDialogue();
    }


    // =========================
    // NEXT DIALOGUE
    // =========================

    public void NextDialogue()
    {
        // Jangan bisa spam selama transition
        if (isTransitioning)
        {
            return;
        }

        // Kalau masih typing,
        // klik hanya menyelesaikan kalimat
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        HideNextIndicator();

        lineIndex++;

        // Masih ada line dalam block yang sama
        if (lineIndex <
            dialogueBlocks[blockIndex].lines.Length)
        {
            ShowDialogue();
            return;
        }

        // Pindah ke block berikutnya
        blockIndex++;
        lineIndex = 0;

        // Dialogue selesai → battle transition
        if (blockIndex >= dialogueBlocks.Length)
        {
            StartCoroutine(
                PlayBattleTransition()
            );

            return;
        }

        ShowDialogue();
    }


    // =========================
    // SHOW DIALOGUE
    // =========================

    private void ShowDialogue()
    {
        DialogueBlock currentBlock =
            dialogueBlocks[blockIndex];

        nameText.text =
            currentBlock.displayName;

        // Portrait hanya fade saat speaker berubah
        if (currentSpeaker != currentBlock.speaker)
        {
            currentSpeaker =
                currentBlock.speaker;

            ChangePortrait(
                currentBlock.speaker
            );
        }

        StartTyping(
            currentBlock.lines[lineIndex]
        );
    }


    // =========================
    // TYPING SYSTEM
    // =========================

    private void StartTyping(string line)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        HideNextIndicator();

        currentLine = line;

        typingCoroutine =
            StartCoroutine(
                TypeDialogue(line)
            );
    }


    private IEnumerator TypeDialogue(string line)
    {
        isTyping = true;

        dialogueText.text = line;

        dialogueText.maxVisibleCharacters = 0;

        dialogueText.ForceMeshUpdate();

        int totalCharacters =
            dialogueText.textInfo.characterCount;

        int soundCounter = 0;

        for (int i = 0; i < totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters =
                i + 1;

            char currentCharacter =
                dialogueText
                    .textInfo
                    .characterInfo[i]
                    .character;

            // Typing sound tidak bunyi di spasi
            if (!char.IsWhiteSpace(currentCharacter))
            {
                soundCounter++;

                if (soundCounter >= soundEveryCharacters)
                {
                    PlayTypingSound();
                    soundCounter = 0;
                }
            }

            yield return
                new WaitForSecondsRealtime(
                    typingSpeed
                );
        }

        isTyping = false;
        typingCoroutine = null;

        ShowNextIndicator();
    }


    private void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = currentLine;

        dialogueText.maxVisibleCharacters =
            int.MaxValue;

        isTyping = false;

        ShowNextIndicator();
    }


    private void PlayTypingSound()
    {
        if (typingAudioSource != null &&
            typingSound != null)
        {
            typingAudioSource.PlayOneShot(
                typingSound
            );
        }
    }


    // =========================
    // PORTRAIT SYSTEM
    // =========================

    private void ChangePortrait(Speaker speaker)
    {
        if (portraitCoroutine != null)
        {
            StopCoroutine(portraitCoroutine);
        }

        portraitCoroutine =
            StartCoroutine(
                FadePortraits(speaker)
            );
    }


    private IEnumerator FadePortraits(Speaker speaker)
    {
        float startAiko =
            aikoPortrait.alpha;

        float startOni =
            oniPortrait.alpha;

        float targetAiko =
            speaker == Speaker.Aiko
            ? 1f
            : 0f;

        float targetOni =
            speaker == Speaker.Oni
            ? 1f
            : 0f;

        float timer = 0f;

        while (timer < portraitFadeDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float t =
                timer / portraitFadeDuration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            aikoPortrait.alpha =
                Mathf.Lerp(
                    startAiko,
                    targetAiko,
                    t
                );

            oniPortrait.alpha =
                Mathf.Lerp(
                    startOni,
                    targetOni,
                    t
                );

            yield return null;
        }

        aikoPortrait.alpha = targetAiko;
        oniPortrait.alpha = targetOni;

        portraitCoroutine = null;
    }


    // =========================
    // NEXT INDICATOR
    // =========================

    private void ShowNextIndicator()
    {
        nextIndicator.SetActive(true);

        nextIndicator.transform.localScale =
            nextIndicatorBaseScale;

        if (nextPulseCoroutine != null)
        {
            StopCoroutine(nextPulseCoroutine);
        }

        nextPulseCoroutine =
            StartCoroutine(
                PulseNextIndicator()
            );
    }


    private void HideNextIndicator()
    {
        if (nextPulseCoroutine != null)
        {
            StopCoroutine(nextPulseCoroutine);

            nextPulseCoroutine = null;
        }

        nextIndicator.transform.localScale =
            nextIndicatorBaseScale;

        nextIndicator.SetActive(false);
    }


    private IEnumerator PulseNextIndicator()
    {
        while (true)
        {
            float pulse =
                1f +
                Mathf.Sin(
                    Time.unscaledTime *
                    nextPulseSpeed
                ) *
                nextPulseAmount;

            nextIndicator.transform.localScale =
                nextIndicatorBaseScale *
                pulse;

            yield return null;
        }
    }


    // =========================
    // BATTLE TRANSITION
    // =========================

    private IEnumerator PlayBattleTransition()
    {
        isTransitioning = true;

        HideNextIndicator();


        // =====================
        // PLAY SHING SFX
        // =====================

        if (transitionAudioSource != null &&
            battleSlashSound != null)
        {
            transitionAudioSource.PlayOneShot(
                battleSlashSound
            );
        }


        // =====================
        // WHITE FLASH IN
        // =====================

        battleTransitionImage.color =
            Color.white;

        float timer = 0f;

        battleTransition.alpha = 0f;

        while (timer < whiteFlashInDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / whiteFlashInDuration;

            battleTransition.alpha =
                Mathf.Lerp(
                    0f,
                    whiteFlashAlpha,
                    t
                );

            yield return null;
        }

        battleTransition.alpha =
            whiteFlashAlpha;


        // =====================
        // WHITE FLASH OUT
        // =====================

        timer = 0f;

        while (timer < whiteFlashOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / whiteFlashOutDuration;

            battleTransition.alpha =
                Mathf.Lerp(
                    whiteFlashAlpha,
                    0f,
                    t
                );

            yield return null;
        }

        battleTransition.alpha = 0f;


        // =====================
        // RED AFTERGLOW IN
        // =====================

        battleTransitionImage.color =
            redAfterglowColor;

        timer = 0f;

        while (timer < redFadeInDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / redFadeInDuration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            battleTransition.alpha =
                Mathf.Lerp(
                    0f,
                    redAfterglowAlpha,
                    t
                );

            yield return null;
        }

        battleTransition.alpha =
            redAfterglowAlpha;


        // =====================
        // RED HOLD
        // =====================

        yield return
            new WaitForSecondsRealtime(
                redHoldDuration
            );


        // =====================
        // RED AFTERGLOW OUT
        // =====================

        timer = 0f;

        while (timer < redFadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / redFadeOutDuration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            battleTransition.alpha =
                Mathf.Lerp(
                    redAfterglowAlpha,
                    0f,
                    t
                );

            yield return null;
        }

        battleTransition.alpha = 0f;


        // =====================
        // BLACK FADE
        // =====================

        battleTransitionImage.color =
            Color.black;

        timer = 0f;

        while (timer < battleFadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / battleFadeDuration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            battleTransition.alpha = t;

            yield return null;
        }

        battleTransition.alpha = 1f;


        // =====================
        // BLACK HOLD
        // =====================

        yield return
            new WaitForSecondsRealtime(
                blackScreenDuration
            );


        // =====================
        // LOAD BATTLE
        // =====================

        SceneManager.LoadScene(
            "ShrineBattle"
        );
    }
}