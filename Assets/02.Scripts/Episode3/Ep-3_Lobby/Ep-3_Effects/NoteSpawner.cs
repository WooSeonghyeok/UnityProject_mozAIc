using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 여섯 개의 노트를 랜덤으로 스폰 + 오브젝트 풀링 방식
public class NoteSpawner : MonoBehaviour
{
    [Header("대상")]
    [SerializeField] private Transform player;

    [Header("노트 프리팹")]
    [SerializeField] private GameObject[] notePrefabs;

    [Header("스폰 설정")]
    [SerializeField] private float spawnInterval = 0.2f;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnHeightOffset = .5f;

    [Header("최대 활성 개수 제한")]
    [SerializeField] private int maxNoteCount = 30;

    [Header("프리팹 종류당 풀 개수")]
    [SerializeField] private int poolSizePerType = 10;

    private float timer;

    // 프리팹 종류별 풀
    private List<GameObject>[] notePools;

    private void Awake()
    {
        CreateNotePools();
    }

    private void Update()
    {
        if (player == null) return;
        if (notePrefabs == null || notePrefabs.Length == 0) return;
        if (notePools == null) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;

            // 현재 활성화된 음표 수가 최대치보다 적을 때만 스폰
            if (GetCurrentActiveNoteCount() < maxNoteCount)
            {
                SpawnRandomNote();
            }
        }
    }

    private void CreateNotePools()
    {
        if (notePrefabs == null || notePrefabs.Length == 0)
        {
            Debug.LogWarning("NoteSpawner : notePrefabs가 비어 있습니다.");
            return;
        }

        notePools = new List<GameObject>[notePrefabs.Length];

        GameObject root = new GameObject("NotePools");

        for (int i = 0; i < notePrefabs.Length; i++)
        {
            notePools[i] = new List<GameObject>();

            if (notePrefabs[i] == null)
            {
                Debug.LogWarning($"NoteSpawner : notePrefabs[{i}] 가 비어 있습니다.");
                continue;
            }

            GameObject typeRoot = new GameObject($"{notePrefabs[i].name}_Pool");
            typeRoot.transform.SetParent(root.transform);

            for (int j = 0; j < poolSizePerType; j++)
            {
                GameObject note = Instantiate(notePrefabs[i], typeRoot.transform);
                note.name = $"{notePrefabs[i].name}_{j + 1}";
                note.SetActive(false);

                notePools[i].Add(note);
            }
        }
    }

    private void SpawnRandomNote()
    {
        // 랜덤 시작 인덱스
        int startIndex = Random.Range(0, notePrefabs.Length);

        // 한 종류가 꽉 차 있으면 다른 종류도 확인
        for (int offset = 0; offset < notePrefabs.Length; offset++)
        {
            int noteIndex = (startIndex + offset) % notePrefabs.Length;

            GameObject note = GetInactiveNote(noteIndex);

            if (note != null)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

                Vector3 spawnPos = new Vector3(
                    player.position.x + randomCircle.x,
                    player.position.y + spawnHeightOffset,
                    player.position.z + randomCircle.y
                );

                note.transform.position = spawnPos;
                note.transform.rotation = Quaternion.identity;
                note.SetActive(true);
                return;
            }
        }
    }

    private GameObject GetInactiveNote(int noteIndex)
    {
        if (notePools == null) return null;
        if (noteIndex < 0 || noteIndex >= notePools.Length) return null;
        if (notePools[noteIndex] == null) return null;

        foreach (GameObject note in notePools[noteIndex])
        {
            if (note != null && note.activeSelf == false)
            {
                return note;
            }
        }

        return null;
    }

    private int GetCurrentActiveNoteCount()
    {
        int count = 0;

        if (notePools == null) return 0;

        foreach (List<GameObject> pool in notePools)
        {
            if (pool == null) continue;

            foreach (GameObject note in pool)
            {
                if (note != null && note.activeSelf)
                {
                    count++;
                }
            }
        }

        return count;
    }
}