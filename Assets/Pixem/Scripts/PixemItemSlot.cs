using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixemItemSlot : MonoBehaviour
{
    public PartsTypes PartsType;
    public Image ItemIcon;
    public Toggle SlotToggle;
    public bool IsEmptySlot;

    public Texture2D ItemTexture
    {
        get; set;
    }

#if UNITY_EDITOR
    private void Awake()
    {
        SlotToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                PixemManager.GetInstance.ChangeItem(PartsType, ItemTexture, IsEmptySlot);
            }
        });
    }

    public void SetPreviewIcon()
    {
        Sprite previewSprite = GetPreviewSprite(new Rect(0, 0, 32, 32));

        switch (PartsType)
        {
            case PartsTypes.Prefab:

                break;
            case PartsTypes.Cape:
                break;
            case PartsTypes.Body:
                ItemIcon.rectTransform.localPosition = new Vector2(0, 10);
                break;
            case PartsTypes.Pants:
                ItemIcon.rectTransform.localPosition = new Vector2(0, 60);
                ItemIcon.rectTransform.localScale = new Vector2(1.2f, 1.2f);
                break;
            case PartsTypes.Top:
                ItemIcon.rectTransform.localPosition = new Vector2(0, 40);
                ItemIcon.rectTransform.localScale = new Vector2(1.2f, 1.2f);
                break;
            case PartsTypes.Face:
                break;
            case PartsTypes.FaceAcc_1:
                break;
            case PartsTypes.FaceAcc_2:
                break;
            case PartsTypes.Hair:
                break;
            case PartsTypes.HairAcc:
                ItemIcon.rectTransform.localPosition = new Vector2(0, -20);
                break;
            case PartsTypes.Helmet:
                ItemIcon.rectTransform.localPosition = new Vector2(0, -20);
                break;
            case PartsTypes.RightHandWeapon:
                break;
            case PartsTypes.Shield:
                ItemIcon.rectTransform.localPosition = new Vector2(35, 35f);
                break;
            case PartsTypes.LeftHandWeapon:
                ItemIcon.rectTransform.localPosition = new Vector2(0, -30f);
                ItemIcon.rectTransform.localScale = new Vector2(1.5f, 1.5f);
                break;
            default:
                break;
        }

        ItemIcon.sprite = previewSprite;
    }

    private Sprite GetPreviewSprite(Rect rectInTexture)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Items/" + PartsType.ToString() + "/" + ItemTexture.name);

        foreach (Sprite sprite in sprites)
        {
            Rect spriteRect = sprite.textureRect;
            if (ApproximatelyEqualRects(spriteRect, rectInTexture))
            {
                return sprite;
            }
        }

        return sprites[0];
    }

    private bool ApproximatelyEqualRects(Rect a, Rect b)
    {
        return Mathf.Approximately(a.x, b.x) &&
               Mathf.Approximately(a.y, b.y) &&
               Mathf.Approximately(a.width, b.width) &&
               Mathf.Approximately(a.height, b.height);
    }
#endif
}
