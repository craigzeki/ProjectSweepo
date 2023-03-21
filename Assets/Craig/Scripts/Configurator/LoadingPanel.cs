using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPanel : PanelRoot
{
    [SerializeField] private PanelRoot _questionSelectPanel;
    [SerializeField] private float _startupDelay = 1.5f;
    [SerializeField] private float _closeDelay = 1.5f;
    [SerializeField] private bool _simulateLoading = false;


    private Coroutine _panelDelayCoroutine;
    private bool _delaying = false;
    private bool _runOnce = false;
    private bool _questionsLoaded = false;
    
    protected override void CloseComplete(bool _backPressed)
    {
        SaveLoadManager.Instance.LoadComplete -= QuestionsLoaded;
        if(_backPressed)
        {
            //do nothing;
        }
        else
        {
            _nextPanel = _questionSelectPanel;
        }
    }

    protected override void Init()
    {
        SaveLoadManager.Instance.LoadComplete += QuestionsLoaded;
    }

    protected override void LoadComplete()
    {
        _panelDelayCoroutine ??= StartCoroutine(PanelDelay(_startupDelay));
    }

    protected override void Running()
    {
        if((!_delaying) && (_runOnce == false))
        {
            _runOnce = true;
            if(_simulateLoading)
            {
                QuestionsLoaded(this, true);
            }
            else
            {
                SaveLoadManager.Instance.RequestLoadGame();
            }
            
        }

        if((!_delaying) && (_questionsLoaded))
        {
            ClosePanel();
        }
    }

    protected void QuestionsLoaded(object sender, bool IsSuccessful)
    {
        if (IsSuccessful)
        {
            if (_panelDelayCoroutine == null) StopCoroutine(_panelDelayCoroutine);
            _panelDelayCoroutine = StartCoroutine(PanelDelay(_closeDelay));
            _questionsLoaded = true;
        }

    }

    IEnumerator PanelDelay(float delay)
    {
        _delaying = true;
        yield return new WaitForSeconds(delay);
        _delaying = false;
    }
}
