using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEditor.PackageManager.Requests;
using Random = UnityEngine.Random;

public class WebRequest : MonoBehaviour
{
    [SerializeField] private string _url;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _deleteButton;
    [SerializeField] private Button _updateButton;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private CustomButtonView _customButtonPrefab;
    [SerializeField] private Transform _customButtonHolder;
    [SerializeField] private DialogWindow _dialogWindowPrefab;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private ParticleSystem _responseSuccesParticle;


    private List<CustomButtonView> _customButtonViews;

    void Awake()
    {
        _createButton.onClick.AddListener(() => CreateButtonHandler());
        _deleteButton.onClick.AddListener(() => DeleteButtonHandler());
        _updateButton.onClick.AddListener(() => UpdateButtonHandler());
        _refreshButton.onClick.AddListener(() => RefreshButtonHandler());

        _customButtonViews = new List<CustomButtonView>();
    }

    private async UniTaskVoid CreateButtonHandler()
    {
        UnityWebRequest request;
        try
        {
            request = await UnityWebRequest.Post(_url, new WWWForm()).SendWebRequest();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        string response = request.downloadHandler.text;

        ButtonsDataModel buttonModel = DeserializeObject<ButtonsDataModel>(response);
        if (buttonModel == null) return;

        _responseSuccesParticle.Play();
        Debug.Log("Create: " + response);

        CreateButtonView(buttonModel);
    }

    private void DeleteButtonHandler()
    {
        var dialog = Instantiate(_dialogWindowPrefab, _canvas.transform);
        dialog.Init(DeleteRequest, false);
    }

    private async UniTask<string> DeleteRequest(int id)
    {
        var request = UnityWebRequest.Delete(_url + id);
        request.downloadHandler = new DownloadHandlerBuffer();

        try
        {
            await request.SendWebRequest();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return string.Empty;
        }

        var view = _customButtonViews.Find(b => b.ButtonDataModel.id == id);

        if (view != null)
        {
            _customButtonViews.Remove(view);
            Destroy(view.gameObject);
        }

        _responseSuccesParticle.Play();
        Debug.Log("Delete: " + request.downloadHandler.text);

        return request.downloadHandler.text;
    }

    private async UniTaskVoid UpdateButtonHandler()
    {
        var dialog = Instantiate(_dialogWindowPrefab, _canvas.transform);
        dialog.Init(UpdateRequest, false);
    }

    private async UniTask<string> UpdateRequest(int id)
    {
        var customButtonData = new ButtonsDataModel()
        {
            id = id,
            animationType = false,
            text = "Random" + Random.Range(1, 100),
            color = new[] { Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f) }
        };

        string jsonData = JsonConvert.SerializeObject(customButtonData, Formatting.Indented);

        byte[] myData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request;
        try
        {
            request = UnityWebRequest.Put(_url + id, myData);
            request.SetRequestHeader("Content-Type", "application/json");
            await request.SendWebRequest();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return string.Empty;
        }

        var view = _customButtonViews.Find(b => b.ButtonDataModel.id == id);
        if (view != null)
        {
            view.SetView(customButtonData.text, ConvertFloatDataToColor(customButtonData.color), customButtonData);
        }
        else
        {
            CreateButtonView(customButtonData);
        }
        
        _responseSuccesParticle.Play();
        Debug.Log("Update: " + request.downloadHandler.text);

        return request.downloadHandler.text;
    }

    private async UniTaskVoid RefreshButtonHandler()
    {

        var dialog = Instantiate(_dialogWindowPrefab, _canvas.transform);
        dialog.Init(RefreshRequest, true);
    }

    private async UniTask<string> RefreshRequest(int id)
    {
        UnityWebRequest request;

        if (id == -1)
        {
            foreach (CustomButtonView buttonView in _customButtonViews)
            {
                Destroy(buttonView.gameObject);
            }
            _customButtonViews.Clear();

            try
            {
                request = await UnityWebRequest.Get(_url).SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return String.Empty;
            }

            string response = request.downloadHandler.text;
            Debug.Log("Refresh: " + response);

            var buttonsDataModel = DeserializeObject<List<ButtonsDataModel>>(response);

            if (buttonsDataModel == null)
                return string.Empty;

            foreach (var buttonModel in buttonsDataModel)
            {
                CreateButtonView(buttonModel);
            }
            _responseSuccesParticle.Play();
            return response;
        }
        else
        {
            try
            {
                request = await UnityWebRequest.Get(_url + id).SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return String.Empty;
            }

            string response = request.downloadHandler.text;

            Debug.Log("Refresh: " + response);

            var buttonModel = DeserializeObject<ButtonsDataModel>(response);

            if (buttonModel == null)
                return string.Empty;

            var view = _customButtonViews.Find(b => b.ButtonDataModel.id == id);

            if (view != null)
            {
                view.SetView(buttonModel.text, ConvertFloatDataToColor(buttonModel.color), buttonModel);
            }
            else
            {
                CreateButtonView(buttonModel);
            }
            _responseSuccesParticle.Play();
            return response;
        }
    }

    private T DeserializeObject<T>(string json)
    {
        T data;
        try
        {
            data = JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return default(T);
        }

        return data;
    }

    private void CreateButtonView(ButtonsDataModel buttonModel)
    {
        var customButton = Instantiate(_customButtonPrefab, _customButtonHolder);
        customButton.SetView(buttonModel.text, ConvertFloatDataToColor(buttonModel.color), buttonModel);
        _customButtonViews.Add(customButton);
    }

    private Color ConvertFloatDataToColor(float[] data)
    {
        return new Color(data[0], data[1], data[2]);
    }
}
