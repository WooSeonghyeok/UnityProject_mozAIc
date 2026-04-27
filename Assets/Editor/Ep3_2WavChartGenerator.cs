using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class Ep3_2WavChartGenerator
{
    private const int FrameSize = 1024;
    private const int HopSize = 256;
    private const float MinGapSeconds = 0.11f;
    private const float SyncAdvanceSeconds = 0.035f;

    public static bool GenerateFromManager(RhythmAudioManager manager)
    {
        if (manager == null)
        {
            Debug.LogWarning("[Ep3_2WavChartGenerator] RhythmAudioManager가 없습니다.");
            return false;
        }

        AudioClip audioClip = manager.AudioClip;
        BeatMapData beatMapAsset = manager.TopDownBeatMapAsset;

        if (audioClip == null)
        {
            Debug.LogWarning("[Ep3_2WavChartGenerator] audioClip이 비어 있습니다.");
            return false;
        }

        if (beatMapAsset == null)
        {
            Debug.LogWarning("[Ep3_2WavChartGenerator] topDownBeatMapAsset이 비어 있습니다.");
            return false;
        }

        string assetPath = AssetDatabase.GetAssetPath(audioClip);
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            Debug.LogWarning("[Ep3_2WavChartGenerator] 오디오 클립 경로를 찾지 못했습니다.");
            return false;
        }

        string fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[Ep3_2WavChartGenerator] WAV 파일이 존재하지 않습니다: {fullPath}");
            return false;
        }

        if (!TryReadWav(fullPath, out float[] samples, out int sampleRate))
        {
            Debug.LogWarning("[Ep3_2WavChartGenerator] WAV 파형 읽기에 실패했습니다. 16-bit PCM WAV인지 확인해 주세요.");
            return false;
        }

        List<float> onsetTimes = DetectOnsets(samples, sampleRate, manager.StartOffset, audioClip.length);
        if (onsetTimes.Count == 0)
        {
            Debug.LogWarning("[Ep3_2WavChartGenerator] 검출된 온셋이 없습니다.");
            return false;
        }

        List<float> pitchValues = new List<float>(onsetTimes.Count);
        for (int i = 0; i < onsetTimes.Count; i++)
        {
            int startSample = Mathf.Clamp(Mathf.FloorToInt(onsetTimes[i] * sampleRate), 0, samples.Length - 1);
            float pitch = EstimatePitch(samples, sampleRate, startSample);
            if (pitch > 0f)
            {
                pitchValues.Add(pitch);
            }
        }

        pitchValues.Sort();

        Undo.RecordObject(beatMapAsset, "Generate Episode3-2 WAV Chart");
        beatMapAsset.topDownChartNotes.Clear();

        int previousLane = 2;
        for (int i = 0; i < onsetTimes.Count; i++)
        {
            float onsetTime = Mathf.Max(manager.StartOffset, onsetTimes[i] - SyncAdvanceSeconds);
            int startSample = Mathf.Clamp(Mathf.FloorToInt(onsetTimes[i] * sampleRate), 0, samples.Length - 1);
            float pitch = EstimatePitch(samples, sampleRate, startSample);
            int lane = MapPitchToLane(pitch, pitchValues, previousLane);

            TopDownChartNote note = new TopDownChartNote
            {
                judgeTimeSeconds = (float)Math.Round(onsetTime, 3),
                judgeWindowOverride = 0f,
                laneType = (Ep3_2LaneType)lane,
                isHoldNote = false,
                holdDurationSeconds = 0f,
                memo = i % 16 == 0 ? $"AUTO-{i / 16 + 1}" : string.Empty
            };

            beatMapAsset.topDownChartNotes.Add(note);
            previousLane = lane;
        }

        EditorUtility.SetDirty(beatMapAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[Ep3_2WavChartGenerator] WAV 기반 초안 채보 생성 완료 - notes={beatMapAsset.topDownChartNotes.Count}, clip={audioClip.name}");
        return true;
    }

    private static bool TryReadWav(string fullPath, out float[] monoSamples, out int sampleRate)
    {
        monoSamples = null;
        sampleRate = 0;

        byte[] bytes = File.ReadAllBytes(fullPath);
        if (bytes.Length < 44)
        {
            return false;
        }

        using MemoryStream stream = new MemoryStream(bytes);
        using BinaryReader reader = new BinaryReader(stream);

        if (new string(reader.ReadChars(4)) != "RIFF")
        {
            return false;
        }

        reader.ReadUInt32();
        if (new string(reader.ReadChars(4)) != "WAVE")
        {
            return false;
        }

        int channelCount = 0;
        int bitsPerSample = 0;
        long dataPosition = -1;
        int dataSize = 0;

        while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
        {
            string chunkId = new string(reader.ReadChars(4));
            int chunkSize = reader.ReadInt32();

            if (chunkId == "fmt ")
            {
                ushort audioFormat = reader.ReadUInt16();
                channelCount = reader.ReadUInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadUInt16();
                bitsPerSample = reader.ReadUInt16();

                if (audioFormat != 1)
                {
                    return false;
                }

                int remaining = chunkSize - 16;
                if (remaining > 0)
                {
                    reader.BaseStream.Seek(remaining, SeekOrigin.Current);
                }
            }
            else if (chunkId == "data")
            {
                dataPosition = reader.BaseStream.Position;
                dataSize = chunkSize;
                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
            }

            if ((chunkSize & 1) == 1)
            {
                reader.BaseStream.Seek(1, SeekOrigin.Current);
            }
        }

        if (dataPosition < 0 || sampleRate <= 0 || channelCount <= 0 || bitsPerSample != 16)
        {
            return false;
        }

        int bytesPerSample = bitsPerSample / 8;
        int frameBytes = channelCount * bytesPerSample;
        int sampleCount = dataSize / frameBytes;
        monoSamples = new float[sampleCount];

        reader.BaseStream.Position = dataPosition;
        for (int i = 0; i < sampleCount; i++)
        {
            float mixed = 0f;
            for (int ch = 0; ch < channelCount; ch++)
            {
                mixed += reader.ReadInt16() / 32768f;
            }

            monoSamples[i] = mixed / channelCount;
        }

        return true;
    }

    private static List<float> DetectOnsets(float[] samples, int sampleRate, float startOffset, float clipLength)
    {
        int frameCount = Mathf.Max(0, (samples.Length - FrameSize) / HopSize);
        if (frameCount <= 0)
        {
            return new List<float>();
        }

        float[] novelty = new float[frameCount];
        float previousLogEnergy = 0f;

        for (int frame = 0; frame < frameCount; frame++)
        {
            int start = frame * HopSize;
            double energy = 0d;

            float prev = samples[start];
            for (int i = 0; i < FrameSize && start + i < samples.Length; i++)
            {
                float current = samples[start + i];
                float highPass = i == 0 ? current : current - prev;
                energy += highPass * highPass;
                prev = current;
            }

            float logEnergy = Mathf.Log10((float)energy + 1e-9f);
            novelty[frame] = Mathf.Max(0f, logEnergy - previousLogEnergy);
            previousLogEnergy = logEnergy;
        }

        SmoothInPlace(novelty, 2);

        float average = 0f;
        for (int i = 0; i < novelty.Length; i++)
        {
            average += novelty[i];
        }

        average /= novelty.Length;

        float variance = 0f;
        for (int i = 0; i < novelty.Length; i++)
        {
            float delta = novelty[i] - average;
            variance += delta * delta;
        }

        float standardDeviation = Mathf.Sqrt(variance / novelty.Length);
        float threshold = average + standardDeviation * 1.05f;
        int minGapFrames = Mathf.Max(1, Mathf.RoundToInt(MinGapSeconds * sampleRate / HopSize));

        List<float> onsetTimes = new List<float>();
        int lastAcceptedFrame = -minGapFrames;

        for (int frame = 1; frame < novelty.Length - 1; frame++)
        {
            if (novelty[frame] < threshold)
            {
                continue;
            }

            if (novelty[frame] < novelty[frame - 1] || novelty[frame] <= novelty[frame + 1])
            {
                continue;
            }

            float onsetTime = frame * HopSize / (float)sampleRate;
            if (onsetTime < startOffset || onsetTime > clipLength - 0.4f)
            {
                continue;
            }

            if (frame - lastAcceptedFrame < minGapFrames)
            {
                continue;
            }

            onsetTimes.Add((float)Math.Round(onsetTime, 3));
            lastAcceptedFrame = frame;
        }

        return onsetTimes;
    }

    private static void SmoothInPlace(float[] values, int radius)
    {
        float[] copy = new float[values.Length];
        Array.Copy(values, copy, values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            float sum = 0f;
            int count = 0;
            for (int offset = -radius; offset <= radius; offset++)
            {
                int index = i + offset;
                if (index < 0 || index >= copy.Length)
                {
                    continue;
                }

                sum += copy[index];
                count++;
            }

            values[i] = count > 0 ? sum / count : copy[i];
        }
    }

    private static float EstimatePitch(float[] samples, int sampleRate, int startSample)
    {
        int windowLength = Mathf.Min(Mathf.FloorToInt(sampleRate * 0.12f), samples.Length - startSample);
        if (windowLength <= sampleRate / 50)
        {
            return -1f;
        }

        int minLag = Mathf.Max(20, sampleRate / 900);
        int maxLag = Mathf.Min(windowLength - 1, sampleRate / 140);
        if (maxLag <= minLag)
        {
            return -1f;
        }

        double bestCorrelation = double.MinValue;
        int bestLag = minLag;

        for (int lag = minLag; lag <= maxLag; lag++)
        {
            double correlation = 0d;
            for (int i = 0; i < windowLength - lag; i++)
            {
                correlation += samples[startSample + i] * samples[startSample + i + lag];
            }

            if (correlation > bestCorrelation)
            {
                bestCorrelation = correlation;
                bestLag = lag;
            }
        }

        return bestLag > 0 ? sampleRate / (float)bestLag : -1f;
    }

    private static int MapPitchToLane(float pitch, List<float> sortedPitchValues, int previousLane)
    {
        if (pitch <= 0f || sortedPitchValues == null || sortedPitchValues.Count < 5)
        {
            return Mathf.Clamp(previousLane, 0, 4);
        }

        float q20 = GetPercentile(sortedPitchValues, 0.2f);
        float q40 = GetPercentile(sortedPitchValues, 0.4f);
        float q60 = GetPercentile(sortedPitchValues, 0.6f);
        float q80 = GetPercentile(sortedPitchValues, 0.8f);

        int lane = pitch < q20 ? 0 :
            pitch < q40 ? 1 :
            pitch < q60 ? 2 :
            pitch < q80 ? 3 : 4;

        if (Mathf.Abs(lane - previousLane) > 2)
        {
            lane = previousLane + Math.Sign(lane - previousLane) * 2;
        }

        return Mathf.Clamp(lane, 0, 4);
    }

    private static float GetPercentile(List<float> values, float percentile)
    {
        if (values == null || values.Count == 0)
        {
            return 0f;
        }

        float clamped = Mathf.Clamp01(percentile);
        float scaledIndex = (values.Count - 1) * clamped;
        int lowerIndex = Mathf.FloorToInt(scaledIndex);
        int upperIndex = Mathf.Min(values.Count - 1, lowerIndex + 1);
        float t = scaledIndex - lowerIndex;
        return Mathf.Lerp(values[lowerIndex], values[upperIndex], t);
    }
}
