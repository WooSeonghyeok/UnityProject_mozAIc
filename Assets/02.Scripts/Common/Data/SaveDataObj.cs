using System;
using System.Collections.Generic;
[Serializable]
public class SaveDataObj
{
    /* 슬롯 데이터 */
    public byte ID;
    public string savedTime;

    /* 게임 진행 데이터 */
    public bool ep1_open;
    public bool ep1_isCaveUnlocked;
    public bool ep1_isPuzzleCleared;
    public bool ep2_open;
    public bool ep2_spaceClear;
    public bool ep2_paintClear;
    public bool ep3_open;
    public bool ep3_jumpClear;
    public bool ep3_paperClear;
    public bool ep4_open;
    public bool ep4_puzzle1Clear;
    public bool ep4_puzzle2Clear;
    public bool ep4_puzzle3Clear;
    public int[] memory_reconstruction_rate = new int[13];
    /* 0 : 기본 점수, 1~3 : Episode1, 4~6 : Episode2, 7~9 : Episode3, 10~12 : Episode4
                      1/4/7/10 : 관계 점수, 2/5/8/11 : 퍼즐 점수, 3/6/9/12 : 감정 점수*/
    public List<IsTagGet> CoreTag;    // 진 엔딩 태그
    public List<NPCInfo> npcInformations;

    /* 연출 사용 여부 확인 데이터 */
    public bool Played_Episode2_Intro;          //에피소드 2 첫 진입
    public bool Played_EP2_Text_Intro;          //에피소드 2 인트로
    public bool Played_Space_Intro;             //에피소드 2 Space 퍼즐 첫 진입
    public bool Played_Paint_Intro;             //에피소드 2 Paint 퍼즐 첫 진입
    public bool Played_Space_Text;              //에피소드 2 Space 퍼즐 중간 텍스트
    public bool Played_Space_Clear_Immediate;   //에피소드 2 Space 퍼즐 클리어 직후
    public bool Played_Paint_Sequences;         //에피소드 2 Paint 퍼즐 클리어 직후
    public bool Played_Space_Clear;             //에피소드 2 Space 퍼즐 클리어 후 복귀
    public bool Played_Paint_Clear;             //에피소드 2 Paint 퍼즐 클리어 후 복귀
    public bool Played_EP2_Ending;              //에피소드 2 엔딩
    public bool Played_EP3_LobbyIntro;          //에피소드 3 첫 로비 진입 컷씬
    public bool Played_EP3_Stage3_1Intro;       //에피소드 3-1 진입 컷씬
    public bool Played_EP3_Stage3_1Completion;  //에피소드 3-1 완료 연출 컷씬
    public bool Played_EP3_Stage3_2Intro;       //에피소드 3-2 진입 컷씬
    public bool Played_EP3_ReturnedLobbyIntro;  //에피소드 3-2 완료 후 복귀 로비 컷씬
    public bool isFirstEnterAtEP3Lobby;         //에피소드 3 첫 진입
    public bool isFirstEnterAtEP3_1;            //에피소드 3 악보 퍼즐 진입
    public bool isFirstEnterAtS3CP0;            //에피소드 4 첫 진입
}
[Serializable]
public class IsTagGet  // 진 엔딩 태그 정보(이름, 획득 여부)
{
    public string TagName;
    public bool tagGet;
}
[Serializable]
public class NPCInfo  // NPC 정보(이름, 친밀도, 대화 횟수, 기억 단어 리스트)
{
    public string npcId;
    public int Affinity;
    public int talkCount;
    public List<MemoryKeyword> words;
}
[Serializable]
public class MemoryKeyword  // NPC 기억 단어 정보(단어, 기억 재구성 점수, 사용 여부)
{
    public string word;
    public int memoryRate;
    public bool isUsed = false;
}
