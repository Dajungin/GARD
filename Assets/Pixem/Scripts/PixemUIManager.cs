using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PixemUIManager : MonoBehaviour
{
    private static PixemUIManager _Instance = null;
    public static PixemUIManager GetInstance
    {
        get
        {
            if (!_Instance)
            {
                GameObject obj;
                obj = GameObject.Find(typeof(PixemUIManager).Name);
                if (!obj)
                {
                    obj = new GameObject(typeof(PixemUIManager).Name);
                    _Instance = obj.AddComponent<PixemUIManager>();
                }
                else
                {
                    _Instance = obj.GetComponent<PixemUIManager>();
                }
            }

            return _Instance;
        }
    }

    public Text CharacterPrefabName;

    [Header("Main Buttons")]
    [Space]
    public Button ResetButton;
    public Button LoadButton;
    public Button MixButton;
    public Button SaveButton;
    public Button ColorPickerButton;
    public GameObject PartsPositionButton;

    [Space]
    [Header("Parts Reset Buttons")]
    [Space]
    public Button ResetButton_Head;
    public Button ResetButton_Top;
    public Button ResetButton_Pants;
    public Button ResetButton_RightHand;
    public Button ResetButton_LeftHand;
    public Button ResetButton_Cape;

    [Space]
    [Header("Animation Buttons")]
    [Space]
    public Button AnimationButton_Idle;
    public Button AnimationButton_Run;
    public Button AnimationButton_Jump;
    public Button AnimationButton_Hit;
    public Button AnimationButton_Death;
    public Button AnimationButton_Slash;
    public Button AnimationButton_Shoot;
    public Button AnimationButton_Magic;
    public Button AnimationButton_Prick;

    [Space]
    [Header("Panels")]
    [Space]
    public GameObject PrefabLoadPanel;
    public GameObject ColorPickerPanel;

#if UNITY_EDITOR
    public void Start()
    {
        // Main Buttons
        ResetButton.onClick.AddListener(PixemManager.GetInstance.AllReset);
        LoadButton.onClick.AddListener(() => { PrefabLoadPanel.SetActive(true); });
        MixButton.onClick.AddListener(PixemManager.GetInstance.AllRandomMix);
        SaveButton.onClick.AddListener(PixemManager.GetInstance.CreateCharacterPrefab);
        ColorPickerButton.onClick.AddListener(() => 
        {
            PixemManager.GetInstance.SetCurrentColor();
            ColorPickerPanel.SetActive(true);
            PixemManager.GetInstance.SetCurrentColor();
        });

        // Parts Reset Buttons
        ResetButton_Head.onClick.AddListener (() =>
            {
                PixemManager.GetInstance.ResetParts(PartsTypes.Face);
                PixemManager.GetInstance.ResetParts(PartsTypes.FaceAcc_1);
                PixemManager.GetInstance.ResetParts(PartsTypes.FaceAcc_2);
                PixemManager.GetInstance.ResetParts(PartsTypes.Hair);
                PixemManager.GetInstance.ResetParts(PartsTypes.HairAcc);
                PixemManager.GetInstance.ResetParts(PartsTypes.Helmet);
            });
        ResetButton_Top.onClick.AddListener(() => { PixemManager.GetInstance.ResetParts(PartsTypes.Top); });
        ResetButton_Pants.onClick.AddListener(() => { PixemManager.GetInstance.ResetParts(PartsTypes.Pants); });
        ResetButton_RightHand.onClick.AddListener(() => 
        { 
            PixemManager.GetInstance.ResetParts(PartsTypes.RightHandWeapon); 
            PixemManager.GetInstance.ResetParts(PartsTypes.Shield); 
        });
        ResetButton_LeftHand.onClick.AddListener(() => { PixemManager.GetInstance.ResetParts(PartsTypes.LeftHandWeapon); });
        ResetButton_Cape.onClick.AddListener(() => { PixemManager.GetInstance.ResetParts(PartsTypes.Cape); });

        // Animation Buttons
        AnimationButton_Idle.onClick.AddListener(() => { PixemManager.GetInstance.PlayTestAnimation("Idle"); });
        AnimationButton_Run.onClick.AddListener(() => { PixemManager.GetInstance.PlayTestAnimation("Run"); });
        AnimationButton_Jump.onClick.AddListener(() => { PixemManager.GetInstance.PlayTestAnimation("Jump"); });
        AnimationButton_Hit.onClick.AddListener(() => { PixemManager.GetInstance.PlayTestAnimation("Hit"); });
        AnimationButton_Death.onClick.AddListener(() => { PixemManager.GetInstance.PlayTestAnimation("Death"); });
        AnimationButton_Slash.onClick.AddListener(() => { PixemManager.GetInstance.PlayTestAnimation("AttackSlash"); });
        AnimationButton_Shoot.onClick.AddListener(() => { PixemManager.GetInstance.PlayTestAnimation("AttackShoot"); });
        AnimationButton_Magic.onClick.AddListener(() => { PixemManager.GetInstance.PlayTestAnimation("AttackMagic"); });
        AnimationButton_Prick.onClick.AddListener(() => { PixemManager.GetInstance.PlayTestAnimation("AttackPrick"); });        
    }

    public void Update()
    {
        if (PixemManager.GetInstance.SelectedPartsType == PartsTypes.Face
            || PixemManager.GetInstance.SelectedPartsType == PartsTypes.FaceAcc_1
            || PixemManager.GetInstance.SelectedPartsType == PartsTypes.FaceAcc_2
            || PixemManager.GetInstance.SelectedPartsType == PartsTypes.Hair
            || PixemManager.GetInstance.SelectedPartsType == PartsTypes.HairAcc
            || PixemManager.GetInstance.SelectedPartsType == PartsTypes.Helmet)
        {
            PartsPositionButton.gameObject.SetActive(true);
        }
        else
        {
            PartsPositionButton.gameObject.SetActive(false);
        }
    }
#endif
}
