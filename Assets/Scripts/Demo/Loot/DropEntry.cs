using System;
using UnityEngine;

[Serializable]
public class DropEntry
{
    public string itemId;
    [Min(1)] public int minAmount = 1;
    [Min(1)] public int maxAmount = 1;

    [Range(0f, 1f)]
    public float chance = 1f; // 0.25 = %25 ihtimal
}