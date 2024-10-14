using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DefaultButtonScript : MonoBehaviour
{
    public TMP_Text choice;

    public void Defaultify()
    {
        choice.text="Default";
    }
}
