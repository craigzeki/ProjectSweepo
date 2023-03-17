using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ZekstersLab.Helpers;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Unity.VisualScripting;
using System.Linq;
using JetBrains.Annotations;

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
    [SerializeField] private GameObject _targetAsteroidPrefab;
    [SerializeField] private ParticleSystem _explosionParticleSystem;
    [SerializeField] private Transform _targetAsteroidSpawnPoint;
    [SerializeField] private Transform _targetMissedPoint;
    [SerializeField] private GameObject _missilePrefab;
    [SerializeField] private Transform _missileSpawnPoint;
    [SerializeField] private GameObject _welcomeHUDCanvas;
    [SerializeField] private GameObject _questionHUDCanvas;
    [SerializeField] private GameObject _firingHUDCanvas;
    [SerializeField] private GameObject _firingCompleteHUDCanvas;
    [SerializeField] private GameObject _welcomeInputCanvas;
    [SerializeField] private GameObject _answerInputCanvas;
    [SerializeField] private GameObject _firingInputCanvas;
    [SerializeField] private GameObject _firingCompleteInputCanvas;
    [SerializeField] private GameObject _inputSurface;
    [SerializeField] private Toggle _startButton;
    [SerializeField] private Toggle _retryButton;
    [SerializeField] private TextMeshProUGUI _questionNumberText;
    [SerializeField] private TextMeshProUGUI _questionNumberWelcomeText;
    [SerializeField] private string _questionWelcomePreText = "You will need to answer all ";
    [SerializeField] private string _questionWelcomePostText = " question(s) correctly";
    [SerializeField] private float _firingSequencePause = 0.4f;
    [SerializeField] private float _blinkTextPeriod = 0.6f;
    [SerializeField] private List<TextMeshProUGUI> _firingSequenceTextFields = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI _firingSequenceQCorrectText;
    [SerializeField] private TextMeshProUGUI _firingSequenceMissileText;
    [SerializeField] private string _firingSequenceQCorrectPreText = "";
    [SerializeField] private string _firingSequenceQCorrectMidText = " of ";
    [SerializeField] private string _firingSequenceQCorrectPostText = " questions correct!";
    [SerializeField] private TextMeshProUGUI _firingCompleteText;
    [SerializeField] private string _firingCompleteAllCorrect = "Target Destroyed!";
    [SerializeField] private string _firingCompleteIncorrect = "Target Missed!";
    [SerializeField] private Color _firingCompleteAllCorrectColor = Color.green;
    [SerializeField] private Color _firingCompleteIncorrectColor = Color.red;
    [SerializeField] private Vector3 _hudCanvasFullScale = Vector3.one;
    [SerializeField] private Vector3 _inputCanvasFullScale = Vector3.one;
    [SerializeField] private Vector3 _hudCanvasMinScaleThreshold = Vector3.zero;
    [SerializeField] private float _tweenSpeed = 1.75f;
    

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
    private uint _numberCorrect = 0;
    private GameObject _currentHUDCanvas;
    private GameObject _currentInputCanvas;
    private Coroutine _blinkingTextCoroutine;
    private GameObject _asteroid;
    private GameObject _missile;
    private TrigMissile _trigMissile;

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

    //public bool AllCorrect { get => _allCorrect; }

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

    private void SpawnAsteroid()
    {
        if (_asteroid != null) return; //asteroid already exists
        if (_targetAsteroidPrefab == null) return; //prefab not referenced
        _asteroid = Instantiate(_targetAsteroidPrefab, _targetAsteroidSpawnPoint);
    }

    private void SpawnMissile()
    {
        if (_missile != null) return; //missile already exists
        if (_missilePrefab == null) return; //prefab not referenced
        _missile = Instantiate(_missilePrefab, _missileSpawnPoint);
        _trigMissile = _missile.GetComponent<TrigMissile>();
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
                if (_firingCompleteHUDCanvas != null) { _firingCompleteHUDCanvas.SetActive(false);}
                if (_welcomeInputCanvas != null) { _welcomeInputCanvas.SetActive(true); _currentInputCanvas = _welcomeInputCanvas; }
                if (_answerInputCanvas != null) { _answerInputCanvas.SetActive(false); };
                if (_firingInputCanvas != null) { _firingInputCanvas.SetActive(false); }
                if (_firingCompleteInputCanvas != null) { _firingCompleteInputCanvas.SetActive(false); }
                break;
            case QM_State.ANSWERING:
                if (_welcomeHUDCanvas != null) { _welcomeHUDCanvas.SetActive(false); }
                if (_questionHUDCanvas != null) { _questionHUDCanvas.SetActive(true); _currentHUDCanvas = _questionHUDCanvas; }
                if (_firingHUDCanvas != null) { _firingHUDCanvas.SetActive(false); }
                if (_firingCompleteHUDCanvas != null) { _firingCompleteHUDCanvas.SetActive(false); }
                if (_welcomeInputCanvas != null) { _welcomeInputCanvas.SetActive(false); }
                if (_answerInputCanvas != null) { _answerInputCanvas.SetActive(true); _currentInputCanvas = _answerInputCanvas; }
                if (_firingInputCanvas != null) { _firingInputCanvas.SetActive(false); }
                if (_firingCompleteInputCanvas != null) { _firingCompleteInputCanvas.SetActive(false); }
                break;
            case QM_State.FIRING_SEQUENCE:
                if (_welcomeHUDCanvas != null) { _welcomeHUDCanvas.SetActive(false); }
                if (_questionHUDCanvas != null) { _questionHUDCanvas.SetActive(false); }
                if (_firingHUDCanvas != null) { _firingHUDCanvas.SetActive(true); _currentHUDCanvas = _firingHUDCanvas; }
                if (_firingCompleteHUDCanvas != null) { _firingCompleteHUDCanvas.SetActive(false); }
                if (_welcomeInputCanvas != null) { _welcomeInputCanvas.SetActive(false); }
                if (_answerInputCanvas != null) { _answerInputCanvas.SetActive(false); };
                if (_firingInputCanvas != null) { _firingInputCanvas.SetActive(true); _currentInputCanvas = _firingInputCanvas; }
                if (_firingCompleteInputCanvas != null) { _firingCompleteInputCanvas.SetActive(false); }
                break;
            case QM_State.ALL_CORRECT:
                if (_welcomeHUDCanvas != null) { _welcomeHUDCanvas.SetActive(false); }
                if (_questionHUDCanvas != null) { _questionHUDCanvas.SetActive(false); }
                if (_firingHUDCanvas != null) { _firingHUDCanvas.SetActive(false); }
                if (_firingCompleteHUDCanvas != null) { _firingCompleteHUDCanvas.SetActive(true); _currentHUDCanvas = _firingCompleteHUDCanvas; }
                if (_welcomeInputCanvas != null) { _welcomeInputCanvas.SetActive(false); }
                if (_answerInputCanvas != null) { _answerInputCanvas.SetActive(false); };
                if (_firingInputCanvas != null) { _firingInputCanvas.SetActive(false);  }
                if (_firingCompleteInputCanvas != null) { _firingCompleteInputCanvas.SetActive(true); _currentInputCanvas = _firingCompleteInputCanvas; }
                break;
            case QM_State.INCORRECT:
                if (_welcomeHUDCanvas != null) { _welcomeHUDCanvas.SetActive(false); }
                if (_questionHUDCanvas != null) { _questionHUDCanvas.SetActive(false); }
                if (_firingHUDCanvas != null) { _firingHUDCanvas.SetActive(false); }
                if (_firingCompleteHUDCanvas != null) { _firingCompleteHUDCanvas.SetActive(true); _currentHUDCanvas = _firingCompleteHUDCanvas; }
                if (_welcomeInputCanvas != null) { _welcomeInputCanvas.SetActive(false); }
                if (_answerInputCanvas != null) { _answerInputCanvas.SetActive(false); };
                if (_firingInputCanvas != null) { _firingInputCanvas.SetActive(false); }
                if (_firingCompleteInputCanvas != null) { _firingCompleteInputCanvas.SetActive(true); _currentInputCanvas = _firingCompleteInputCanvas; }
                break;
            case QM_State.NUM_OF_STATES:
            default:
                break;
        }
        //_currentInputCanvas.GetComponentInChildren<Panel>.localScale = _inputCanvasFullScale;
        
        _tweenScaler = _currentHUDCanvas.GetComponentInChildren<UI_TweenScale>();
        if (_tweenScaler != null)
        {
            _tweenScaler.GetComponent<RectTransform>().localScale = _hudCanvasFullScale;
            _tweenScaler.speed = _tweenSpeed;
        }
        
        
    }

    private void ResetFiringSequence()
    {
        foreach (TextMeshProUGUI tmpText in _firingSequenceTextFields)
        {
            tmpText.enabled = false;
        }

        _firingSequenceMissileText.enabled = false;
        _firingSequenceQCorrectText.enabled = false;
        if (_blinkingTextCoroutine != null) StopCoroutine(_blinkingTextCoroutine);
    }

    IEnumerator BlinkText(TextMeshProUGUI tmpText, float period)
    {
        while(true)
        {
            yield return new WaitForSeconds(period);
            tmpText.enabled = !tmpText.enabled;
        }
    }

    IEnumerator DoFiringSequence(float pauseTime)
    {
        foreach(TextMeshProUGUI tmpText in _firingSequenceTextFields)
        {
            tmpText.enabled = true;
            yield return new WaitForSeconds(pauseTime);
        }

        _firingSequenceQCorrectText.text = _firingSequenceQCorrectPreText + 
            _numberCorrect.ToString() + 
            _firingSequenceQCorrectMidText + 
            _trigSettingsData.numberOfQuestions.ToString() + 
            _firingSequenceQCorrectPostText;

        _firingSequenceQCorrectText.enabled = true;
        yield return new WaitForSeconds(pauseTime);
        _firingSequenceMissileText.enabled = true;
        if(_blinkingTextCoroutine != null) StopCoroutine(_blinkingTextCoroutine);
        _blinkingTextCoroutine = StartCoroutine(BlinkText(_firingSequenceMissileText, _blinkTextPeriod));

        if(_allCorrect)
        {
            if ((_trigMissile != null) && (_asteroid != null)) _trigMissile.Launch(_asteroid.transform);
        }
        else
        {
            if ((_trigMissile != null) && (_targetMissedPoint != null)) _trigMissile.Launch(_targetMissedPoint);
        }
        
    }

    private void SetInputInteractions(bool enabled)
    {
        if (_inputSurface != null) _inputSurface.SetActive(enabled);
    }

    public bool Vector3GreaterThan(Vector3 v1, Vector3 v2)
    {
        return (v1.x > v2.x) || (v1.y > v2.y) || (v1.z > v2.z);
    }

    IEnumerator SwitchCanvases(QM_State newState, QM_State currentState, System.Action action = null)
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
                if(currentState == QM_State.ANSWERING)
                {
                    if (_tweenScaler != null)
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
            case QM_State.ALL_CORRECT:
                if (currentState == QM_State.FIRING_SEQUENCE)
                {
                    if (_tweenScaler != null)
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
            case QM_State.INCORRECT:
                if (currentState == QM_State.FIRING_SEQUENCE)
                {
                    if (_tweenScaler != null)
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
            case QM_State.NUM_OF_STATES:
                break;
                
        }

        //enable user input
        SetInputInteractions(true);
        //execute the callback (if any)
        action?.Invoke();
    }

    private void FiringCanvasLoadedAction()
    {
        StartCoroutine(DoFiringSequence(_firingSequencePause));
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
                    ResetFiringSequence();
                    StartCoroutine(SwitchCanvases(_nextState, _qmState, FiringCanvasLoadedAction));
                    _qmState = _nextState;
                }
                break;
            case QM_State.ALL_CORRECT:
                if(_qmState == QM_State.FIRING_SEQUENCE)
                {
                    StartCoroutine(SwitchCanvases(_nextState, _qmState, UpdateFiringCompleteText));
                    _qmState = _nextState;
                }
                break;
            case QM_State.INCORRECT:
                if (_qmState == QM_State.FIRING_SEQUENCE)
                {
                    StartCoroutine(SwitchCanvases(_nextState, _qmState, UpdateFiringCompleteText));
                    _qmState = _nextState;
                }
                break;
            case QM_State.NUM_OF_STATES:
            default:
                break;
        }
    }

    private void ResetQuestions()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        _allCorrect = true;
        _numberCorrect = 0;

        if (_questionNumberWelcomeText != null) _questionNumberWelcomeText.text = _questionWelcomePreText + _trigSettingsData.numberOfQuestions.ToString() + _questionWelcomePostText;

        SwitchOffAnswerToggles();
        _startButton.isOn = false;
        _retryButton.isOn = false;

        _selectedAnswer = INVALID_ANSWER;
        _currentQuestion = 1;
        SetQuestionNumber(_currentQuestion);

        SpawnAsteroid();
        SpawnMissile();

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

    public void StartPressed(Toggle buttonToggle)
    {
        if (buttonToggle == null) return;
        if (!buttonToggle.isOn) return;
        NextState = QM_State.ANSWERING;
    }

    public void RetryPressed(Toggle buttonToggle)
    {
        if (buttonToggle == null) return;
        if (!buttonToggle.isOn) return;
        NextState = QM_State.WAITING_TO_START;
    }

    public void ButtonCheckAnswer(Toggle buttonToggle)
    {
        if (buttonToggle == null) return;
        if (!buttonToggle.isOn) return;

        CheckAnswer();
    }

    IEnumerator TestMissile()
    {
        yield return new WaitForSeconds(4);
        FiringSequenceComplete();
    }

    public void FiringSequenceComplete()
    {
        if (_qmState == QM_State.FIRING_SEQUENCE)
        {

            if(_allCorrect)
            {
                NextState = QM_State.ALL_CORRECT;
            }
            else
            {
                //nothing to destroy
                NextState = QM_State.INCORRECT;
            }

        }
    }

    private void UpdateFiringCompleteText()
    {
        if(_allCorrect)
        {
            _firingCompleteText.text = _firingCompleteAllCorrect;
            _firingCompleteText.color = _firingCompleteAllCorrectColor;
        }
        else
        {
            _firingCompleteText.text = _firingCompleteIncorrect;
            _firingCompleteText.color = _firingCompleteIncorrectColor;
        }
        if (_blinkingTextCoroutine != null) StopCoroutine(_blinkingTextCoroutine);
        _blinkingTextCoroutine = StartCoroutine(BlinkText(_firingCompleteText, _blinkTextPeriod));
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
            _numberCorrect++;
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
