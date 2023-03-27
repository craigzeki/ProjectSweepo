/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryQuestionManager : QuestionManager
{

    // SORT THESE FIELDS OUT LATER
    private Vector2Int startPoint;
    private Vector2Int currentOffset, currentPosition;
    private byte questionsDone = 0, questionsAnswered = 0;

    private const byte ALL_CORRECT_VALUE = 1;

    private bool AllQuestionsCorrect()
    { return questionsAnswered / questionsDone / locationsToMoveTo.Count == ALL_CORRECT_VALUE; }

    [SerializeField] private List<Vector2Int> locationsToMoveTo;

    [ContextMenu("Validate Locations")]
    private void ValidateLocations()
    {
        for (int i = locationsToMoveTo.Count - 1; i > 0; i--)
        {
            if (locationsToMoveTo[i].x > 10 || locationsToMoveTo[i].x < 0 || locationsToMoveTo[i].y > 10 || locationsToMoveTo[i].y < 0)
            {
                locationsToMoveTo.RemoveAt(i);
            }
        }
    }

    public override void CheckAnswer()
    {
        if (VerifyAnswer(startPoint, locationsToMoveTo[(int)questionsDone], currentOffset))
        {
            questionsAnswered++;
        }

        questionsDone++;

        startPoint = currentPosition;
    }

    public override void NextQuestion()
    {
        #region EDIT AND RELOCATE THIS IN METHOD
        //questionsCompleted++;

        // if (questionsCompleted == questionsToComplete.Count)
        // { DoSomething() }
        #endregion

    }

    protected override void ResetQuestions()
    { }

    private bool VerifyAnswer(Vector2Int startPoint, Vector2Int target, Vector2Int offset)
    {
        return startPoint + offset == target;
    }
}
*/