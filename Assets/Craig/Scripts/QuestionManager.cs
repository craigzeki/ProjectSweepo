using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestionManager : MonoBehaviour
{
    private List<GameObject> _questions = new List<GameObject>();
    private int currentQuestion = 0;

    private void Start()
    {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        SaveLoadManager.Instance.SaveablesDestroyed += QuestionsDestroyed;
        SaveLoadManager.Instance.LoadComplete += QuestionsLoaded;
    }

    public void AddLoadedQuestionToList(SaveableObject saveable)
    {
        _questions.Add(saveable.gameObject);
        saveable.gameObject.transform.SetParent(this.transform);
    }

    protected GameObject GetRandomQuestion()
    {
        if (_questions.Count == 0) return null;
        currentQuestion = UnityEngine.Random.Range((int)0, _questions.Count);
        return _questions[currentQuestion];
    }

    protected void QuestionsDestroyed(object sender, EventArgs e)
    {
        _questions.Clear();
    }

    protected void QuestionsLoaded(object sender, bool IsSuccessful)
    {
        if(IsSuccessful)
        {
            ResetQuestions();
        }
        
    }

    protected abstract void ResetQuestions();
    public abstract void CheckAnswer();
    public abstract void NextQuestion();
}
