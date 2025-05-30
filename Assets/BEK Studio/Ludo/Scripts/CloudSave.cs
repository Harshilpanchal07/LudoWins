using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;

public class CloudSave : MonoBehaviour
{
    public static CloudSave instance;
    public Text status;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    public async void SaveDataAsString(string key, string value)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { key, value }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("Data saved successfully!");
        }
        catch (CloudSaveException e)
        {
            Debug.LogError($"Cloud Save Error: {e.Message}");
        }
    }

    public async void SaveDataAsInt(string key, int value)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { key, value }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("Data saved successfully!");
        }
        catch (CloudSaveException e)
        {
            Debug.LogError($"Cloud Save Error: {e.Message}");
        }
    }

    public async Task<string> LoadStrData(string key)
    {
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

        if (data.TryGetValue(key, out var item))
        {
            return item.Value.GetAsString();
        }
        else
        {
            return null; // or return "Key not found" if you prefer
        }
    }

    public async Task<int> LoadIntData(string key)
    {
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

        if (data.TryGetValue(key, out var item))
        {
            return item.Value.GetAs<int>();
        }
        else
        {
            return 0; // or return a default like 0 if you prefer
        }
    }

}