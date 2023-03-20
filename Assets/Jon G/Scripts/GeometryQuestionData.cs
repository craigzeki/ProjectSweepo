using UnityEngine;

public class GeometryQuestionData : MonoBehaviour
{

    [SerializeField] private Vector2Int startPosition;
    [SerializeField] private Vector2Int endPosition;
    private Vector2Int _currentOffset;

    public bool IsAtTargetPosition
    { get { return startPosition + _currentOffset == endPosition; } }

    public void OffsetTransformPosition(Vector2Int offset)
    { _currentOffset += offset; }
}