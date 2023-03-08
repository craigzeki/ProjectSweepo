using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrigButtonAnswer : MonoBehaviour
{
    [SerializeField] private float answer = 0f;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private string _textFormat = "F0";
    [SerializeField] private string _textPrepend = "";
    [SerializeField] private string _textPostpend = "°";

    public float Answer
    {
        get => answer;
        
        set
        {
            answer = value;
            UpdateButtonText();
        }
    }

    private void Awake()
    {
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        _buttonText.text = _textPrepend + answer.ToString(_textFormat) + _textPostpend; //F0 = no decimal points
    }
}
