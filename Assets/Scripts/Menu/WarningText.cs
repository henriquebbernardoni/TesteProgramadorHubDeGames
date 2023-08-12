using System.Collections;
using TMPro;
using UnityEngine;

public class WarningText : MonoBehaviour
{
    public static WarningText Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI warningText;

    private Coroutine warningCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        warningText.text = string.Empty;
    }

    public void SetWarningText(string text)
    {
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }
        warningCoroutine = StartCoroutine(WarningTextRoutine(text));
    }

    public void AddToWarningText(string text)
    {
        if (string.IsNullOrEmpty(warningText.text))
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