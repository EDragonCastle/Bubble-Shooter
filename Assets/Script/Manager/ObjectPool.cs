using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object를 관리하는 공간
/// SetActive True, False로 관리한다.
/// </summary>
/// <typeparam name="T">Object Type</typeparam>
/// <typeparam name="CATEGORY">원하는 Category를 설정한다</typeparam>
public class ObjectPool<T, CATEGORY>
{
    private readonly CATEGORY category;
    private readonly int initalizeLength;

    // key : origin Object, value : Clone Object List
    private Dictionary<GameObject, Stack<GameObject>> pools;

    private GameObject saveObjectPools;

    // Object Pool 생성자
    #region AddressObject Object Pool Consgtrcut
    /// <summary>
    /// Object Pool 생성자를 담당한다.
    /// </summary>
    /// <param name="_category">Category 설정</param>
    /// <param name="parent">Object들을 한 곳에 담을 공간</param>
    public ObjectPool(CATEGORY _category, GameObject parent = null)
    {
        category = _category;
        initalizeLength = 5;
        pools = new Dictionary<GameObject, Stack<GameObject>>();
        saveObjectPools = parent;
        Initialize();
    }

    /// <summary>
    /// Object Pool 생성자를 담당한다.
    /// </summary>
    /// <param name="_category">Category 설정</param>
    /// <param name="length">Object Pool의 최초 Size</param>
    /// <param name="parent">Object들을 한 곳에 담을 공간</param>
    public ObjectPool(CATEGORY _category, int length, GameObject parent = null)
    {
        category = _category;
        initalizeLength = length;
        pools = new Dictionary<GameObject, Stack<GameObject>>();
        saveObjectPools = parent;
        Initialize();
    }
    #endregion

    // 생성자에서 실행할 함수
    private void Initialize()
    {
        // ElementCategory를 확인한다.
        if (typeof(CATEGORY) == typeof(BubbleCategory))
        {
            // category를 Element Category로 변경
            var ElementCategory = category as BubbleCategory;

            // Color의 개수만큼 순회하면서 Object 생성
            for (int j = 0; j < (int)BubbleColor.End; j++)
            {
                // int를 Enum 값으로 변경해서 Category에서 Origine Prefab을 가져온다.
                var originePrefab = ElementCategory.GetCategory((BubbleColor)j);
                var prefabList = new Stack<GameObject>();
                
                // 초기 size만큼 순회해서 생성한다.
                for (int i = 0; i < initalizeLength; i++)
                {
                    // prefab을 받아와서 instantiate로 생성 실제 동작할 수 있도록 생성한다.
                    GameObject copyGameObject = GameObject.Instantiate(originePrefab, saveObjectPools.transform);
                    
                    // Object 생성여부 확인
                    if (copyGameObject != null)
                    {
                        // 비활성화 시키고 Stack에 넣는다.
                        copyGameObject.SetActive(false);
                        prefabList.Push(copyGameObject);
                    }
                    else
                        Debug.LogError("Exist Not Object");
                }
                
                // 마지막에 Stack에 저장한 것을 Dictionary에 넣어서 관리한다.
                pools.Add(originePrefab, prefabList);
            }
        }
    }

    // Type에 받는 Object를 가져온다.
    public GameObject Get(T _value)
    {
        // CATEGORY type과 T Type이 연관되어 있는지 확인한다.
        if (typeof(CATEGORY) == typeof(BubbleCategory) && typeof(T) == typeof(BubbleColor))
        {
            // Category를 ElementCategory로 변경한다.
            var elementCategory = category as BubbleCategory;

            // _value 값을 ElementColor color로 변경한다.
            if(_value is BubbleColor color)
            {
                // color를 통해 Origine Prefab을 찾는다.
                var prefabs = elementCategory.GetCategory(color);

                // Prefab을 통해 object Pool에 저장되어 있는 stack을 가져온다.
                var stackObjects = pools[prefabs];

                // 미리 object 준비
                GameObject getObject = null;
                
                // stack Count가 비어 있는지 확인한다. 비어 있으면 새로 생성해서 Object를 출력하고, 비어 있지 않다면 Stack에서 하나씩 뺀다.
                if (stackObjects.Count > 0)
                {
                    getObject = stackObjects.Pop();
                    getObject.SetActive(true);
                }
                else
                {
                    getObject = GameObject.Instantiate(prefabs, saveObjectPools.transform);
                }
                return getObject;
            }
        }
     

        return null;
    }

    /// <summary>
    /// Object를 Pool로 반납한다.
    /// </summary>
    /// <param name="destoryObject">반납할 object</param>
    public void Return(GameObject destoryObject)
    {
        // 반납하는 곳의 CATEGORY를 확인한다.
        if (typeof(CATEGORY) == typeof(BubbleCategory))
        {
            // category를 ElementCategory로 변경하고 Component를 확인한다.
            var elementCategory = category as BubbleCategory;

            // 다시 초기 형태로 되돌리고 Pool에 넣는다.
            destoryObject.SetActive(false);
            var bubbleComponent = destoryObject.GetComponent<Bubble>();
            var bubbleInformation = bubbleComponent.GetBubbleInformation();
            var returnObject = elementCategory.GetCategory(bubbleInformation.bubbleColor);
            var stackObjects = pools[returnObject];
            stackObjects.Push(destoryObject);
        }

    }
}
