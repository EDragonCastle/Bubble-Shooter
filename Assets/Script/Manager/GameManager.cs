using UnityEngine;

/// <summary>
/// Game을 실행하기 위한 필수 Manager
/// </summary>
[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private EditorData bubbleToolKit;

    [SerializeField]
    private GameObject objectPoolStore;

    // Test Object (Resource Manager에 들어갈 예정)
    public GameObject redBubble;
    public GameObject blueBubble;
    public GameObject greenBubble;
    public GameObject yellowBubble;
    public GameObject controllerBoard;
    public GameObject gameBoard;
    public GameObject settingWall;
    public GameObject title;
    public GameObject gameOver;
    public GameObject gameClear;
    public Texture attack;
    public Texture bomb;

    private void Awake()
    {
        var resourceManager = Locator<ResourceManager>.Get();

        // Manager들을 생성해서
        var bubbleCategory = new BubbleCategory(resourceManager.GetResource(ResourceType.Default, "Red Bubble"),
                                                resourceManager.GetResource(ResourceType.Default, "Blue Bubble"),
                                                resourceManager.GetResource(ResourceType.Default, "Green Bubble"),
                                                resourceManager.GetResource(ResourceType.Default, "Yellow Bubble"));
        var factory = new Factory(bubbleCategory, objectPoolStore);
        var dataMaanager = new DataManager(bubbleToolKit);
        var eventManager = new EventManager();

        var uiCategory = new UICategory(resourceManager.GetResource(ResourceType.Default, "Ttitle"),
                                        gameBoard,
                                        resourceManager.GetResource(ResourceType.Default, "GameOver"),
                                        resourceManager.GetResource(ResourceType.Default, "GameClear"),
                                        controllerBoard);
        var uiManager = new UIManager(uiCategory);

        var settingMaterial = new SettingMaterial(bomb, attack);

        // Locator에 등록한다.
        Locator<SettingMaterial>.Provide(settingMaterial);
        Locator<UIManager>.Provide(uiManager);
        Locator<EventManager>.Provide(eventManager);
        Locator<Factory>.Provide(factory);
        Locator<DataManager>.Provide(dataMaanager);
    }

    private void Start()
    {
        // board와 controller와 title 화면을 가져온다.
        var uiManager = Locator<UIManager>.Get();
        var board = uiManager.GetUICloneObject(UIType.Board);
        var controller = uiManager.GetUICloneObject(UIType.Controller);

        var resourceManager = Locator<ResourceManager>.Get();

        GameObject.Instantiate(settingWall);
        board.SetActive(false);
        controller.SetActive(false);
        uiManager.GetUICloneObject(UIType.Title);
    }

}

/*
private void Awake()
    {
        var resourceManager = Locator<ResourceManager>.Get();

        // Manager들을 생성해서
        var bubbleCategory = new BubbleCategory(resourceManager.GetResource(ResourceType.Default, "Red Bubble"),
                                                resourceManager.GetResource(ResourceType.Default, "Blue Bubble"),
                                                resourceManager.GetResource(ResourceType.Default, "Green Bubble"),
                                                resourceManager.GetResource(ResourceType.Default, "Yellow Bubble"));
        var factory = new Factory(bubbleCategory, objectPoolStore);
        var dataMaanager = new DataManager(bubbleToolKit);
        var eventManager = new EventManager();

        var uiCategory = new UICategory(resourceManager.GetResource(ResourceType.Default, "Ttitle"),
                                        resourceManager.GetResource(ResourceType.Default, "Game Board"),
                                        resourceManager.GetResource(ResourceType.Default, "GameOver"),
                                        resourceManager.GetResource(ResourceType.Default, "GameClear"),
                                        resourceManager.GetResource(ResourceType.Default, "GameClear"),);
        var uiManager = new UIManager(uiCategory);

        var settingMaterial = new SettingMaterial(bomb, attack);

        // Locator에 등록한다.
        Locator<SettingMaterial>.Provide(settingMaterial);
        Locator<UIManager>.Provide(uiManager);
        Locator<EventManager>.Provide(eventManager);
        Locator<Factory>.Provide(factory);
        Locator<DataManager>.Provide(dataMaanager);
    }

    private void Start()
    {
        // board와 controller와 title 화면을 가져온다.
        var uiManager = Locator<UIManager>.Get();
        var board = uiManager.GetUICloneObject(UIType.Board);
        var controller = uiManager.GetUICloneObject(UIType.Controller);

        var resourceManager = Locator<ResourceManager>.Get();


        board.SetActive(false);
        controller.SetActive(false);
        uiManager.GetUICloneObject(UIType.Title);
    }
 */