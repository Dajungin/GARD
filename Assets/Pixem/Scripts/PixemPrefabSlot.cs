using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixemPrefabSlot : MonoBehaviour
{
    public PixemCharacterData_SO CharacterData;
    public Image CharacterPreview;
    public Text PrefabNameText;
    public Button SlotButton;

    public GameObject PrefabListPanel { get; set; }

#if UNITY_EDITOR
    private void Awake()
    {
        SlotButton.onClick.AddListener(() => 
        { 
            PixemManager.GetInstance.LoadCharacterPrefab(CharacterData);
            PrefabListPanel.SetActive(false);
        });
    }
#endif
}
