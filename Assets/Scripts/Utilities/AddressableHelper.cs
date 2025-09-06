using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public static class AddressableHelper
{
    public static void LoadSprite(string key, Action<Sprite> onLoaded)
    {
        Addressables.LoadAssetAsync<Sprite>(key).Completed += (AsyncOperationHandle<Sprite> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                onLoaded?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load sprite with key {key}");
            }
        };
    }
}
