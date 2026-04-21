using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EP3LobbyIntroSequence", menuName = "Episode3/EP3 Lobby Intro Sequence")]
public class Ep3LobbyIntroSequenceAsset : ScriptableObject
{
    public string sequenceId = "EP3_LOBBY_INTRO";
    public List<Ep3LobbyIntroShotData> shots = new List<Ep3LobbyIntroShotData>();

    public Ep3LobbyIntroSequenceData ToSequenceData()
    {
        Ep3LobbyIntroSequenceData sequence = new Ep3LobbyIntroSequenceData
        {
            sequenceId = sequenceId,
            shots = new List<Ep3LobbyIntroShotData>(shots.Count)
        };

        foreach (Ep3LobbyIntroShotData shot in shots)
        {
            if (shot == null)
            {
                continue;
            }

            sequence.shots.Add(shot.Clone());
        }

        return sequence;
    }
}
