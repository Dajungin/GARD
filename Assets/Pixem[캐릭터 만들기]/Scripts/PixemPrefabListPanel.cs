using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixemPrefabListPanel : MonoBehaviour
{
    public PixemPrefabSlot PrefabSlotPrefab;
    public GameObject Content;

    private ScrollRect _scrollRect;

#if UNITY_EDITOR
    private void Awake()
    {
        _scrollRect = GetComponentInChildren<ScrollRect>();
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void LoadPrefabs()
    {
        PixemManager.GetInstance.LoadPrefabList();

        ClearChildren(Content.transform);

        for (int i = 0; i < PixemManager.GetInstance.CharacterPrefabList.Count; i++)
        {
            PixemPrefabSlot slot = Instantiate(PrefabSlotPrefab, Content.transform);
            slot.PrefabListPanel = this.gameObject;
            slot.CharacterData = PixemManager.GetInstance.CharacterPrefabList[i].CharacterData;
            slot.CharacterPreview.sprite = PixemManager.GetInstance.CharacterPrefabList[i].Body.sprite;
            slot.PrefabNameText.text = PixemManager.GetInstance.CharacterPrefabList[i].name;
        }
    }

    private void OnEnable()
    {
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 1f;

        LoadPrefabs();
    }
#endif
}
