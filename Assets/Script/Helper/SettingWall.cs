using UnityEngine;

/// <summary>
/// 주변에 둘러싸인 벽을 세팅해주는 함수
/// </summary>
public class SettingWall : MonoBehaviour
{
    public GameObject topWall;
    public GameObject bottomWall;
    public GameObject leftWall;
    public GameObject rightWall;


    private void Start()
    {
        SetWallPosition();
    }

    /// <summary>
    /// 벽 위치를 camera의 size에 맞게 조절해준다.
    /// </summary>
    private void SetWallPosition()
    {
        Camera cam = Camera.main;

        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        Vector3 camPosition = cam.transform.position;

        // 위치 조절
        topWall.transform.position = new Vector3(camPosition.x, camPosition.y + cameraHeight, 0);
        bottomWall.transform.position = new Vector3(camPosition.x, camPosition.y - cameraHeight, 0);
        leftWall.transform.position = new Vector3(camPosition.x - cameraWidth, camPosition.y , 0);
        rightWall.transform.position = new Vector3(camPosition.x + cameraWidth, camPosition.y , 0);

        // 크기 조절
        float wallThickness = 0.5f;
        topWall.transform.localScale = new Vector3(cameraWidth * 2 + wallThickness, wallThickness, 1);
        bottomWall.transform.localScale = new Vector3(cameraWidth * 2 + wallThickness, wallThickness, 1);
        leftWall.transform.localScale = new Vector3(wallThickness, cameraHeight * 2 + wallThickness, 1);
        rightWall.transform.localScale = new Vector3(wallThickness, cameraHeight * 2 + wallThickness, 1);
    }
}
