using System.Collections.Generic;
using UnityEngine;

public class TrumpHandView : MonoBehaviour
{
    [SerializeField] private TrumpCardView trumpPrefab;
    [SerializeField] private Transform container;

    private readonly List<TrumpCardView> _spawned = new List<TrumpCardView>();

    public void Refresh(List<TrumpCard> trumps, CardArtLibrary art)
    {
        if (trumpPrefab == null || container == null)
        {
            return;
        }

        Clear();

        foreach (TrumpCard trump in trumps)
        {
            TrumpCardView view = Instantiate(trumpPrefab, container);
            view.Initialize(trump, art);
            _spawned.Add(view);
        }
    }

    public void Clear()
    {
        foreach (TrumpCardView view in _spawned)
        {
            if (view != null)
            {
                Destroy(view.gameObject);
            }
        }

        _spawned.Clear();
    }
}
