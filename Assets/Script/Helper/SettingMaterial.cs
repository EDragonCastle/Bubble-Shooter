using UnityEngine;

/// <summary>
/// Item에 맞는 Texture를 적용하기 위해 만든 Material 클래스
/// </summary>
public class SettingMaterial : MonoBehaviour
{
    private Texture bomb;
    private Texture attack;

    private float activeItem = 0;
    /// <summary>
    /// Setting Material 생성자
    /// </summary>
    /// <param name="_bomb">bomb texture</param>
    /// <param name="_attack">attack texture</param>
    public SettingMaterial(Texture _bomb, Texture _attack)
    {
        bomb = _bomb;
        attack = _attack;
    }

    /// <summary>
    /// Material 생성하는 함수
    /// </summary>
    /// <param name="targetObject">Material을 적용할 object</param>
    /// <param name="item">어떤 item을 적용할 것인지</param>
    public void CreateMaterial(GameObject targetObject, Item item)
    {
        var renderer = targetObject.GetComponent<SpriteRenderer>();
        activeItem = 0;
        // renderer를 확인한다.
        if (renderer != null && renderer.material != null)
        {
            Material instanceMaterial = new Material(renderer.material);
            renderer.material = instanceMaterial;
            var itemTexture = ItemToTexture(item);

            // 값에 맞게 texture를 적용하고 sub texture의 사용여부를 확인한다.
            renderer.material.SetTexture("_SubTex", itemTexture);
            renderer.material.SetFloat("_UseSubTex", activeItem);
        }
    }

    /// <summary>
    /// Item Enum을 Texture로 변경해주는 함수
    /// </summary>
    /// <param name="item">Item 값</param>
    /// <returns>Texture 값</returns>
    private Texture ItemToTexture(Item item)
    {
        Texture texture = null;

        switch(item)
        {
            case Item.Attack:
                texture = attack;
                activeItem = 1.0f;
                break;
            case Item.Bomb:
                texture = bomb;
                activeItem = 1.0f;
                break;
            case Item.None:
                texture = null;
                activeItem = 0f;
                break;
        }

        return texture;
    }
    

}