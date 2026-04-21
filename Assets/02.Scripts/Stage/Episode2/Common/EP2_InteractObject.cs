using UnityEngine;
using System.Collections;

public class EP2_InteractObject : MonoBehaviour
{
    private bool isUsed = false;
    readonly string playerTag = "Player";
    private PlayerInput user;
    public SaveDataObj CurData;
    bool isContact = false;

    [Header("Interaction Setting")]
    public float interactDistance = 3f;

    [Header("Interaction Effect")]
    public GameObject interactionEffectPrefab;
    private void Awake()
    {
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        CurData = SaveManager.instance.curData;
    }
    private void OnEnable()
    {
        if (user != null) user.Interact += Interact;
    }
    private void OnDisable()
    {
        if (user != null) user.Interact -= Interact;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isContact = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isContact = false;
        }
    }

    [Header("Glow Effect")]
    public GameObject glowEffect;

    [Header("Cutscene")]
    public string cutsceneName;

    private Transform playerTr;
    private PlayerInput playerInput;

    private void OnEnable()
    {
        TryFindPlayer();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTr = player.transform;
            playerInput = player.GetComponent<PlayerInput>();
        }
    }

    private void Subscribe()
    {
        if (playerInput != null)
        {
            playerInput.Interact += TryInteract;
        }
    }

    private void Unsubscribe()
    {
        if (playerInput != null)
        {
            playerInput.Interact -= TryInteract;
        }
    }

    private void TryInteract()
    {
        if (isUsed) return;
        if (playerTr == null) return;

        float distance = Vector3.Distance(playerTr.position, transform.position);

        if (distance > interactDistance) return;

        Interact();
    }

    public void Interact()
    {
        if (isUsed || !isContact) return;
        isUsed = true;
        Episode2ScoreManager.Instance?.AddInteractionScore(1);

        if (!string.IsNullOrEmpty(cutsceneName))
        {
            StartCoroutine(PlayAfterCutscene()); // ⭐ 핵심
        }
        else
        {
            PlayEffect();
        }

        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
        Debug.Log($"{gameObject.name} 상호작용 +1");
        GetComponent<Collider>().enabled = false;
        // 또는
        // gameObject.SetActive(false);
    }

    // ⭐ 컷씬 끝까지 기다렸다가 이펙트 실행
    IEnumerator PlayAfterCutscene()
    {
        yield return StartCoroutine(
            EP2CutsceneManager.Instance.PlayCutsceneAndWait(cutsceneName)
        );

        PlayEffect();
    }

    // ⭐ 이펙트 실행
    private void PlayEffect()
    {
        if (interactionEffectPrefab != null && playerTr != null)
        {
            GameObject effect = Instantiate(
                interactionEffectPrefab,
                playerTr.position + Vector3.up * 1.2f,
                Quaternion.identity
            );

            // ⭐ 여기 추가
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }

            Destroy(effect, 2f);
        }
    }
}