using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixemItemListPanel : MonoBehaviour
{
    public PartsTypes PanelPartsType;

    public PixemItemSlot ItemSlotPrefab;
    public GameObject Content;
    public ToggleGroup SlotToggleGroup;

    private List<PixemItemSlot> _itemList;
    private ScrollRect _scrollRect;

#if UNITY_EDITOR
    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void LoadItem()
    {
        ClearChildren(Content.transform);

        if (!PixemManager.GetInstance.ItemDictionary.ContainsKey(PanelPartsType.ToString()))
        {
            Debug.LogError("Failed to load item list for part: " + PanelPartsType);
            return;
        }

        Texture2D[] textures = PixemManager.GetInstance.ItemDictionary[PanelPartsType.ToString()];

        for (int i = 0; i < textures.Length; i++)
        {
            PixemItemSlot slot = Instantiate(ItemSlotPrefab, Content.transform);

            slot.PartsType = PanelPartsType;
            slot.SlotToggle.group = SlotToggleGroup;
            slot.ItemTexture = textures[i];

            if (i == 0)
            {
                slot.IsEmptySlot = true;
            }

            slot.SetPreviewIcon();

            if (PixemManager.GetInstance.SampleCharacter.CharacterData.GetTexture(PanelPartsType) == textures[i])
            {
                slot.SlotToggle.isOn = true;
            }
        }

    }

    private void OnEnable()
    {
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 1f;

        LoadItem();

        PixemManager.GetInstance.SelectedPartsType = PanelPartsType;
    }
#endif
}
