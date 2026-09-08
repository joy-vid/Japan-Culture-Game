using System.Collections;
using UnityEngine;
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
    [Header("Dialogue UI")]
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    [Header("Portraits")]
    public CanvasGroup aikoPortrait;
    public CanvasGroup oniPortrait;

    [Header("Dialogue")]
    public DialogueBlock[] dialogueBlocks;

    [Header("Portrait Settings")]
    public float portraitFadeDuration = 0.2f;

    [Header("Typing Settings")]
    public float typingSpeed = 0.025f;

    public AudioSource typingAudioSource;
    public AudioClip typingSound;

    public int soundEveryCharacters = 2;

    [Header("Next Indicator")]
    public GameObject nextIndicator;

    public float nextPulseSpeed = 3f;
    public float nextPulseAmount = 0.08f;


    private int blockIndex = 0;
    private int lineIndex = 0;

    private Speaker? currentSpeaker = null;

    private Coroutine portraitCoroutine;
    private Coroutine typingCoroutine;
    private Coroutine nextPulseCoroutine;

    private bool isTyping = false;

    private string currentLine;

    private Vector3 nextIndicatorBaseScale;


    void Start()
    {
        // Portrait awal dibuat invisible
        aikoPortrait.alpha = 0f;
        oniPortrait.alpha = 0f;

        // Simpan ukuran awal indicator
        nextIndicatorBaseScale =
            nextIndicator.transform.localScale;

        // Indicator awal disembunyikan
        nextIndicator.SetActive(false);

        ShowDialogue();
    }


    public void NextDialogue()
    {
        // Kalau text masih mengetik,
        // klik hanya menyelesaikan text
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        HideNextIndicator();

        // Pindah ke line berikutnya
        lineIndex++;

        // Kalau masih ada line dalam block yang sama
        if (lineIndex < dialogueBlocks[blockIndex].lines.Length)
        {
            ShowDialogue();
            return;
        }

        // Kalau block sudah selesai,
        // pindah ke block / speaker berikutnya
        blockIndex++;
        lineIndex = 0;

        // Kalau semua dialogue selesai
        if (blockIndex >= dialogueBlocks.Length)
        {
            SceneManager.LoadScene("ShrineBattle");
            return;
        }

        ShowDialogue();
    }


    private void ShowDialogue()
    {
        DialogueBlock currentBlock =
            dialogueBlocks[blockIndex];

        // Update nama speaker
        nameText.text = currentBlock.displayName;

        // Ganti portrait hanya kalau speakernya berubah
        if (currentSpeaker != currentBlock.speaker)
        {
            currentSpeaker = currentBlock.speaker;

            ChangePortrait(currentBlock.speaker);
        }

        // Mulai typing dialogue
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
            StartCoroutine(TypeDialogue(line));
    }


    private IEnumerator TypeDialogue(string line)
    {
        isTyping = true;

        // Masukkan seluruh text terlebih dahulu
        dialogueText.text = line;

        // Tapi sembunyikan semua karakter
        dialogueText.maxVisibleCharacters = 0;

        dialogueText.ForceMeshUpdate();

        int totalCharacters =
            dialogueText.textInfo.characterCount;

        int soundCounter = 0;

        for (int i = 0; i < totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;

            char currentCharacter =
                dialogueText.textInfo.characterInfo[i].character;

            // Jangan bunyi kalau spasi
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
                new WaitForSecondsRealtime(typingSpeed);
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
            speaker == Speaker.Aiko ? 1f : 0f;

        float targetOni =
            speaker == Speaker.Oni ? 1f : 0f;

        float timer = 0f;

        while (timer < portraitFadeDuration)
        {
            timer += Time.unscaledDeltaTime;

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
}