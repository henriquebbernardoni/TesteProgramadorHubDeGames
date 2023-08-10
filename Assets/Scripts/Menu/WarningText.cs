using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningText : MonoBehaviour
{
    public static WarningText Instance { get; private set; }

    private TextMeshProUGUI warningText;

    private void Awake()
    {
        Instance = this;
        warningText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        warningText.text = string.Empty;
    }

    public void SetWarningText(string text)
    {
        StopAllCoroutines();
        StartCoroutine(WarningTextRoutine(text));
    }

    public void AddToWarningText(string text)
    {
        if (warningText.text == string.Empty)
        {
            SetWarningText(text);
        }
        else
        {
            warningText.text += "\n" + text;
        }
    }

    private IEnumerator WarningTextRoutine(string text)
    {
        warningText.text = text;
        yield return new WaitForSeconds(4f);
        warningText.text = string.Empty;
    }
}