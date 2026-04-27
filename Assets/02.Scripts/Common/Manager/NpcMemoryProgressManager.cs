using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class NpcMemoryProgressManager : MonoBehaviour
    {
        public static NpcMemoryProgressManager Instance;

        public enum EpisodeType
        {
            Episode2,
            Episode3
        }
        [Header("대상 에피소드")]
        [SerializeField] private EpisodeType episodeType = EpisodeType.Episode2;

        [Header("연결할 NPC")]
        [SerializeField] private NPCData targetNpcData;

        [Header("자동 탐색용 NPC ID")]
        [SerializeField] private string targetNpcId = "npc_ep2_painter";

        [Header("기억 단계 설정")]
        [SerializeField] private MemoryRevealStage startStage = MemoryRevealStage.FaintFeeling;
        [SerializeField] private MemoryRevealStage midStage = MemoryRevealStage.Partial;
        [SerializeField] private MemoryRevealStage clearStage = MemoryRevealStage.Full;

        [Header("SceneId 설정")]
        [SerializeField] private string defaultSceneId = "ep_02_studio";
        [SerializeField] private string oneClearSceneId = "ep_02_one_puzzle_cleared";
        [SerializeField] private string allClearSceneId = "ep_02_all_puzzle_cleared";

        private bool triedBind = false;

        private void Awake()
        {
            // 싱글톤 초기화
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            TryBindNpcData();
            // 시작 단계 적용
            if (targetNpcData == null)
            {
                Debug.LogWarning("[NpcMemory] NPC 연결 안됨");
                return;
            }
            //  SaveManager에 저장된 EP2 진행 상태를 EP2ProgressData로 복원
            if (SaveManager.instance != null)
            {
                EP2ProgressData.spaceClear = SaveManager.instance.curData.ep2_spaceClear;
                EP2ProgressData.paintClear = SaveManager.instance.curData.ep2_paintClear;

                Debug.Log($"[NpcMemory] SaveManager 복원 - space:{EP2ProgressData.spaceClear}, paint:{EP2ProgressData.paintClear}");
            }
            // 게임 시작 또는 ep2로비 진입 시 기본 시작 단계 설정
            targetNpcData.SetRevealStage(startStage);

            // 시작 시 현재 세이브 기준 진행 상태 반영
            ApplyProgress();
            // ep2로비에 돌아왔을 때 진행 상태 반영
            //ApplyEpisode2Progress();
        }
        private void Update()
        {
            if (!triedBind && targetNpcData == null)
            {
                triedBind = true;
                TryBindNpcData();

                if (targetNpcData != null)
                {
                    Debug.Log("[NpcMemory] 늦은 바인딩 성공");

                    //ApplyEpisode2Progress();

                }
            }
        }
        // scene 안의 NPCData 중 targetNpcId와 일치하는 NPC를 자동 연결
        private void TryBindNpcData()
        {
            if (targetNpcData != null)
                return;

            NPCData[] all = FindObjectsOfType<NPCData>(true);

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].npcId == targetNpcId)
                {
                    targetNpcData = all[i];
                    Debug.Log($"[NpcMemory] 자동 연결 성공: {targetNpcData.name}");
                    return;
                }
            }

            Debug.LogWarning("[NpcMemory] NPC 자동 연결 실패");
        }
        // 외부에서 퍼즐 클리어 직후 호출할 공용 함수
        public void OnPuzzleStateChanged()
        {
            ApplyProgress();
        }
        public void ApplyProgress()
        {
            if (targetNpcData == null)
            {
                TryBindNpcData();
                Debug.LogWarning("[NpcMemory] targetNpcData가 없어 진행 상태 반영 실패");
                return;
            }

            SaveDataObj curData = null;

            if (SaveManager.instance != null)
            {
                curData = SaveManager.instance.curData;
            }

            if (curData == null)
            {
                curData = SaveManager.ReadCurJSON();
            }

            if (curData == null)
            {
                Debug.LogWarning("[NpcMemory] SaveData를 읽지 못함");
                return;
            }

            int clearCount = GetClearCount(curData);

            Debug.Log($"[NpcMemory] Apply 호출 - episode:{episodeType}, clearCount:{clearCount}, current:{targetNpcData.revealStage}");

            // 퍼즐 2개 모두 클리어
            if (clearCount >= 2)
            {
                targetNpcData.sceneId = allClearSceneId;
                targetNpcData.SetRevealStage(clearStage);
                targetNpcData.RefreshPrompt();

                Debug.Log($"[NpcMemory] 전체 클리어 반영: {clearStage}, sceneId:{targetNpcData.sceneId}");
                return;
            }

            // 퍼즐 1개 클리어
            if (clearCount == 1)
            {
                targetNpcData.sceneId = oneClearSceneId;
                targetNpcData.SetRevealStage(midStage);
                targetNpcData.RefreshPrompt();

                Debug.Log($"[NpcMemory] 일부 클리어 반영: {midStage}, sceneId:{targetNpcData.sceneId}");
                return;
            }

            // 퍼즐 0개 클리어
            targetNpcData.sceneId = defaultSceneId;
            targetNpcData.SetRevealStage(startStage);
            targetNpcData.RefreshPrompt();

            Debug.Log($"[NpcMemory] 미클리어 상태 유지: {startStage}, sceneId:{targetNpcData.sceneId}");
        }

        /// <summary>
        /// 에피소드별 퍼즐 클리어 개수 계산
        /// </summary>
        private int GetClearCount(SaveDataObj curData)
        {
            int count = 0;

            switch (episodeType)
            {
                case EpisodeType.Episode2:
                    if (curData.ep2_spaceClear) count++;
                    if (curData.ep2_paintClear) count++;
                    break;

                case EpisodeType.Episode3:
                    if (curData.ep3_jumpClear) count++;
                    if (curData.ep3_paperClear) count++;
                    break;
            }

            return count;
        }
        
        // EP2 진행 상태를 읽어서 NPC 기억 단계를 반영
        //public void ApplyEpisode2Progress()
        //{
        //    Debug.Log($"[NpcMemory] Apply 호출 - space:{EP2ProgressData.spaceClear}, paint:{EP2ProgressData.paintClear}, current:{targetNpcData.revealStage}");
        //    if (targetNpcData == null)
        //    {
        //        TryBindNpcData();
        //        Debug.LogWarning("[NpcMemoryProgressManager] targetNpcData가 없어 진행 상태 반영 실패");
        //        return;
        //    }
        //    // 두 퍼즐 모두 클리어한 경우 전체 클리어 단계 반영
        //    if (EP2ProgressData.spaceClear && EP2ProgressData.paintClear)
        //    {
        //        // 모든 퍼즐 클리어 문맥으로 sceneId 변경
        //        targetNpcData.sceneId = allClearSceneId;

        //        // 기억 단계 최종 반영
        //        targetNpcData.SetRevealStage(clearStage);

        //        // 프롬프트 즉시 갱신
        //        targetNpcData.RefreshPrompt();

        //        Debug.Log($"[NpcMemoryProgressManager] EP2 전체 클리어 반영: {clearStage}, sceneId:{targetNpcData.sceneId}");
        //        return;
        //    }

        //    // 하나만 클리어한 경우 중간 단계 반영
        //    if (EP2ProgressData.spaceClear || EP2ProgressData.paintClear)
        //    {
        //        //  하나 클리어 문맥으로 sceneId 변경
        //        targetNpcData.sceneId = oneClearSceneId;

        //        // 기억 단계 중간 반영
        //        targetNpcData.SetRevealStage(midStage);

        //        // 프롬프트 즉시 갱신
        //        targetNpcData.RefreshPrompt();

        //        Debug.Log($"[NpcMemoryProgressManager] EP2 일부 클리어 반영: {midStage}, sceneId:{targetNpcData.sceneId}");
        //        return;
        //    }

        //    // 아무것도 안 한 경우
        //    targetNpcData.sceneId = defaultSceneId;
        //    targetNpcData.SetRevealStage(startStage);
        //    targetNpcData.RefreshPrompt();

        //    Debug.Log($"[NpcMemoryProgressManager] EP2 미클리어 상태 유지: {startStage}, sceneId:{targetNpcData.sceneId}");
        //}
    }