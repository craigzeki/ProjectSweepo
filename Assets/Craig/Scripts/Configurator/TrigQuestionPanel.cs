using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrigQuestionPanel : PanelRoot
{
    [SerializeField] private string _titleString = "Trigonometry Questions";
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TMP_InputField _answerInputField;
    [SerializeField] private TextMeshProUGUI _answerText;
    [SerializeField] private TextMeshProUGUI _idPreField;
    [SerializeField] private TMP_InputField _idInputField;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private Color _successTextColor = Color.green;
    [SerializeField] private Color _failTextColor = Color.red;
    [SerializeField] private string _successString = "Save OK";
    [SerializeField] private string _failString = "Save FAIL";
    [SerializeField] private float _statusTextFadeTime = 1f;
    [SerializeField] private string _idPreText = "w_trig_";
    [SerializeField] private SaveType _saveType = SaveType.Q_TRIG;

    private List<TrigQuestion> _trigQuestions = new List<TrigQuestion>();
    private int _questionIndex = -1;
    private QuestionIDInput _questionIDInput;
    private Coroutine _fadeCoroutine;

    protected override void CloseComplete(bool _backPressed)
    {
        SaveLoadManager.Instance.CloudSaveComplete -= OnCloudSaveComplete;
        SaveLoadManager.Instance.LocalSaveComplete -= OnLocalSaveComplete;

        if (_backPressed)
        {
            _nextPanel = _previousPanel;
        }
    }

    protected override void Init()
    {
        _questionIDInput = _idInputField.GetComponent<QuestionIDInput>();
        RefreshQuestionsList();
        NextQuestion();

        SaveLoadManager.Instance.CloudSaveComplete += OnCloudSaveComplete;
        SaveLoadManager.Instance.LocalSaveComplete += OnLocalSaveComplete;
    }

    private void OnCloudSaveComplete(object sender, bool IsSuccessful)
    {
        UnlockButtons();
        if (_statusText == null) return;
        if(IsSuccessful)
        {
            _statusText.text = _successString;
            _statusText.color = _successTextColor;
        }
        else
        {
            _statusText.text = _failString;
            _statusText.color = _failTextColor;
        }
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeText(_statusText));
    }

    private void OnLocalSaveComplete(object sender, bool IsSuccessful)
    {

    }

    protected override void LoadComplete()
    {
        
    }

    protected override void Running()
    {
        
    }

    private void UpdateQuestionNumberText()
    {
        if (_titleText == null) return;
        _titleText.text = _titleString + " (" + (_questionIndex + 1).ToString() + "/" + _trigQuestions.Count.ToString() + ")";
    }

    private void RefreshQuestionsList()
    {
        List<int> toRemove = new List<int>();

        toRemove.Clear();
        _trigQuestions.Clear();
        TrigQuestionManager.Instance.gameObject.GetComponentsInChildren<TrigQuestion>(_trigQuestions);
        //now filter out any invalid objects - ones which have been destroyed but not cleaned up yet
        for(int i = 0; i < _trigQuestions.Count; i++)
        {
            if (!_trigQuestions[i].trigQuestionData.valid) toRemove.Add(i);
        }
        int removeCount = 0;
        foreach(int i in toRemove)
        {
            _trigQuestions.RemoveAt(i - removeCount);
            removeCount++;
        }

        _questionIndex = _trigQuestions.Count - 1;
    }

    private void NextQuestion()
    {
        
        if(_questionIndex < 0)
        {
            GameObject newQuestion = SaveLoadManager.Instance.CreateSaveable(_saveType);
            newQuestion.transform.SetParent(TrigQuestionManager.Instance.gameObject.transform);
            RefreshQuestionsList();
        }
        else if(_questionIndex == _trigQuestions.Count - 1)
        {
            //wrap to start of list
            _questionIndex = 0;
        }
        else
        {
            //increment to next question
            _questionIndex++;
        }

        if((_idInputField != null) && (_questionIDInput != null)) _idInputField.text = _questionIDInput.GetFormattedID(_trigQuestions[_questionIndex].trigQuestionData.id, _idPreText);
        if(_answerInputField != null) _answerInputField.text = _trigQuestions[_questionIndex].trigQuestionData.a.ToString();
        
        UpdateIDField();
        UpdateAnswerField();
        UpdateQuestionNumberText();

    }

    private void PreviousQuestion()
    {
        if (_questionIndex < 0)
        {
            GameObject newQuestion = SaveLoadManager.Instance.CreateSaveable(_saveType);
            newQuestion.transform.SetParent(TrigQuestionManager.Instance.gameObject.transform);
            RefreshQuestionsList();
        }
        else if (_questionIndex == 0)
        {
            //wrap to start of list
            _questionIndex = _trigQuestions.Count - 1;
        }
        else
        {
            //increment to next question
            _questionIndex--;
        }

        if ((_idInputField != null) && (_questionIDInput != null)) _idInputField.text = _questionIDInput.GetFormattedID(_trigQuestions[_questionIndex].trigQuestionData.id, _idPreText);
        if (_answerInputField != null) _answerInputField.text = _trigQuestions[_questionIndex].trigQuestionData.a.ToString();

        UpdateIDField();
        UpdateAnswerField();
        UpdateQuestionNumberText();
    }

    private void AddQuestion()
    {
        GameObject newQuestion = SaveLoadManager.Instance.CreateSaveable(_saveType);
        newQuestion.transform.SetParent(TrigQuestionManager.Instance.gameObject.transform);
        RefreshQuestionsList();
        _questionIndex = _trigQuestions.Count - 1;
        if ((_idInputField != null) && (_questionIDInput != null)) _idInputField.text = _questionIDInput.GetFormattedID(_trigQuestions[_questionIndex].trigQuestionData.id, _idPreText);
        if (_answerInputField != null) _answerInputField.text = _trigQuestions[_questionIndex].trigQuestionData.a.ToString();

        UpdateIDField();
        UpdateAnswerField();
        UpdateQuestionNumberText();
    }
    public void AddButton()
    {
        AddQuestion();
    }

    public void NextQuestionButton()
    {
        NextQuestion();
    }

    public void PreviousQuestionButton()
    {
        PreviousQuestion();
    }

    public void DeleteButton()
    {
        _trigQuestions[_questionIndex].GetComponent<SaveableObject>().UnRegisterSaveable();
        Destroy(_trigQuestions[_questionIndex].gameObject);
        _trigQuestions[_questionIndex].trigQuestionData.valid = false; //set this to be false as the Destroy function will operate at the end of the frame and we will rebuild the list before then
        _trigQuestions.RemoveAt(_questionIndex);
        if (_trigQuestions.Count == 0) _questionIndex = -1;
        PreviousQuestion();
    }

    public void UpdateAnswerField()
    {
        if (_answerText == null) return;
        if (_answerInputField == null) return;

        if (Int32.TryParse(_answerInputField.text, out int value))
        {
            _answerText.text = (90 - value).ToString();

            if (_answerInputField == null) return;
            if (_trigQuestions.Count == 0) return;
            _trigQuestions[_questionIndex].trigQuestionData.a = value;
            _trigQuestions[_questionIndex].trigQuestionData.b = 90 - value;
        }
    }

    public void UpdateIDField()
    {
        if (_questionIDInput == null) return;
        if(_idPreField == null) return;
        if (_idInputField == null) return;

        _idPreField.text = _idPreText + _questionIDInput.GetFormattedID(_idInputField.text);

        if (_answerInputField == null) return;
        if (_trigQuestions.Count == 0) return;
        _trigQuestions[_questionIndex].trigQuestionData.id = _idPreField.text;
    }

    public void InputFieldLostFocus()
    {
        if (_answerText == null) return;
        if (_answerInputField == null) return;

        if(_answerInputField.text == "")
        {
            _answerInputField.text = (45).ToString();
        }
    }

    public void InputFieldChanged()
    {
        if(_answerInputField == null) return;
        if(_trigQuestions.Count == 0) return;
         float.TryParse(_answerInputField.text, out _trigQuestions[_questionIndex].trigQuestionData.a);
    }

    public void SaveButton()
    {
        LockButtons();
        SaveLoadManager.Instance.SaveGameToLocal();
        SaveLoadManager.Instance.RequestSaveGame();
    }

    IEnumerator FadeText(TextMeshProUGUI tmpText)
    {
        float elapsedTime = 0;

        tmpText.alpha = 255;

        while (elapsedTime < _statusTextFadeTime)
        {
            tmpText.alpha = Mathf.Lerp(255, 0, elapsedTime / _statusTextFadeTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        tmpText.text = "";
        tmpText.alpha = 255;
        _fadeCoroutine = null;
    }
}
