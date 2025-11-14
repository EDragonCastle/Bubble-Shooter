using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 쏘고 싶은 Bubble을 바꾸는 용도
/// </summary>
public class SwapController : MonoBehaviour, IPointerDownHandler
{
    public Controller controller;

    public void OnPointerDown(PointerEventData eventData)
    {
        controller.SwapObject();
    }

}
