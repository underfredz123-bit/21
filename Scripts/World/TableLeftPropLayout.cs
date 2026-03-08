using UnityEngine;

public class TableLeftPropLayout : MonoBehaviour
{
    [SerializeField] private Transform tableAnchor;
    [SerializeField] private Transform sawRoot;
    [SerializeField] private Transform counterRoot;

    [Header("Offsets from table anchor")]
    [SerializeField] private Vector3 sawOffset = new Vector3(-2.2f, 0.75f, 0f);
    [SerializeField] private Vector3 counterOffset = new Vector3(-2.8f, 0.3f, 0f);

    [Header("Rotation")]
    [SerializeField] private float sawZRotation = 0f;
    [SerializeField] private float counterZRotation = 0f;

    private void Start()
    {
        ApplyLayout();
    }

    private void OnValidate()
    {
        ApplyLayout();
    }

    [ContextMenu("Apply Layout")]
    public void ApplyLayout()
    {
        if (tableAnchor == null)
        {
            return;
        }

        if (sawRoot != null)
        {
            sawRoot.position = tableAnchor.TransformPoint(sawOffset);
            sawRoot.rotation = Quaternion.Euler(0f, 0f, sawZRotation);
        }

        if (counterRoot != null)
        {
            counterRoot.position = tableAnchor.TransformPoint(counterOffset);
            counterRoot.rotation = Quaternion.Euler(0f, 0f, counterZRotation);
        }
    }
}
