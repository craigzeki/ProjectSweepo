using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class QuestionSelectPanel : PanelRoot
{
    [SerializeField] private PanelRoot _trigQuestionMainPanel;
    [SerializeField] private PanelRoot _ratioQuestionMainPanel;
    [SerializeField] private PanelRoot _geometryQuestionMainPanel;
    protected override void CloseComplete(bool _backPressed)
    {
        if(_backPressed)
        {
            _nextPanel = _previousPanel;
        }
        else
        {
            //_nextPanel = the one selected
        }
    }

    protected override void Init()
    {
        //do nothing
    }

    protected override void LoadComplete()
    {
        //do nothing
    }

    protected override void Running()
    {
        //do nothing
    }

    

    public void TrigQuestionSelected()
    {
        _nextPanel = _trigQuestionMainPanel;
        ClosePanel();
    }
    
    public void RatioQuestionSelected()
    {
        _nextPanel = _ratioQuestionMainPanel;
        ClosePanel();
    }
    
    public void GeometryQuestionSelected()
    {
        _nextPanel = _geometryQuestionMainPanel;
        ClosePanel();
    }
}
