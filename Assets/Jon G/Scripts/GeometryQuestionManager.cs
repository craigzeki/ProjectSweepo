using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryQuestionManager : QuestionManager
{

    // SORT THESE FIELDS OUT LATER
    //private Vector2Int startPoint;
    //private Vector2Int currentOffset, currentPosition;
    private byte questionsDone = 0, questionsAnswered = 0;

    public RectTransform moveablePoint;
    private Vector2Int currentPosition;

    private Vector2Int CurrentPosition
    {
        get
        {
            if (currentPosition.x > 10)
            { currentPosition.x = 10; }
            else if (currentPosition.x < 0)
            { currentPosition.x = 0; }

            if (currentPosition.y > 10)
            { currentPosition.y = 10; }
            else if (currentPosition.y < 0)
            { currentPosition.y = 0; }

            return currentPosition;
        }
    }

    private const byte ALL_CORRECT_VALUE = 1;

    private bool AllQuestionsCorrect()
    { return questionsAnswered / questionsDone / locationsToMoveTo.Count == ALL_CORRECT_VALUE; }

    [SerializeField] [HideInInspector] private List<Vector2Int> locationsToMoveTo;

    [SerializeField] private int numberOfQuestions;

    private List<Vector2Int> Locations
    {
        get
        {
            List<Vector2Int> v = new List<Vector2Int>();
            for(int i = 0; i < numberOfQuestions; i++)
            {
                v.Add(new Vector2Int(Random.Range(0, 11), Random.Range(0, 11)));
                //v[i].x = Random.Range(0, 11);
                //v[i].y = Random.Range(0, 11);
            }

           return v;
        }
    }

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

        locationsToMoveTo = new List<Vector2Int>();

        foreach (Vector2Int v in Locations)
        {
            Debug.Log($"{ v.x } | { v.y }");
            locationsToMoveTo.Add(v);
        }

        foreach (Vector2Int v in locationsToMoveTo)
        { Debug.Log($" { v.x } | { v.y }"); }
    }

    public override void CheckAnswer()
    {
        //if (VerifyAnswer(startPoint, locationsToMoveTo[(int)questionsDone], currentOffset))
        //{
           // questionsAnswered++;
        //}

        //questionsDone++;

        //startPoint = currentPosition;
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

    public void Button_MovePoint(TransformDirection transformDirection)
    {
        switch(transformDirection.direction.vertical)
        {
            case TransformDirection.Vertical.Up:
                currentPosition.y += 1;
                
                //moveablePoint.Translate(0, 0.1f, 0);
                // DELET THIS
                break;
            case TransformDirection.Vertical.Down:
                currentPosition.y -= 1;
                
                //moveablePoint.Translate(0, -0.1f, 0);
                // DELET THIS
                break;
            default:
                break;
        }

        switch (transformDirection.direction.horizontal)
        {
            case TransformDirection.Horizontal.Right:
                currentPosition.x += 1;

                //moveablePoint.Translate(0.1f, 0, 0);
                // DELET THIS
                break;
            case TransformDirection.Horizontal.Left:
                currentPosition.x -= 1;

                //moveablePoint.Translate(-0.1f, 0, 0);
                // DELET THIS
                break;
            default:
                break;
        }

        currentPosition = CurrentPosition;

        moveablePoint.localPosition = new Vector3((currentPosition.x - 5) * 0.05f, (currentPosition.y - 5) * 0.05f, 0f);
    }

    public void Button_SubmitAnswer()
    {

    }

    private void Awake()
    {
        currentPosition = new Vector2Int(5, 5);

        moveablePoint.localPosition = new Vector3((currentPosition.x - 5) * 0.1f, (currentPosition.y - 5) * 0.1f, 0f);
    }
}