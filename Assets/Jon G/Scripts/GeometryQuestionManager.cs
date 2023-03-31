using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeometryQuestionManager : MonoBehaviour //QuestionManager
{

    // SORT THESE FIELDS OUT LATER
    private bool gridIsActive = false;

    public RectTransform transformTextParent;
    public RectTransform startPoint, guessPoints, answerPoints;
    [SerializeField] [HideInInspector] private List<Vector2Int> guesses;
    [SerializeField] [HideInInspector] private List<RectTransform> guessPositions;

    private Vector2Int startLocation = new Vector2Int(0, 0);

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

    private List<Vector2Int> Locations;
    //{
    //    get
    //    {
    //        List<Vector2Int> v = new List<Vector2Int>();
    //        for(int i = 0; i < numberOfQuestions; i++)
    //        {
    //            v.Add(new Vector2Int(Random.Range(0, 11), Random.Range(0, 11)));
    //            //v[i].x = Random.Range(0, 11);
    //            //v[i].y = Random.Range(0, 11);
    //        }

    //       return v;
    //    }
    //}

    private List<Vector2Int> GenerateLocations()
    {
        List<Vector2Int> v = new List<Vector2Int>();
        for (int i = 0; i < numberOfQuestions; i++)
        {
            v.Add(new Vector2Int(Random.Range(0, 11), Random.Range(0, 11)));
            //v[i].x = Random.Range(0, 11);
            //v[i].y = Random.Range(0, 11);
        }

        return v;
    }

    private List<Vector2Int> GetOffsets(List<Vector2Int> locations)
    {
        List<Vector2Int> loc = new List<Vector2Int>();

        foreach (Vector2Int v in locations)
        {
            Vector2Int offset = v - startLocation;
            loc.Add(offset);
        }

        return loc;
    }

    [ContextMenu("Validate Locations")]
    private void ValidateLocations()
    {
        locationsToMoveTo = new List<Vector2Int>();

        foreach (Vector2Int v in Locations)
        {
            //Debug.Log($"{ v.x } | { v.y }");
            locationsToMoveTo.Add(v);
        }

        //foreach (Vector2Int v in locationsToMoveTo)
        //{ Debug.Log($" { v.x } | { v.y }"); }
    }

    private bool VerifyAnswer(Vector2Int startPoint, Vector2Int target, Vector2Int offset)
    {
        return startPoint + offset == target;
    }

    public void Button_MovePoint(TransformDirection transformDirection)
    {
        if (!gridIsActive)
        { return; }

        switch (transformDirection.direction.vertical)
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

    private void Button_SubmitAnswer()
    {
        guesses.Add(currentPosition);

        RectTransform newGuess = Instantiate(moveablePoint, moveablePoint.position, Quaternion.identity);
        newGuess.SetParent(guessPoints);
        guessPositions.Add(newGuess);

        //newGuess.gameObject.SetActive(false);

        newGuess.gameObject.GetComponent<Image>().color = new Color(1f, 0.8f, 0f, 1f);

        if (currentPosition == locationsToMoveTo[questionsDone])
        { questionsAnswered++; }

        questionsDone++;

        if (questionsDone == numberOfQuestions)
        {
            Debug.Log("PINGAS");
            gridIsActive = false;

            foreach(RectTransform r in guessPositions)
            { r.gameObject.SetActive(true); }

            moveablePoint.gameObject.SetActive(false);
            startPoint.gameObject.SetActive(false);

            for (int i = 0; i < numberOfQuestions; i++)
            {
                guessPositions[i].gameObject.GetComponent<Image>().color = guesses[i] == locationsToMoveTo[i] ? new Color(0f, 1f, 0f, 1f) : new Color(1f, 0f, 0f, 1f);
            }
        }
    }

    private IEnumerator C(float interval, List<TextMeshProUGUI> transformText)
    {
        for (int i = 0; i < numberOfQuestions; i++)
        {
            Debug.Log("B");

            transformText[i].gameObject.SetActive(true);

            transformText[i].text = $"({GetOffsets(locationsToMoveTo)[i].x}, {GetOffsets(locationsToMoveTo)[i].y})";

            yield return new WaitForSeconds(Mathf.Abs(interval));
        }
    }

    private void Button_ActivatePuzzle()
    {
        if (Application.isPlaying)
        { gridIsActive = true; }
        else { gridIsActive = false; }

        foreach (Transform child in guessPoints.transform)
        {
            Destroy(child.gameObject);
        }
        guesses.Clear();
        locationsToMoveTo.Clear();
        startLocation = new Vector2Int(Random.Range(0, 11), Random.Range(0, 11));
        startPoint.localPosition = new Vector3((startLocation.x - 5) * 0.05f, (startLocation.y - 5) * 0.05f, 0f);
        startPoint.gameObject.SetActive(true);

        currentPosition = startLocation;
        moveablePoint.localPosition = new Vector3((currentPosition.x - 5) * 0.05f, (currentPosition.y - 5) * 0.05f, 0f);
        moveablePoint.gameObject.SetActive(gridIsActive);

        questionsDone = 0;
        questionsAnswered = 0;

        
        Locations = GenerateLocations();

        ValidateLocations();

        

        List<TextMeshProUGUI> transformText = new List<TextMeshProUGUI>();

        foreach (Transform child in transformTextParent.transform)
        {
            Debug.Log("A");

            if (child.GetComponent<TextMeshProUGUI>() != null)
            {
                transformText.Add(child.GetComponent<TextMeshProUGUI>());

                child.gameObject.SetActive(false);
            }
        }

        

        /*for (int i = 0; i < numberOfQuestions; i++)
        {
            Debug.Log("B");

            transformText[i].text = $"({GetOffsets(locationsToMoveTo)[i].x}, {GetOffsets(locationsToMoveTo)[i].y})";

            transformText[i].gameObject.SetActive(true);
        }*/

        Debug.Log("Pingas!");

        guessPositions.Clear();
        
        
        StartCoroutine(C(0.25f, transformText));
    }

    [ContextMenu("Press Big Button")]
    public void Button_BigButton()
    {
        if (gridIsActive)
        { Button_SubmitAnswer(); }
        else { Button_ActivatePuzzle(); }
    }

    private void Awake()
    {
        gridIsActive = false;

        Random.InitState((int)System.DateTime.Now.Ticks);

        Locations = GenerateLocations();
        //Button_ActivatePuzzle();

        //currentPosition = new Vector2Int(5, 5);

        //startLocation = new Vector2Int(Random.Range(0, 11), Random.Range(0, 11));
        //startPoint.localPosition = new Vector3((startLocation.x - 5) * 0.05f, (startLocation.y - 5) * 0.05f, 0f);

        //moveablePoint.localPosition = new Vector3((currentPosition.x - 5) * 0.1f, (currentPosition.y - 5) * 0.1f, 0f);
    }
}