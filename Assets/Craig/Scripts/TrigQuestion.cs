using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveableObject))]
public class TrigQuestion : MonoBehaviour
{
    [SerializeField] public TrigQuestionData trigQuestionData = new TrigQuestionData();
}
