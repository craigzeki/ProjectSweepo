using UnityEngine;

[RequireComponent(typeof(SaveableObject))]
public class GeometryQuestion : MonoBehaviour
{
    [SerializeField] public GeometryQuestionData geometryQuestionData = new GeometryQuestionData();
}