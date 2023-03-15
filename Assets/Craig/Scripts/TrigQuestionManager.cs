using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ZekstersLab.Helpers;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Unity.VisualScripting;
using System.Linq;

public class TrigQuestionManager : QuestionManager
{
    private enum QM_State
    {
        WAITING_TO_START = 0,
        ANSWERING,
        FIRING_SEQUENCE,
        ALL_CORRECT,
        INCORRECT,
        NUM_OF_STATES
    }

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
    [SerializeField] private List<Transform> _targetAsteroidSpawnPoints = new List<Transform>();
    [SerializeField] private GameObject _missilePrefab;
    [SerializeField] private Transform _missileSpawnPoint;
    [SerializeField] private GameObject _welcomeHUDCanvas;
    [SerializeField] private GameObject _questionHUDCanvas;
    [SerializeField] private GameObject _firingHUDCanvas;
    [SerializeField] private GameObject _welcomeInputCanvas;
    [SerializeField] private GameObject _answerInputCanvas;
    [SerializeField] private GameObject _firingInputCanvas;
    [SerializeField] private GameObject _inputSurface;
    [SerializeField] private TextMeshProUGUI _questionNumberText;
    [SerializeField] private Vector3 _hudCanvasFullScale = Vector3.one;
    [SerializeField] private Vector3 _inputCanvasFullScale = Vector3.one;
    [SerializeField] private Vector3 _hudCanvasMinScaleThreshold = Vector3.zero;
    

    private static TrigQuestionManager instance;
    private TrigQuestionData _questionData;
    private string _questionIdString = "";
    private string _aString = "";
    private string _bString = "";
    private List<float> _randomAnswers = new List<float>();
    private List<Toggle> _toggles = new List<Toggle>();
    private float _selectedAnswer = INVALID_ANSWER;
    private uint _currentQuestion = 1;
    private UI_TweenScale _tweenScaler;
    private QM_State _qmState = QM_State.WAITING_TO_START;
    private QM_State _nextState = QM_State.WAITING_TO_START;
    private TrigSettingsData _trigSettingsData = new TrigSettingsData();
    private bool _allCorrect = false;
    private GameObject _currentHUDCanvas;
    private GameObject _currentInputCanvas;

    public static TrigQuestionManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<TrigQuestionManager>();
            return instance;
        }
    }

    private QM_State NextState
    {
        get => _nextState;

        set
        {
            _nextState = value;
            DoTransition();
        }
    }

    public bool AllCorrect { get => _allCorrect; }

    private void Awake()
    {
        if (_questionID != null) { _questionIdString = _questionID.text; }
        if (_a != null) { _aString = _a.text; }
        if (_b != null) { _bString = _b.text; }
        

        _trigSettingsData.numberOfQuestions = 3;

        //colect list of buttons on the answer canvas
        if (_answerInputCanvas != null)
        {
            _answerInputCanvas.GetComponentsInChildren(true, _toggles);
        }

        //create list of random answers
        for (uint i = _randomAnswersMin; i <= _randomAnswersMax; i++)
        {
            //! can be updated later to generate a random float between i (inclusive) and i+1 (exclusive)
            _randomAnswers.Add(i);
        }
        _randomAnswers.Shuffle();

        if (_questionNumberText != null) { SetQuestionNumber(_currentQuestion); }
        ResetQuestions();
        SetCanvases(QM_State.WAITING_TO_START);
    }

    private void SetQuestionNumber(uint questionNumber)
    {
        _questionNumberText.text = questionNumber.ToString() + " of " + _trigSettingsData.numberOfQuestions.ToString();
    }

    private void SetCanvases(QM_State _newState)
    {
        switch (_newState)
        {
            case QM_State.WAITING_TO_START:
                if (_welcomeHUDCanvas != null) { _welcomeHUDCanvas.SetActive(true); _currentHUDCanvas = _welcomeHUDCanvas; }
                if (_questionHUDCanvas != null) { _questionHUDCanvas.SetActive(false); }
                if (_firingHUDCanvas != null) { _firingHUDCanvas.SetActive(false); }
                if (_welcomeInputCanvas != null) { _welcomeInputCanvas.SetActive(true); _currentInputCanvas = _welcomeInputCanvas; }
                if (_answerInputCanvas != null) { _answerInputCanvas.SetActive(false); };
                if (_firingInputCanvas != null) { _firingInputCanvas.SetActive(false); }
                break;
            case QM_State.ANSWERING:
                if (_welcomeHUDCanvas != null) { _welcomeHUDCanvas.SetActive(false); }
                if (_questionHUDCanvas != null) { _questionHUDCanvas.SetActive(true); _currentHUDCanvas = _questionHUDCanvas; }
                if (_firingHUDCanvas != null) { _firingHUDCanvas.SetActive(false); }
                if (_welcomeInputCanvas != null) { _welcomeInputCanvas.SetActive(false); }
                if (_answerInputCanvas != null) { _answerInputCanvas.SetActive(true); _currentInputCanvas = _answerInputCanvas; }
                if (_firingInputCanvas != null) { _firingInputCanvas.SetActive(false); }
                break;
            case QM_State.FIRING_SEQUENCE:
                if (_welcomeHUDCanvas != null) { _welcomeHUDCanvas.SetActive(false); }
                if (_questionHUDCanvas != null) { _questionHUDCanvas.SetActive(false); }
                if (_firingHUDCanvas != null) { _firingHUDCanvas.SetActive(true); _currentHUDCanvas = _firingHUDCanvas; }
                if (_welcomeInputCanvas != null) { _welcomeInputCanvas.SetActive(false); }
                if (_answerInputCanvas != null) { _answerInputCanvas.SetActive(false); };
                if (_firingInputCanvas != null) { _firingInputCanvas.SetActive(true); _currentInputCanvas = _firingInputCanvas; }
                break;
            case QM_State.ALL_CORRECT:
                break;
            case QM_State.INCORRECT:
                break;
            case QM_State.NUM_OF_STATES:
            default:
                break;
        }
        //_currentInputCanvas.GetComponentInChildren<Panel>.localScale = _inputCanvasFullScale;
        
        _tweenScaler = _currentHUDCanvas.GetComponentInChildren<UI_TweenScale>();
        if (_tweenScaler != null) _tweenScaler.GetComponent<RectTransform>().localScale = _hudCanvasFullScale;
        
    }

    private void SetInputInteractions(bool enabled)
    {
        if (_inputSurface != null) _inputSurface.SetActive(enabled);
    }

    public bool Vector3GreaterThan(Vector3 v1, Vector3 v2)
    {
        return (v1.x > v2.x) || (v1.y > v2.y) || (v1.z > v2.z);
    }

    IEnumerator SwitchCanvases(QM_State newState, QM_State currentState)
    {
        //disable user input
        SetInputInteractions(false);

        switch (newState)
        {
            case QM_State.WAITING_TO_START:
                if(currentState == QM_State.WAITING_TO_START)
                {
                    //just initialize - no animation
                    SetCanvases(newState);

                }
                else if((currentState == QM_State.ALL_CORRECT) || (currentState == QM_State.INCORRECT))
                {
                    if(_tweenScaler != null)
                    {
                        _tweenScaler.Play();
                        while (Vector3GreaterThan(_tweenScaler.GetComponent<RectTransform>().localScale, _hudCanvasMinScaleThreshold))
                        {
                            yield return null;
                        }
                    }

                    SetCanvases(newState);
                }
                break;
            case QM_State.ANSWERING:
                if(currentState == QM_State.WAITING_TO_START)
                {
                    if(_tweenScaler != null)
                    {
                        _tweenScaler.Play();
                        while (Vector3GreaterThan(_tweenScaler.GetComponent<RectTransform>().localScale, _hudCanvasMinScaleThreshold))
                        {
                            yield return null;
                        }
                    }

                    SetCanvases(newState);
                }
                break;
            case QM_State.FIRING_SEQUENCE:
                break;
            case QM_State.ALL_CORRECT:
                break;
            case QM_State.INCORRECT:
                break;
            case QM_State.NUM_OF_STATES:
                break;
                
        }

        //enable user input
        SetInputInteractions(true);
    }

    private void DoTransition()
    {
        switch (_nextState)
        {
            case QM_State.WAITING_TO_START:
                if((_qmState == QM_State.INCORRECT) || (_qmState == QM_State.ALL_CORRECT))
                {
                    

                    
                    ResetQuestions();
                    StartCoroutine(SwitchCanvases(_nextState, _qmState));
                    
                    _qmState = _nextState;
                }
                break;
            case QM_State.ANSWERING:
                if(_qmState == QM_State.WAITING_TO_START)
                {


                    StartCoroutine(SwitchCanvases(_nextState, _qmState));
                    _qmState = _nextState;
                }
                break;
            case QM_State.FIRING_SEQUENCE:
                if(_qmState == QM_State.ANSWERING)
                {
                    StartCoroutine(SwitchCanvases(_nextState, _qmState));
                    _qmState = _nextState;
                }
                break;
            case QM_State.ALL_CORRECT:
                break;
            case QM_State.INCORRECT:
                break;
            case QM_State.NUM_OF_STATES:
            default:
                break;
        }
    }

    private void ResetQuestions()
    {
        _allCorrect = true;

        SwitchOffAnswerToggles();

        _selectedAnswer = INVALID_ANSWER;
        _currentQuestion = 1;
        //todo: respawn the targets

        SetFireButtonText("Submit");
        NextQuestion();
    }

    private void SwitchOffAnswerToggles()
    {
        //switch off all buttons
        foreach (Toggle toggle in _toggles)
        {
            toggle.isOn = false;
        }
        if(_currentQuestion == _trigSettingsData.numberOfQuestions)
        {
            SetFireButtonText("Fire");
        }
        else
        {
            SetFireButtonText("Submit");
        }
    }

    public void StartPressed()
    {
        NextState = QM_State.ANSWERING;
    }

    public void ButtonCheckAnswer(Toggle buttonToggle)
    {
        if (buttonToggle == null) return;
        if (!buttonToggle.isOn) return;

        CheckAnswer();
    }

    public override void CheckAnswer()
    {
        

        _currentQuestion++;
        //updates the all correct flag
        _allCorrect &= (_selectedAnswer == _questionData.b);

        if (_currentQuestion > _trigSettingsData.numberOfQuestions)
        {
            SetFireButtonText("Firing");
            NextState = QM_State.FIRING_SEQUENCE;
        }
        else
        {
            
            SetQuestionNumber(_currentQuestion);
            SetFireButtonText("Submitting");
            StartCoroutine(NextQuestionWait(0.5f));
        }

        if (_selectedAnswer == _questionData.b)
        {
            Debug.Log("Question " + (_currentQuestion - 1).ToString() + " - CORRECT ANSWER: AllCorrect = " + _allCorrect.ToString());

        }
        else
        {
            Debug.Log("Question " + (_currentQuestion - 1).ToString() + " - INCORRECT ANSWER: AllCorrect = " + _allCorrect.ToString());
        }

        
    }

    IEnumerator NextQuestionWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        NextQuestion();
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
        //_b.text = _bString + _questionData.b.ToString();
        RandomizeAnswers(_questionData.b);
        SwitchOffAnswerToggles();
    }

    public void LoadTrigSettings(TrigSettingsData trigSettingsData)
    {
        if(trigSettingsData == null) return;
        _trigSettingsData = trigSettingsData;
    }

    public void SaveTrigSettings(ref TrigSettingsData trigSettingsData)
    {
        if(trigSettingsData == null) return;
        trigSettingsData = _trigSettingsData;
    }

    private void RandomizeAnswers(float answer)
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
    }

    public void SetSelectedAnswer(Toggle trigButtonToggle)
    {
        if (trigButtonToggle == null) return;
        if(trigButtonToggle.isOn)
        {
            Debug.Log("Toggle ON");
            TrigButtonAnswer trigButtonAnswer;
            if(trigButtonToggle.gameObject.TryGetComponent<TrigButtonAnswer>(out trigButtonAnswer))
            {
                _selectedAnswer = trigButtonAnswer.Answer;
            }
            else
            {
                _selectedAnswer = INVALID_ANSWER;
            }
        }
        else
        {
            Debug.Log("Toggle OFF");
            _selectedAnswer = INVALID_ANSWER;
        }
        
    }

    private void SetFireButtonText(string text)
    {
        if (_fireButtonText == null) return;
        _fireButtonText.text = text;
    }

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
