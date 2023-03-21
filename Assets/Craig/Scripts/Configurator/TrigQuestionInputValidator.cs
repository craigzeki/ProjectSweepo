using UnityEngine;
using System;

namespace TMPro
{
    /// <summary>
    /// EXample of a Custom Character Input Validator to only allow digits from 0 to 9.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "InputValidator - TrigQIV.asset", menuName = "TextMeshPro/Input Validators/Trig Question Input Validator", order = 100)]
    public class TrigQuestionInputValidator : TMP_InputValidator
    {
        
        string tempText = "";
        // Custom text input validation function
        public override char Validate(ref string text, ref int pos, char ch)
        {

            if (ch >= '0' && ch <= '9')
            {
                tempText = text;
                tempText = tempText.Insert(pos, ch.ToString()); ;
                if (Int32.TryParse(tempText, out int j))
                {
                    if ((j >= 90) || (j <= 0)) return (char)0;

                    text = text.Insert(pos, ch.ToString());
                    pos += 1;
                    return ch;
                }
                else
                {
                    return (char)0;
                }
            }
            
            return (char)0;
        }
    }
}
