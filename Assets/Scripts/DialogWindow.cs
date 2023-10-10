using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogWindow : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private WebRequest _webRequest;
    [SerializeField] private GameObject _errorText;
    [SerializeField] private GameObject _errorTextEmpty;

    private event Func<int, UniTask<string>> _okButtonAction;
    private bool _isStringEmptyValid;

    void Awake()
    {
        _cancelButton.onClick.AddListener(() => Destroy(gameObject));
        _okButton.onClick.AddListener(OkButtonHandler);
        _errorText.SetActive(false);
        _errorTextEmpty.SetActive(false);
    }

    public void Init(Func<int, UniTask<string>> okButtonAction, bool isStringEmptyValid)
    {
        _okButtonAction = okButtonAction;
        _isStringEmptyValid = isStringEmptyValid;
    }

    private async void OkButtonHandler()
    {
        if (_okButtonAction == null) return;

        var isParse = int.TryParse(_inputField.text, out var id);

        if (isParse && id >= 1)
        {
            await _okButtonAction.Invoke(id);
            Destroy(gameObject);
            return;
        }

        if (!isParse && _isStringEmptyValid && string.IsNullOrEmpty(_inputField.text))
        {
            await _okButtonAction.Invoke(-1);
            Destroy(gameObject);
            return;
        }

        _errorText.SetActive(true);
        if (_isStringEmptyValid) _errorTextEmpty.SetActive(true);
    }
}
