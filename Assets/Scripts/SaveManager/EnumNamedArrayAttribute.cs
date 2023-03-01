//Author: bonzairob
//Website: https://answers.unity.com/questions/1589226/showing-an-array-with-enum-as-keys-in-the-property.html

using UnityEngine;
public class EnumNamedArrayAttribute : PropertyAttribute
{
    public string[] names;
    public EnumNamedArrayAttribute(System.Type names_enum_type)
    {
        this.names = System.Enum.GetNames(names_enum_type);
    }
}
