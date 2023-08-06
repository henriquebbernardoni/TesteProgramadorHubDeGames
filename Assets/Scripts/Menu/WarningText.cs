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

    private IEnumerator WarningTextRoutine(string text)
    {
        warningText.text = text;

        float waitTime = 0f;
        while (waitTime <= 2.5f)
        {
            waitTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        warningText.text = string.Empty;
    }
}