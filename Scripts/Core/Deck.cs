using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    private readonly List<int> _availableValues = new List<int>();

    public Deck()
    {
        Reset();
    }

    public void Reset()
    {
        _availableValues.Clear();
        for (int value = 1; value <= 11; value++)
        {
            _availableValues.Add(value);
        }
    }

    public bool IsEmpty => _availableValues.Count == 0;
    public int AvailableCount => _availableValues.Count;

    public int DrawRandom()
    {
        if (_availableValues.Count == 0)
        {
            Debug.LogWarning("Deck is empty.");
            return -1;
        }

        int index = Random.Range(0, _availableValues.Count);
        int value = _availableValues[index];
        _availableValues.RemoveAt(index);
        return value;
    }

    public int DrawSpecific(int value)
    {
        if (_availableValues.Remove(value))
        {
            return value;
        }

        Debug.LogWarning($"Card value {value} is not available.");
        return -1;
    }

    public void ReturnCard(int value)
    {
        if (_availableValues.Contains(value))
        {
            return;
        }

        _availableValues.Add(value);
    }

    public bool HasValue(int value)
    {
        return _availableValues.Contains(value);
    }
}
