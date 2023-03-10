using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ZekstersLab.Helpers;
using UnityEngine.UI;


public class RatioQuestionManager : QuestionManager
{
    private const float INVALID_ANSWER = -1;

    [SerializeField] private TextMeshProUGUI _questionID;
    [SerializeField] private TextMeshProUGUI _a;
    [SerializeField] private TextMeshProUGUI _b;
    [SerializeField] private List<TrigButtonAnswer> _answerOptionsValue = new List<TrigButtonAnswer>();
    [SerializeField] private uint _randomAnswersMin = 0, _randomAnswersMax = 90;
    [SerializeField] private Toggle _fireButtonToggle;
    [SerializeField] private TextMeshProUGUI _fireButtonText;
    [SerializeField] private Color _fireButtonTextEnabledColor;
    [SerializeField] private Color _fireButtonTextDisabledColor;

    private static RatioQuestionManager instance;
    private RatioQuestionData _questionData;
    private string _questionIdString = "";
    private string _aString = "";
    private string _bString = "";
    private List<float> _randomAnswers = new List<float>();
    private float _selectedAnswer = INVALID_ANSWER;

    public static RatioQuestionManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<RatioQuestionManager>();
            return instance;
        }
    }

    private void Awake()
    {
        if (_questionID == null) return;
        _questionIdString = _questionID.text;
        if (_a == null) return;
        _aString = _a.text;
        if (_b == null) return;
        _bString = _b.text;

        for(uint i = _randomAnswersMin; i <= _randomAnswersMax; i++)
        {
            //! can be updated later to generate a random float between i (inclusive) and i+1 (exclusive)
            _randomAnswers.Add(i);
        }
        _randomAnswers.Shuffle();
    }

    public override void CheckAnswer()
    {
        if(_selectedAnswer == _questionData.blueplant)
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
        _questionData = go.GetComponent<RatioQuestion>().ratioQuestionData;
        _questionID.text = _questionIdString + _questionData.id.ToString();
        //_a.text = _aString + _questionData.a.ToString();
        //_b.text = _bString + _questionData.b.ToString();
        //RandomizeAnswers(_questionData.b);
    }

    /*private void RandomizeAnswers(float answer)
    {
        int correctAnswerIndex = Random.Range((int)0, (int)9); // pick a random button for the correct answer
        int randomAnswerIndex = 0;

        _randomAnswers.Shuffle(); //shuffle the possible answers

        for(int i = 0; i < _answerOptionsValue.Count; i++)
        {
            if(i == correctAnswerIndex) //display the correct answer
            {
                _answerOptionsValue[i].Answer = answer;
            }
            else
            {
                if (_randomAnswers[randomAnswerIndex] == answer) randomAnswerIndex++;
                _answerOptionsValue[i].Answer = _randomAnswers[randomAnswerIndex];
                randomAnswerIndex++;
            }
        }
    }*/

    //public void SetSelectedAnswer(Toggle trigButtonToggle)
    //{
    //    if (trigButtonToggle == null) return;
    //    if (trigButtonToggle.isOn)
    //    {
    //        Debug.Log("Toggle ON");
    //        TrigButtonAnswer trigButtonAnswer;
    //        if (trigButtonToggle.gameObject.TryGetComponent<TrigButtonAnswer>(out trigButtonAnswer))
    //        {
    //            _selectedAnswer = trigButtonAnswer.Answer;
    //        }
    //        else
    //        {
    //            _selectedAnswer = INVALID_ANSWER;
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("Toggle OFF");
    //        _selectedAnswer = INVALID_ANSWER;
    //    }

    //}

    private void OnGUI()
    {
        Color textColor = Color.white;

        if (_fireButtonToggle == null) return;

        if(_selectedAnswer != INVALID_ANSWER)
        {
            _fireButtonToggle.interactable = true;
            textColor = _fireButtonTextEnabledColor;
        }
        else
        {
            _fireButtonToggle.interactable = false;
            textColor = _fireButtonTextDisabledColor;
        }

        if (_fireButtonText == null) return;
        _fireButtonText.color = textColor;
    }

}
