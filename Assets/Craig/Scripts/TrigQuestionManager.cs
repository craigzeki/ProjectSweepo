using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrigQuestionManager : QuestionManager
{
    [SerializeField] TextMeshProUGUI _questionID;
    [SerializeField] TextMeshProUGUI _a;
    [SerializeField] TextMeshProUGUI _b;

    private static TrigQuestionManager instance;
    private TrigQuestionData _questionData;
    private float _answer = 0;
    private string _questionIdString = "";
    private string _aString = "";
    private string _bString = "";

    private void Awake()
    {
        if (_questionID == null) return;
        _questionIdString = _questionID.text;
        if (_a == null) return;
        _aString = _a.text;
        if (_b == null) return;
        _bString = _b.text;
    }
    public static TrigQuestionManager Instance
    {
        get
        {
            if(instance == null) instance = FindObjectOfType<TrigQuestionManager>();
            return instance;
        }
    }

    public override void CheckAnswer()
    {
        if(_answer == _questionData.b)
        {
            Debug.Log("CORRECT ANSWER");
        }
        else
        {
            Debug.Log("INCORRECT ANSWER");
        }
    }

    public override void NextQuestion()
    {
        if (_questionID == null) return;
        if (_a == null) return;
        if(_b == null) return;

        GameObject go = GetRandomQuestion();
        if(go == null) return;
        _questionData = go.GetComponent<TrigQuestion>().trigQuestionData;
        _questionID.text = _questionIdString + _questionData.id.ToString();
        _a.text = _aString + _questionData.a.ToString();
        _b.text = _bString + _questionData.b.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
