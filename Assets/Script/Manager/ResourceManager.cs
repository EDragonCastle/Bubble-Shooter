using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

/// <summary>
/// Addressable Asset에서 사용할 Resource들을 관리하고 있는 Manager
/// </summary>
public class ResourceManager
{
    // enum으로도 하려 했지만 string으로 관리하도록 했다.
    private Dictionary<string, HandleObject> managementHandleList;

    // Load가 된 Resource들을 관리할 Dictionary
    private Dictionary<ResourceType, List<GameObject>> loadedResourceList;

    #region ResourceManager 생성자
    /// <summary>
    /// Resource Manager 생성할 때 변수 초기화
    /// </summary>
    public ResourceManager()
    {
        managementHandleList = new Dictionary<string, HandleObject>();
        loadedResourceList = new Dictionary<ResourceType, List<GameObject>>();
    }
    #endregion
    

    // 맨 처음 Load할 Object
    public async UniTask Load()
    {
        await LoadResource(ResourceType.Default, "Red Bubble");
        await LoadResource(ResourceType.Default, "Green Bubble");
        await LoadResource(ResourceType.Default, "Blue Bubble");
        await LoadResource(ResourceType.Default, "Yellow Bubble");

        await LoadResource(ResourceType.Default, "Board");
        await LoadResource(ResourceType.Default, "Game Board");
        await LoadResource(ResourceType.Default, "GameClear");
        await LoadResource(ResourceType.Default, "GameOver");
        await LoadResource(ResourceType.Default, "Ttitle");
        await LoadResource(ResourceType.Default, "SettingWall");

    }

    /// <summary>
    /// Load된 Object List에서 가져온다.
    /// </summary>
    /// <param name="type">원하는 Resource Type</param>
    /// <param name="resourceName">Resource 이름</param>
    /// <returns>GameObject</returns>
    public GameObject GetResource(ResourceType type, string resourceName)
    {
        // Resource Type에 있는지 확인한다.
        if (loadedResourceList.ContainsKey(type))
        {
            // List를 가져와서 순회하는데 있으면 Object를 Return 한다.
            var loadTypeResourceList = loadedResourceList[type];
        
            foreach(var resourceObject in loadTypeResourceList)
            {
                if (resourceObject.name == resourceName)
                    return resourceObject;
            }
        }

        // 없으면 혹시 다른 Type에 있는 것인지 확인한다.
        for(int i = 0; i < loadedResourceList.Count; i++)
        {
            if(!loadedResourceList.ContainsKey((ResourceType)i))
                continue;

            var listObject = loadedResourceList[(ResourceType)i];
            foreach(var resource in listObject)
            {
                if (resource.name == resourceName)
                    return resource;
            }
        }

        // 그래도 없다면 Log를 띄운다.
        Debug.LogError($"Don't Exist {resourceName}");

        return null;
    }

    /// <summary>
    /// 실제 Addressable Asset에서 Load할 Object
    /// </summary>
    /// <param name="type">Resource Type</param>
    /// <param name="resourceName">Resource 이름</param>
    /// <returns></returns>
    public async UniTask LoadResource(ResourceType type, string resourceName)
    {
        // Addressable Asset에서 Load를 한다.
        var handle = Addressables.LoadAssetAsync<GameObject>(resourceName);
        var loadedObject = await handle.Task;

        // Addressable Asset에서 확인했는데 있으면 진행한다.
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // 이름으로 관리되고 있는 Object인지 확인하는데 없으면 생성해서 새로 만들고, 있으면 이미 Load됐다고 알린다.
            if (!managementHandleList.ContainsKey(resourceName))
            {
                HandleObject handleObject = new HandleObject();
                handleObject.handle = handle;
                handleObject.type = type;
                managementHandleList.Add(loadedObject.name, handleObject);
            }
            else
                return;

            // LoadList에 Type이 있는지 확인해서 없다면 새로 만들어서 넣고, 있다면 넣는다.
            if (!loadedResourceList.ContainsKey(type))
            {
                loadedResourceList.Add(type, new List<GameObject>());
                loadedResourceList[type].Add(loadedObject);
            }
            else
                loadedResourceList[type].Add(loadedObject);
        }
    }

    /// <summary>
    /// 사용한 Resource를 반납한다.
    /// </summary>
    /// <param name="releaseObject">반납할 Resource Object</param>
    public void ReleaseResource(GameObject releaseObject)
    {
        // 관리되고 있는 List에 있는지 확인한다.
        if (managementHandleList.ContainsKey(releaseObject.name))
        {
            // List에서 제거한다.
            var handleObject = managementHandleList[releaseObject.name];
            var resourceList = loadedResourceList[handleObject.type];

            // 순회하면서 같으면 제거한다.
            for(int i = 0; i < resourceList.Count; i++)
            {
                var resourceObject = resourceList[i];
                if(resourceObject == releaseObject)
                {
                    resourceList.RemoveAt(i);
                    break;
                }
            }

            // 메모리에 등록되어 있던 것을 반납 후 Dictonary에 저장되어 있는거 삭제하고 Addressable Asset으로 다시 반납한다.
            Debug.Log("Release Handle");
            Addressables.Release(handleObject.handle);
            managementHandleList.Remove(releaseObject.name);
        }
        else
        {
            Debug.Log($"Exits Not {releaseObject.name}");
            return;
        }
    }

}

// Addressable Asset에서 Group Type이다.
public enum ResourceType
{
    None,
    Default,
}

// 관리할 Object struct
public struct HandleObject
{
    public AsyncOperationHandle<GameObject> handle;
    public ResourceType type;
}