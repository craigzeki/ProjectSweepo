using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(UI_TweenScale))]
public abstract class PanelRoot : MonoBehaviour
{
    private enum PanelState
    {
        INIT = 0,
        LOADING,
        LOAD_COMPLETE,
        RUNNING,
        CLOSING,
        CLOSE_COMPLETE,
        CLOSED,
        NUM_OF_STATES
    }

    protected PanelRoot _previousPanel;
    protected PanelRoot _nextPanel;

    private UI_TweenScale _tweenScaler;
    private Vector3 _panelFullScale = Vector3.one;
    private Vector3 _panelMinSizeThreshold = new Vector3(0.001f, 0.001f, 0.001f);
    private Coroutine _closePanelCoroutine;
    private PanelState _panelState = PanelState.INIT;
    private bool _backPressed = false;

    private void Awake()
    {
        _panelFullScale = GetComponent<RectTransform>().localScale;
        _tweenScaler = GetComponent<UI_TweenScale>();
    }

    private void Update()
    {
        switch (_panelState)
        {
            case PanelState.INIT:
                Init();
                _panelState = PanelState.LOADING;
                break;
            case PanelState.LOADING:
                _panelState = PanelState.LOAD_COMPLETE;
                break;
            case PanelState.LOAD_COMPLETE:
                LoadComplete();
                _panelState = PanelState.RUNNING;
                break;
            case PanelState.RUNNING:
                Running();
                break;
            case PanelState.CLOSING:
                //docloseplanel coroutine will move to close_complete
                break;
            case PanelState.CLOSE_COMPLETE:
                _closePanelCoroutine = null;
                CloseComplete(_backPressed);
                _nextPanel?.LoadPanel(this);
                this.gameObject.SetActive(false);
                break;
            case PanelState.NUM_OF_STATES:
            default:
                break;
        }
    }

    protected abstract void Init();
    protected abstract void LoadComplete();
    protected abstract void Running();
    protected abstract void CloseComplete(bool _backPressed);

    private void OnEnable()
    {
        _panelState = PanelState.INIT;
        GetComponent<RectTransform>().localScale = _panelFullScale;
    }

    public void LoadPanel(PanelRoot previousPanel)
    {
        this.gameObject.SetActive(true);
        _previousPanel = previousPanel;
    }

    public void ClosePanel()
    {
        _closePanelCoroutine ??= StartCoroutine(DoClosePanel());
        _panelState= PanelState.CLOSING;
    }

    IEnumerator DoClosePanel()
    {
        if (_tweenScaler != null)
        {
            _tweenScaler.Play();
            while (Vector3GreaterThan(GetComponent<RectTransform>().localScale, _panelMinSizeThreshold))
            {
                yield return null;
            }
        }
        _panelState = PanelState.CLOSE_COMPLETE;
    }

    public void BackButton()
    {
        _backPressed = true;
        ClosePanel();
    }

    protected bool Vector3GreaterThan(Vector3 v1, Vector3 v2)
    {
        return (v1.x > v2.x) || (v1.y > v2.y) || (v1.z > v2.z);
    }

}
