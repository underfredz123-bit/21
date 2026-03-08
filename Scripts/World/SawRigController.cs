using UnityEngine;

public class SawRigController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer bladeRenderer;
    [SerializeField] private SpriteRenderer handRenderer;

    [Header("Rig")]
    [SerializeField] private Transform bladeTransform;
    [SerializeField] private Transform handPivot;
    [SerializeField] private Transform sawRoot;

    [Header("Track Anchors")]
    [SerializeField] private Transform playerAnchor;
    [SerializeField] private Transform opponentAnchor;
    [SerializeField] private float totalDistance = 15f;
    [SerializeField] private float startDistanceFromPlayer = 8f;

    [Header("Blade Motion")]
    [SerializeField] private float bladeSpinSpeed = 720f;

    [Header("Saw Move")]
    [SerializeField] private float positionSmooth = 4f;
    [SerializeField] private float handAngleNearPlayer = 15f;
    [SerializeField] private float handAngleNearOpponent = -15f;

    private float _currentDistanceFromPlayer;
    private float _targetDistanceFromPlayer;

    private void Awake()
    {
        ResetToStart();
    }

    private void Update()
    {
        if (bladeTransform != null)
        {
            bladeTransform.Rotate(0f, 0f, -bladeSpinSpeed * Time.deltaTime);
        }

        _currentDistanceFromPlayer = Mathf.Lerp(_currentDistanceFromPlayer, _targetDistanceFromPlayer, Time.deltaTime * positionSmooth);
        ApplyTransformFromDistance(_currentDistanceFromPlayer);
    }

    public void ResetToStart()
    {
        float clamped = Mathf.Clamp(startDistanceFromPlayer, 0f, totalDistance);
        _currentDistanceFromPlayer = clamped;
        _targetDistanceFromPlayer = clamped;
        ApplyTransformFromDistance(_currentDistanceFromPlayer);
    }

    public void MoveTowardPlayer(float amount)
    {
        SetTargetDistance(_targetDistanceFromPlayer - Mathf.Abs(amount));
    }

    public void MoveTowardOpponent(float amount)
    {
        SetTargetDistance(_targetDistanceFromPlayer + Mathf.Abs(amount));
    }

    public float DistanceToPlayer => _targetDistanceFromPlayer;
    public float DistanceToOpponent => totalDistance - _targetDistanceFromPlayer;

    public void SetVisuals(Sprite bladeSprite, Sprite handSprite)
    {
        if (bladeRenderer != null)
        {
            bladeRenderer.sprite = bladeSprite;
        }

        if (handRenderer != null)
        {
            handRenderer.sprite = handSprite;
        }
    }

    private void SetTargetDistance(float value)
    {
        _targetDistanceFromPlayer = Mathf.Clamp(value, 0f, totalDistance);
    }

    private void ApplyTransformFromDistance(float distanceFromPlayer)
    {
        if (playerAnchor == null || opponentAnchor == null || sawRoot == null)
        {
            return;
        }

        float t = totalDistance <= 0.001f ? 0f : distanceFromPlayer / totalDistance;
        sawRoot.position = Vector3.Lerp(playerAnchor.position, opponentAnchor.position, t);

        if (handPivot != null)
        {
            float angle = Mathf.Lerp(handAngleNearPlayer, handAngleNearOpponent, t);
            handPivot.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
