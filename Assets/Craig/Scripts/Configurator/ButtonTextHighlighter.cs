using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class ButtonTextHighlighter : MonoBehaviour
{
    [SerializeField] private bool _isMomentary = false;
    [SerializeField] private TextMeshProUGUI _textMeshProText;
    [SerializeField] private Color _highlightColor = Color.white;
    [SerializeField] private Color _pressedColor = Color.white;
    [SerializeField] private Color _lockedColor = Color.white;
    [SerializeField] private float _colorMultiplyer = 1f;
    [SerializeField] private float _fadeDuration = 0.1f;
    [SerializeField] UnityEvent _onClick;

    private Color _normalColor;
    private Coroutine _lerpCoroutine;
    private bool _isPressed = false;
    private bool _isLocked = false;
    private bool _restoreValue = false;
    private bool _isInit = false;

    private void Awake()
    {
        if (_textMeshProText == null) return;
        _normalColor = _textMeshProText.color;
        _isInit = true;
    }

    public void ResetButton()
    {
        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
        _isLocked = false;
        _restoreValue = false;
        _isPressed = false;
        if ((_textMeshProText != null) && (_isInit)) _textMeshProText.color = _normalColor;
        
    }

    public void OnPointerEnter()
    {
        if(_isLocked) return;
        if (_isPressed && !_isMomentary) return;
        if (_textMeshProText == null) return;
        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
        _lerpCoroutine = StartCoroutine(LerpColor(_textMeshProText.color, _normalColor * (_highlightColor * _colorMultiplyer)));
    }

    public void OnPointerExit()
    {
        if(_isLocked) return;
        if( _isPressed && !_isMomentary) return;
        if(_textMeshProText == null) return;
        if(_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
        _lerpCoroutine = StartCoroutine(LerpColor(_textMeshProText.color, _normalColor));
    }

    public void OnPointerClick(bool lockButton = false)
    {
        if(_isLocked) return;
        if (_textMeshProText == null) return;
        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
        if(!_isPressed || _isMomentary)
        {
            
            _isPressed = true;
            if(lockButton)
            {
                Lock();
            }
            else
            {
                _textMeshProText.color = _pressedColor;
            }
            _onClick?.Invoke();
        }
        else
        {
            _textMeshProText.color = _normalColor;
            _isPressed = false;
        }
        
    }

    public void Unlock()
    {
        _isLocked = false;
        _restoreValue = false;
        if (_textMeshProText != null) _textMeshProText.color = _normalColor;
    }

    public void Lock()
    {
        _restoreValue = _isLocked;
        _isLocked = true;
        if (_textMeshProText != null) _textMeshProText.color = _lockedColor;
    }

    public void Restore()
    {
        _isLocked = _restoreValue;
        if (!_isLocked) Unlock();
    }

    IEnumerator LerpColor(Color currentColor, Color targetColor)
    {
        if(_textMeshProText == null)
        {
            _lerpCoroutine = null;
            yield return null;
        }

        float timeElapsed = 0f;

        while(timeElapsed < _fadeDuration)
        {
            _textMeshProText.color = Color.Lerp(currentColor, targetColor, timeElapsed / _fadeDuration);
            timeElapsed += Time.deltaTime;
        }
        _textMeshProText.color = targetColor;

        _lerpCoroutine = null;
    }

}
