using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionIDInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField _thisInputField;

    private void Awake()
    {
        _thisInputField = GetComponent<TMP_InputField>();
    }

    public void OnLostFocus()
    {
        if (_thisInputField == null) return;
        
        _thisInputField.text = GetFormattedID(_thisInputField.text);
    }

    public string GetFormattedID(string idString, string preText = "")
    {
        if (_thisInputField == null) return "";

        if(preText.Length > 0) idString = idString.Replace(preText, string.Empty, System.StringComparison.OrdinalIgnoreCase);

        if (idString.Length <= _thisInputField.characterLimit)
        {
            int numberOfMissingChars = Mathf.Clamp((_thisInputField.characterLimit - idString.Length), 0, _thisInputField.characterLimit);
            string pre = "";
            for (int i = 0; i < numberOfMissingChars; i++)
            {
                pre += "0";
            }
            return pre + idString;
        }

        return "";
    }
}
