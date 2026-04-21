using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//SpawnArray라는 이름의 스폰 포인트 배열안에 스폰포인트들에게 악보 조각들을 스폰한다,
//스폰 할 때 231개의 배열들 중에서 랜덤하게 스폰되게 한다.
//또한 중복된 스폰포인트에 스폰이 되지 않도록 하고,
//10종류의 악보 조각 프리팹을 각각 하나씩 스폰한다.
public class PieceSpawner : MonoBehaviour
{
    [Header("스폰 배열 오브젝트")]
    public Transform spawnArray;
    [Header("악보 조각 프리팹(배열 선언)")]
    public GameObject[] piecePrefabs;
    [Header("스폰할 수량")]
    private int spawnCount = 10;
    [Header("옵션")]
    private bool randomSpawn = true;
    private bool useSpawnPoint = true;
    [Header("스폰 회전")]
    [Tooltip("생성 시 X축으로 추가 회전(도 단위). 기본값 90")]
    public float spawnRotationX = 90f;

    [Header("악보 조각 근접 힌트 사운드")]
    [SerializeField] private bool useProximityHintAudio = false;
    [SerializeField] private AudioClip proximityHintClip;
    [SerializeField] private float hintMinDistance = 2f;
    [SerializeField] private float hintMaxDistance = 18f;
    [SerializeField] private float hintMinVolume = 0f;
    [SerializeField] private float hintMaxVolume = 0.85f;
    [SerializeField] private float hintVolumeChangeSpeed = 1.5f;
    [SerializeField] private bool hintStopWhenFar = false;
    [SerializeField] private bool hintLoop = true;
    [SerializeField] private bool hintPlayOnStart = true;
    [SerializeField] private bool hintRandomStartTime = true;
    [SerializeField, Range(0f, 1f)] private float hintSpatialBlend = 1f;
    [SerializeField, Range(0f, 360f)] private float hintSpread = 0f;
    [SerializeField] private AudioRolloffMode hintRolloffMode = AudioRolloffMode.Linear;
    [SerializeField] private float hintDopplerLevel = 0f;

    private List<Transform> spawnPoints = new List<Transform>();  //스폰 포인트 리스트
    private void Start()
    {
        GetSpawnPoints();
        SpawnPieces();
    }
    // SpawnArray 아래 자식들을 전부 SpawnPoint로 사용
    private void GetSpawnPoints()
    {
        spawnPoints.Clear();
        if (spawnArray == null)
        {
            Debug.LogWarning("SpawnArray가 비어 있습니다.");
            return;
        }
        for (int i = 0; i < spawnArray.childCount; i++)
        {
            spawnPoints.Add(spawnArray.GetChild(i));
        }
    }
    private void SpawnPieces()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("스폰 포인트가 없습니다.");
            return;
        }
        if (piecePrefabs == null || piecePrefabs.Length == 0)
        {
            Debug.LogWarning("악보 조각 프리팹이 없습니다.");
            return;
        }
        // 실제 생성 개수는 "스폰포인트 개수", "프리팹 개수", "spawnCount" 중 가장 작은 값
        int finalCount = Mathf.Min(spawnCount, spawnPoints.Count, piecePrefabs.Length);
        // 스폰 포인트 섞기
        Shuffle(spawnPoints);
        // 프리팹 배열을 리스트로 복사해서 필요하면 섞기
        List<GameObject> pieceList = new List<GameObject>(piecePrefabs  );
        if (randomSpawn)
        {
            Shuffle(pieceList);
        }
        // 앞에서 finalCount개만 사용
        for (int i = 0; i < finalCount; i++)
        {
            Transform point = spawnPoints[i];
            GameObject prefab = pieceList[i];
            // 인스펙터에 연결된 포인트 회전 값에 X축 90도(또는 spawnRotationX) 추가 적용
            Quaternion additional = Quaternion.Euler(spawnRotationX, 0f, 0f);
            Quaternion rot = useSpawnPoint ? point.rotation * additional : additional;
            // 생성한 오브젝트를 해당 스폰 포인트의 자식으로 넣기 위해 parent 파라미터 사용
            GameObject instance = Instantiate(prefab, point.position, rot, point);
            ConfigureProximityHintAudio(instance);
            // 필요하면 로컬 포지션/로컬 회전 초기화 (부모에 맞추고 싶을 때)
            // instance.transform.localPosition = Vector3.zero;
            // instance.transform.localRotation = Quaternion.identity;
        }
    }

    private void ConfigureProximityHintAudio(GameObject instance)
    {
        if (!useProximityHintAudio || proximityHintClip == null || instance == null)
        {
            return;
        }

        AudioHint audioHint = instance.GetComponent<AudioHint>();
        if (audioHint == null)
        {
            audioHint = instance.AddComponent<AudioHint>();
        }

        audioHint.Configure(
            proximityHintClip,
            null,
            hintMinDistance,
            hintMaxDistance,
            hintMinVolume,
            hintMaxVolume,
            hintVolumeChangeSpeed,
            hintStopWhenFar,
            hintLoop,
            hintPlayOnStart,
            hintRandomStartTime,
            hintSpatialBlend,
            hintSpread,
            hintRolloffMode,
            hintDopplerLevel
        );
    }

    // 리스트 섞기용 함수 (Fisher-Yates Shuffle)
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
