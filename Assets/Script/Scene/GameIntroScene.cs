using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// Loading Scene을 담당하는 클래스
/// </summary>
public class GameIntroScene : MonoBehaviour
{
    // Loading Bar와 어떤 값이 Load 되고 있는지 
    [SerializeField]
    private Slider loadingBar;
    [SerializeField]
    private TMP_Text loadingBarText;
    [SerializeField]
    private TMP_Text loadingText;

    private async void Awake()
    {
        // 시작 시 Resource를 Load 한다.
        await Initalize();
    }

    public async UniTask Initalize()
    {
        var resourceManager = new ResourceManager();
        await resourceManager.Load();

        Locator<ResourceManager>.Provide(resourceManager);

        var sceneHandle = Addressables.LoadSceneAsync("MainBoard", UnityEngine.SceneManagement.LoadSceneMode.Single, activateOnLoad: false);

        // Scene이 Load가 될 때까지 Loop를 돈다.
        while (!sceneHandle.IsDone)
        {
            // PercentComplete는 로딩의 전체 진행률을 나타낸다.
            await UniTask.Yield();
        }

        // Load가 끝나면 다음 Frame에 Scene으로 이동한다.
        await UniTask.Yield();

        await sceneHandle.Result.ActivateAsync();
    }

    /// <summary>
    /// 실제 게임으로 가는 버튼
    /// </summary>
    public void GameStartButton()
    {
        SceneManager.LoadScene("MainBoard");
    }
}
