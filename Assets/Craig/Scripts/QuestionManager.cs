using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestionManager : MonoBehaviour
{
    private List<GameObject> _questions = new List<GameObject> ();
    private int currentQuestion = 0;
    
    public void AddLoadedQuestionToList(SaveableObject saveable)
    {
        _questions.Add(saveable.gameObject);
        saveable.gameObject.transform.SetParent(this.transform);
    }

    protected GameObject GetRandomQuestion()
    {
        currentQuestion = Random.Range((int)0, _questions.Count);
        return _questions[currentQuestion];
    }

    public abstract void CheckAnswer();
    public abstract void NextQuestion();
}
