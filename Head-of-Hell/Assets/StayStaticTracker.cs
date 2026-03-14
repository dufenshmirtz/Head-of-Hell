using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class StaticStateTracker
{
    private class StaticCallInfo
    {
        public string characterName;
        public string reason;
        public float time;
        public int frame;
        public string stackTrace;
    }

    private static Dictionary<int, StaticCallInfo> lastCalls =
        new Dictionary<int, StaticCallInfo>();

    // Called whenever stayStatic() happens
    public static void Record(MonoBehaviour character, string reason = null)
    {
        if (character == null) return;

        var info = new StaticCallInfo
        {
            characterName = character.name,
            reason = string.IsNullOrEmpty(reason) ? "No reason provided" : reason,
            time = Time.time,
            frame = Time.frameCount,
            stackTrace = new StackTrace(2, true).ToString()
        };

        lastCalls[character.GetInstanceID()] = info;
    }

    public static void PrintLastInfo(MonoBehaviour character, string context = "stayStatic info")
    {
        if (character == null) return;

        if (!lastCalls.TryGetValue(character.GetInstanceID(), out var info))
        {
            UnityEngine.Debug.Log(
                $"[{context}] No stayStatic recorded for {character.name}",
                character
            );
            return;
        }

        UnityEngine.Debug.Log(
            "===================[t]=====================\n" +
            $"[{context}] {info.characterName}\n" +
            $"Reason: {info.reason}\n" +
            $"Time: {info.time:F3}\n" +
            $"Frame: {info.frame}\n" +
            $"StackTrace:\n{info.stackTrace}" +
            "========================================",
            character
        );
    }

    public static void Clear(MonoBehaviour character)
    {
        if (character == null) return;
        lastCalls.Remove(character.GetInstanceID());
    }
}