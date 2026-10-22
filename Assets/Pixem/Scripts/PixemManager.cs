using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using HSVPicker;

public enum PartsTypes
{
    Prefab = -1,
    Cape,
    Body,
    Pants,
    Top,
    Face,
    Hair,
    HairAcc,
    Helmet,
    RightHandWeapon,
    Shield,
    LeftHandWeapon,
    FaceAcc_1,
    FaceAcc_2
}

public class PixemManager : MonoBehaviour
{

    private static PixemManager _Instance = null;
    public static PixemManager GetInstance
    {
        get
        {
            if (!_Instance)
            {
                GameObject obj;
                obj = GameObject.Find(typeof(PixemManager).Name);
                if (!obj)
                {
                    obj = new GameObject(typeof(PixemManager).Name);
                    _Instance = obj.AddComponent<PixemManager>();
                }
                else
                {
                    _Instance = obj.GetComponent<PixemManager>();
                }
            }

            return _Instance;
        }
    }

    public PixemCharacter SampleCharacter;
    public PixemSaveCharacter SaveCharacter;

    public Animator SourceAnimator;
    public GameObject TargetObject;
    public Camera CaptureCamera;
    public List<AnimationClip> ClipOrder;
    public int CellSize = 32;

    [Header("Color Picker")]
    public ColorPicker PartsColorPicker;

    [Space]
    [Header("Renderer")]
    [Space]

    public SpriteRenderer Renderer_Body;
    public SpriteRenderer Renderer_RightHand;
    public SpriteRenderer Renderer_LeftHand;

    public SpriteRenderer Renderer_Cape;
    public SpriteRenderer Renderer_Face;
    public SpriteRenderer Renderer_FaceAcc_1;
    public SpriteRenderer Renderer_FaceAcc_2;
    public SpriteRenderer Renderer_Hair;
    public SpriteRenderer Renderer_HairAcc;
    public SpriteRenderer Renderer_Helmet;
    public SpriteRenderer Renderer_Top;
    public SpriteRenderer Renderer_Pants;
    public SpriteRenderer Renderer_RightHandWeapon;
    public SpriteRenderer Renderer_Shield;
    public SpriteRenderer Renderer_LeftHandWeapon;

    [Space]
    [Header("Base Texture")]
    [Space]
    public Texture2D Base_Body;
    public Texture2D Base_Face;
    public Texture2D Empty_Cape;
    public Texture2D Empty_FaceAcc_1;
    public Texture2D Empty_FaceAcc_2;
    public Texture2D Empty_Hair;
    public Texture2D Empty_HairAcc;
    public Texture2D Empty_Helmet;
    public Texture2D Empty_Top;
    public Texture2D Empty_Pants;
    public Texture2D Empty_RightHandWeapon;
    public Texture2D Empty_Shield;
    public Texture2D Empty_LeftHandWeapon;

    private int _saveNumber = 0;
    private string _sheetSavePath;
    private string _assetSaveFolder;

    private List<int> _framesPerClip = new();

    public List<PixemSaveCharacter> CharacterPrefabList { get; protected set; }
    public Dictionary<string, Texture2D[]> ItemDictionary { get; protected set; }

    public PartsTypes SelectedPartsType
    {
        get; set;
    }

#if UNITY_EDITOR
    private void Awake()
    {
        RefreshDataNumber();
        LoadItemList();
        LoadPrefabList();
    }

    public void LoadCharacterPrefab(PixemCharacterData_SO characterData)
    {
        EditorUtility.CopySerialized(characterData, SampleCharacter.CharacterData);
        SampleCharacter.CharacterData.name = "SampleData";

        SetColor(PartsTypes.Cape, SampleCharacter.CharacterData.C_Cape);
        SetColor(PartsTypes.Face, SampleCharacter.CharacterData.C_Face);
        SetColor(PartsTypes.FaceAcc_1, SampleCharacter.CharacterData.C_FaceAcc_1);
        SetColor(PartsTypes.FaceAcc_2, SampleCharacter.CharacterData.C_FaceAcc_2);
        SetColor(PartsTypes.Hair, SampleCharacter.CharacterData.C_Hair);
        SetColor(PartsTypes.HairAcc, SampleCharacter.CharacterData.C_HairAcc);
        SetColor(PartsTypes.Helmet, SampleCharacter.CharacterData.C_Helmet);
        SetColor(PartsTypes.Top, SampleCharacter.CharacterData.C_Top);
        SetColor(PartsTypes.Pants, SampleCharacter.CharacterData.C_Pants);
        SetColor(PartsTypes.RightHandWeapon, SampleCharacter.CharacterData.C_RightHandWeapon);
        SetColor(PartsTypes.Shield, SampleCharacter.CharacterData.C_Shield);
        SetColor(PartsTypes.LeftHandWeapon, SampleCharacter.CharacterData.C_LeftHandWeapon);

        LoadPartsPositions();

        SampleCharacter.SetAnimation();
    }

    public void LoadPrefabList()
    {
        CharacterPrefabList = Resources.LoadAll<PixemSaveCharacter>("CharacterPrefabs")
            .OrderBy(prefab => ExtractTrailingNumber(prefab.name))
            .ToList();
    }

    public void LoadItemList()
    {
        ItemDictionary = new Dictionary<string, Texture2D[]>
        {
            { PartsTypes.Cape.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.Cape.ToString()) },
            { PartsTypes.Body.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.Body.ToString()) },
            { PartsTypes.Pants.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.Pants.ToString()) },
            { PartsTypes.Top.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.Top.ToString()) },
            { PartsTypes.Face.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.Face.ToString()) },
            { PartsTypes.FaceAcc_1.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.FaceAcc_1.ToString()) },
            { PartsTypes.FaceAcc_2.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.FaceAcc_2.ToString()) },
            { PartsTypes.Hair.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.Hair.ToString()) },
            { PartsTypes.HairAcc.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.HairAcc.ToString()) },
            { PartsTypes.Helmet.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.Helmet.ToString()) },
            { PartsTypes.RightHandWeapon.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.RightHandWeapon.ToString()) },
            { PartsTypes.Shield.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.Shield.ToString()) },
            { PartsTypes.LeftHandWeapon.ToString(), Resources.LoadAll<Texture2D>("Items/" + PartsTypes.LeftHandWeapon.ToString()) }
        };

        var sortedDict = new Dictionary<string, Texture2D[]>();

        foreach (var pair in ItemDictionary)
        {
            Texture2D[] sortedTextures = pair.Value
                .OrderBy(tex => IsEmpty(tex.name) ? "" : Regex.Replace(tex.name, @"_\d+$", ""))
                .ThenBy(tex => IsEmpty(tex.name) ? -1 : ExtractTrailingNumber(tex.name))
                .ToArray();

            sortedDict[pair.Key] = sortedTextures;
        }

        ItemDictionary = sortedDict;
    }

    private bool IsEmpty(string name)
    {
        return name.StartsWith("Empty_");
    }

    private int ExtractTrailingNumber(string name)
    {
        Match match = Regex.Match(name, @"_(\d+)$");
        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
    }


    public void ResetParts(PartsTypes partsType)
    {
        switch (partsType)
        {
            case PartsTypes.Cape:
                ChangeItem(PartsTypes.Cape, Empty_Cape, true);
                SetColor(PartsTypes.Cape, Color.white);
                break;
            case PartsTypes.Body:
                ChangeItem(PartsTypes.Body, Base_Body, true);
                SetColor(PartsTypes.Body, Color.white);
                break;
            case PartsTypes.Pants:
                ChangeItem(PartsTypes.Pants, Empty_Pants, true);
                SetColor(PartsTypes.Pants, Color.white);
                break;
            case PartsTypes.Top:
                ChangeItem(PartsTypes.Top, Empty_Top, true);
                SetColor(PartsTypes.Top, Color.white);
                break;
            case PartsTypes.Face:
                ChangeItem(PartsTypes.Face, Base_Face, true);
                SetColor(PartsTypes.Face, Color.white);
                break;
            case PartsTypes.FaceAcc_1:
                ChangeItem(PartsTypes.FaceAcc_1, Empty_FaceAcc_1, true);
                SetColor(PartsTypes.FaceAcc_1, Color.white);
                break;
            case PartsTypes.FaceAcc_2:
                ChangeItem(PartsTypes.FaceAcc_2, Empty_FaceAcc_2, true);
                SetColor(PartsTypes.FaceAcc_2, Color.white);
                break;
            case PartsTypes.Hair:
                ChangeItem(PartsTypes.Hair, Empty_Hair, true);
                SetColor(PartsTypes.Hair, Color.white);
                break;
            case PartsTypes.HairAcc:
                ChangeItem(PartsTypes.HairAcc, Empty_HairAcc, true);
                SetColor(PartsTypes.HairAcc, Color.white);
                break;
            case PartsTypes.Helmet:
                ChangeItem(PartsTypes.Helmet, Empty_Helmet, true);
                SetColor(PartsTypes.Helmet, Color.white);
                break;
            case PartsTypes.RightHandWeapon:
                ChangeItem(PartsTypes.RightHandWeapon, Empty_RightHandWeapon, true);
                SetColor(PartsTypes.RightHandWeapon, Color.white);
                break;
            case PartsTypes.Shield:
                ChangeItem(PartsTypes.Shield, Empty_Shield, true);
                SetColor(PartsTypes.Shield, Color.white);
                break;
            case PartsTypes.LeftHandWeapon:
                ChangeItem(PartsTypes.LeftHandWeapon, Empty_LeftHandWeapon, true);
                SetColor(PartsTypes.LeftHandWeapon, Color.white);
                break;

            default:
                break;
        }
    }

    public void AllReset()
    {
        ResetParts(PartsTypes.Body);
        ResetParts(PartsTypes.Face);
        ResetParts(PartsTypes.FaceAcc_1);
        ResetParts(PartsTypes.FaceAcc_2);
        ResetParts(PartsTypes.Hair);
        ResetParts(PartsTypes.HairAcc);
        ResetParts(PartsTypes.Helmet);
        ResetParts(PartsTypes.Top);
        ResetParts(PartsTypes.Pants);
        ResetParts(PartsTypes.RightHandWeapon);
        ResetParts(PartsTypes.Shield);
        ResetParts(PartsTypes.LeftHandWeapon);
        ResetParts(PartsTypes.Cape);

        ResetPositionsAllParts();

        SampleCharacter.SetAnimation();
    }

    public void AllRandomMix()
    {
        SetRandomParts(PartsTypes.Cape);
        SetRandomParts(PartsTypes.Body);
        SetRandomParts(PartsTypes.Top);
        SetRandomParts(PartsTypes.Pants);

        int random = Random.Range(0, 3);
        if (random == 0)
        {
            ChangeItem(PartsTypes.Shield, Empty_Shield, true);
            ChangeItem(PartsTypes.LeftHandWeapon, Empty_LeftHandWeapon, true);

            SetRandomParts(PartsTypes.RightHandWeapon);
        }
        else
        {
            ChangeItem(PartsTypes.RightHandWeapon, Empty_RightHandWeapon, true);

            SetRandomParts(PartsTypes.Shield);
            SetRandomParts(PartsTypes.LeftHandWeapon);
        }

        random = Random.Range(0, 4);
        if (random == 0)
        {
            ChangeItem(PartsTypes.Face, Base_Face, true);
            ChangeItem(PartsTypes.FaceAcc_1, Empty_FaceAcc_1, true);
            ChangeItem(PartsTypes.FaceAcc_2, Empty_FaceAcc_2, true);
            ChangeItem(PartsTypes.Hair, Empty_Hair, true);
            ChangeItem(PartsTypes.HairAcc, Empty_HairAcc, true);

            SetRandomParts(PartsTypes.Helmet);
        }
        else
        {
            ChangeItem(PartsTypes.Helmet, Empty_Helmet, true);

            SetRandomParts(PartsTypes.Face);
            SetRandomParts(PartsTypes.FaceAcc_1);
            SetRandomParts(PartsTypes.FaceAcc_2);
            SetRandomParts(PartsTypes.Hair);
            SetRandomParts(PartsTypes.HairAcc);
        }
    }

    public void SetRandomParts(PartsTypes parts)
    {
        if (!ItemDictionary.ContainsKey(parts.ToString()))
        {
            Debug.LogError("Cannot find item list for part: " + parts);
            return;
        }

        int random = Random.Range(0, ItemDictionary[parts.ToString()].Length);
        ChangeItem(parts, ItemDictionary[parts.ToString()][random], random == 0);
    }

    public void OnColorChanged(Color color)
    {
        SetColor(SelectedPartsType, color);
    }

    public void ResetColor()
    {
        PartsColorPicker.AssignColor(Color.white);
    }

    public void SetCurrentColor()
    {
        PartsColorPicker.CurrentColor = GetColor(SelectedPartsType);
    }

    public Color GetColor(PartsTypes parts)
    {
        switch (parts)
        {
            case PartsTypes.Body:
                return Renderer_Body.color;
            case PartsTypes.Cape:
                return Renderer_Cape.color;
            case PartsTypes.Pants:
                return Renderer_Pants.color;
            case PartsTypes.Top:
                return Renderer_Top.color;
            case PartsTypes.Face:
                return Renderer_Face.color;
            case PartsTypes.FaceAcc_1:
                return Renderer_FaceAcc_1.color;
            case PartsTypes.FaceAcc_2:
                return Renderer_FaceAcc_2.color;
            case PartsTypes.Hair:
                return Renderer_Hair.color;
            case PartsTypes.HairAcc:
                return Renderer_HairAcc.color;
            case PartsTypes.Helmet:
                return Renderer_Helmet.color;
            case PartsTypes.RightHandWeapon:
                return Renderer_RightHandWeapon.color;
            case PartsTypes.Shield:
                return Renderer_Shield.color;
            case PartsTypes.LeftHandWeapon:
                return Renderer_LeftHandWeapon.color;
        }

        return Color.white;
    }

    public void SetColor(PartsTypes partsType, Color color)
    {
        switch (partsType)
        {
            case PartsTypes.Body:
                Renderer_Body.color = color;
                Renderer_RightHand.color = color;
                Renderer_LeftHand.color = color;
                SampleCharacter.CharacterData.C_Body = color;
                break;
            case PartsTypes.Cape:
                Renderer_Cape.color = color;
                SampleCharacter.CharacterData.C_Cape = color;
                break;
            case PartsTypes.Pants:
                Renderer_Pants.color = color;
                SampleCharacter.CharacterData.C_Pants = color;
                break;
            case PartsTypes.Top:
                Renderer_Top.color = color;
                SampleCharacter.CharacterData.C_Top = color;
                break;
            case PartsTypes.Face:
                Renderer_Face.color = color;
                SampleCharacter.CharacterData.C_Face = color;
                break;
            case PartsTypes.FaceAcc_1:
                Renderer_FaceAcc_1.color = color;
                SampleCharacter.CharacterData.C_FaceAcc_1 = color;
                break;
            case PartsTypes.FaceAcc_2:
                Renderer_FaceAcc_2.color = color;
                SampleCharacter.CharacterData.C_FaceAcc_2 = color;
                break;
            case PartsTypes.Hair:
                Renderer_Hair.color = color;
                SampleCharacter.CharacterData.C_Hair = color;
                break;
            case PartsTypes.HairAcc:
                Renderer_HairAcc.color = color;
                SampleCharacter.CharacterData.C_HairAcc = color;
                break;
            case PartsTypes.Helmet:
                Renderer_Helmet.color = color;
                SampleCharacter.CharacterData.C_Helmet = color;
                break;
            case PartsTypes.RightHandWeapon:
                Renderer_RightHandWeapon.color = color;
                SampleCharacter.CharacterData.C_RightHandWeapon = color;
                break;
            case PartsTypes.Shield:
                Renderer_Shield.color = color;
                SampleCharacter.CharacterData.C_Shield = color;
                break;
            case PartsTypes.LeftHandWeapon:
                Renderer_LeftHandWeapon.color = color;
                SampleCharacter.CharacterData.C_LeftHandWeapon = color;
                break;
            default:
                break;
        }
    }

    public void LoadPartsPositions()
    {
        Renderer_Face.transform.localPosition = SampleCharacter.CharacterData.P_Face;
        Renderer_FaceAcc_1.transform.localPosition = SampleCharacter.CharacterData.P_FaceAcc_1;
        Renderer_FaceAcc_2.transform.localPosition = SampleCharacter.CharacterData.P_FaceAcc_2;
        Renderer_Hair.transform.localPosition = SampleCharacter.CharacterData.P_Hair;
        Renderer_HairAcc.transform.localPosition = SampleCharacter.CharacterData.P_HairAcc;
        Renderer_Helmet.transform.localPosition = SampleCharacter.CharacterData.P_Helmet;
    }

    public void ResetPosision()
    {
        switch (SelectedPartsType)
        {
            case PartsTypes.Face:
                Renderer_Face.transform.localPosition = Vector3.zero;
                SampleCharacter.CharacterData.P_Face = Renderer_Face.transform.localPosition;
                break;
            case PartsTypes.Hair:
                Renderer_Hair.transform.localPosition = Vector3.zero;
                SampleCharacter.CharacterData.P_Hair = Renderer_Hair.transform.localPosition;
                break;
            case PartsTypes.HairAcc:
                Renderer_HairAcc.transform.localPosition = Vector3.zero;
                SampleCharacter.CharacterData.P_HairAcc = Renderer_HairAcc.transform.localPosition;
                break;
            case PartsTypes.Helmet:
                Renderer_Helmet.transform.localPosition = Vector3.zero;
                SampleCharacter.CharacterData.P_Helmet = Renderer_Helmet.transform.localPosition;
                break;
            case PartsTypes.FaceAcc_1:
                Renderer_FaceAcc_1.transform.localPosition = Vector3.zero;
                SampleCharacter.CharacterData.P_FaceAcc_1 = Renderer_FaceAcc_1.transform.localPosition;
                break;
            case PartsTypes.FaceAcc_2:
                Renderer_FaceAcc_2.transform.localPosition = Vector3.zero;
                SampleCharacter.CharacterData.P_FaceAcc_2 = Renderer_FaceAcc_2.transform.localPosition;
                break;
            default:
                break;
        }
    }

    public void ResetPositionsAllParts()
    {
        Renderer_Face.transform.localPosition = Vector3.zero;
        Renderer_Hair.transform.localPosition = Vector3.zero;
        Renderer_HairAcc.transform.localPosition = Vector3.zero;
        Renderer_Helmet.transform.localPosition = Vector3.zero;
        Renderer_FaceAcc_1.transform.localPosition = Vector3.zero;
        Renderer_FaceAcc_2.transform.localPosition = Vector3.zero;

        SampleCharacter.CharacterData.P_Face = Renderer_Face.transform.localPosition;
        SampleCharacter.CharacterData.P_Hair = Renderer_Hair.transform.localPosition;
        SampleCharacter.CharacterData.P_HairAcc = Renderer_HairAcc.transform.localPosition;
        SampleCharacter.CharacterData.P_Helmet = Renderer_Helmet.transform.localPosition;
        SampleCharacter.CharacterData.P_FaceAcc_1 = Renderer_FaceAcc_1.transform.localPosition;
        SampleCharacter.CharacterData.P_FaceAcc_2 = Renderer_FaceAcc_2.transform.localPosition;
    }

    public void SetVerticalPosParts(float dir)
    {
        switch (SelectedPartsType)
        {
            case PartsTypes.Face:
                Renderer_Face.transform.Translate(Vector3.up * 0.03125f * dir);
                SampleCharacter.CharacterData.P_Face = Renderer_Face.transform.localPosition;
                break;
            case PartsTypes.Hair:
                Renderer_Hair.transform.Translate(Vector3.up * 0.03125f * dir);
                SampleCharacter.CharacterData.P_Hair = Renderer_Hair.transform.localPosition;
                break;
            case PartsTypes.HairAcc:
                Renderer_HairAcc.transform.Translate(Vector3.up * 0.03125f * dir);
                SampleCharacter.CharacterData.P_HairAcc = Renderer_HairAcc.transform.localPosition;
                break;
            case PartsTypes.Helmet:
                Renderer_Helmet.transform.Translate(Vector3.up * 0.03125f * dir);
                SampleCharacter.CharacterData.P_Helmet = Renderer_Helmet.transform.localPosition;
                break;
            case PartsTypes.FaceAcc_1:
                Renderer_FaceAcc_1.transform.Translate(Vector3.up * 0.03125f * dir);
                SampleCharacter.CharacterData.P_FaceAcc_1 = Renderer_FaceAcc_1.transform.localPosition;
                break;
            case PartsTypes.FaceAcc_2:
                Renderer_FaceAcc_2.transform.Translate(Vector3.up * 0.03125f * dir);
                SampleCharacter.CharacterData.P_FaceAcc_2 = Renderer_FaceAcc_2.transform.localPosition;
                break;
            default:
                break;
        }
    }

    public void SetHorizonPosParts(float dir)
    {
        switch (SelectedPartsType)
        {
            case PartsTypes.Face:
                Renderer_Face.transform.Translate(Vector3.right * 0.03125f * dir);
                SampleCharacter.CharacterData.P_Face = Renderer_Face.transform.localPosition;
                break;
            case PartsTypes.Hair:
                Renderer_Hair.transform.Translate(Vector3.right * 0.03125f * dir);
                SampleCharacter.CharacterData.P_Hair = Renderer_Face.transform.localPosition;
                break;
            case PartsTypes.HairAcc:
                Renderer_HairAcc.transform.Translate(Vector3.right * 0.03125f * dir);
                SampleCharacter.CharacterData.P_HairAcc = Renderer_Face.transform.localPosition;
                break;
            case PartsTypes.Helmet:
                Renderer_Helmet.transform.Translate(Vector3.right * 0.03125f * dir);
                SampleCharacter.CharacterData.P_Helmet = Renderer_Face.transform.localPosition;
                break;
            case PartsTypes.FaceAcc_1:
                Renderer_FaceAcc_1.transform.Translate(Vector3.right * 0.03125f * dir);
                SampleCharacter.CharacterData.P_FaceAcc_1 = Renderer_Face.transform.localPosition;
                break;
            case PartsTypes.FaceAcc_2:
                Renderer_FaceAcc_2.transform.Translate(Vector3.right * 0.03125f * dir);
                SampleCharacter.CharacterData.P_FaceAcc_2 = Renderer_Face.transform.localPosition;
                break;
            default:
                break;
        }
    }

    public void ChangeItem(PartsTypes partsType, Texture2D itemTexture, bool isEmpty)
    {
        switch (partsType)
        {
            case PartsTypes.Cape:
                SampleCharacter.CharacterData.T_Cape = itemTexture;
                break;
            case PartsTypes.Body:
                SampleCharacter.CharacterData.T_Body = itemTexture;
                break;
            case PartsTypes.Pants:
                SampleCharacter.CharacterData.T_Pants = itemTexture;
                break;
            case PartsTypes.Top:
                SampleCharacter.CharacterData.T_Top = itemTexture;
                break;
            case PartsTypes.Face:
                SampleCharacter.CharacterData.T_Face = itemTexture;
                if (!isEmpty)
                {
                    SampleCharacter.CharacterData.T_Helmet = Empty_Helmet;
                }
                break;
            case PartsTypes.FaceAcc_1:
                SampleCharacter.CharacterData.T_FaceAcc_1 = itemTexture;
                if (!isEmpty)
                {
                    SampleCharacter.CharacterData.T_Helmet = Empty_Helmet;
                }
                break;
            case PartsTypes.FaceAcc_2:
                SampleCharacter.CharacterData.T_FaceAcc_2 = itemTexture;
                if (!isEmpty)
                {
                    SampleCharacter.CharacterData.T_Helmet = Empty_Helmet;
                }
                break;
            case PartsTypes.Hair:
                SampleCharacter.CharacterData.T_Hair = itemTexture;
                if (!isEmpty)
                {
                    SampleCharacter.CharacterData.T_Helmet = Empty_Helmet;
                }
                break;
            case PartsTypes.HairAcc:
                SampleCharacter.CharacterData.T_HairAcc = itemTexture;
                if (!isEmpty)
                {
                    SampleCharacter.CharacterData.T_Helmet = Empty_Helmet;
                }
                break;
            case PartsTypes.Helmet:
                if (!isEmpty)
                {
                    SampleCharacter.CharacterData.T_Face = Base_Face;
                    SampleCharacter.CharacterData.T_FaceAcc_1 = Empty_FaceAcc_1;
                    SampleCharacter.CharacterData.T_Hair = Empty_Hair;
                    SampleCharacter.CharacterData.T_HairAcc = Empty_HairAcc;
                }
                SampleCharacter.CharacterData.T_Helmet = itemTexture;
                break;
            case PartsTypes.RightHandWeapon:
                SampleCharacter.CharacterData.T_RightHandWeapon = itemTexture;
                if (!isEmpty)
                {
                    SampleCharacter.CharacterData.T_Shield = Empty_Shield;
                }
                break;
            case PartsTypes.Shield:
                if (!isEmpty)
                {
                    SampleCharacter.CharacterData.T_RightHandWeapon = Empty_RightHandWeapon;
                }
                SampleCharacter.CharacterData.T_Shield = itemTexture;
                break;
            case PartsTypes.LeftHandWeapon:
                SampleCharacter.CharacterData.T_LeftHandWeapon = itemTexture;
                break;
            default:
                break;
        }

        SampleCharacter.SetAnimation();
    }


    public void RefreshDataNumber()
    {
        GameObject[] dataArray = Resources.LoadAll<GameObject>("CharacterPrefabs");
        _saveNumber = dataArray.Length;

        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Pixem/Resources/CharacterPrefabs/Character_" + _saveNumber + ".prefab");

        while (existingPrefab != null)
        {
            _saveNumber++;

            existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Pixem/Resources/CharacterPrefabs/Character_" + _saveNumber + ".prefab");
        }

        PixemUIManager.GetInstance.CharacterPrefabName.text = "Character_" + _saveNumber;
    }

    public void CreateCharacterPrefab()
    {
        ProcessAll();

        string newDataPath = GetCharacterFolderPath() + "/CharacterData_" + _saveNumber + ".asset";

        PixemCharacterData_SO asset = ScriptableObject.CreateInstance<PixemCharacterData_SO>();
        EditorUtility.CopySerialized(SampleCharacter.CharacterData, asset);
        AssetDatabase.CreateAsset(asset, newDataPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        GameObject saveInstance = Instantiate(SaveCharacter.gameObject);
        var instanceScript = saveInstance.GetComponent<PixemSaveCharacter>();
        instanceScript.CharacterData = asset;
        instanceScript.CharacterAnimator.runtimeAnimatorController = SaveCharacter.CharacterAnimator.runtimeAnimatorController;
        if (SaveCharacter.Body.sprite != null)
        {
            instanceScript.Body.sprite = SaveCharacter.Body.sprite;
        }
        saveInstance.SetActive(true);

        string prefabName = $"Character_{_saveNumber}.prefab";
        string prefabPath = $"Assets/Pixem/Resources/CharacterPrefabs/{prefabName}";
        string folderPath = System.IO.Path.GetDirectoryName(prefabPath);

        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(saveInstance.gameObject, prefabPath);
        GameObject.DestroyImmediate(saveInstance);

        Debug.Log($"<color=lime><b> New character created:</b></color> <color=orange><b>{prefabName}</b></color>");

        RefreshDataNumber();
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
    }

    public string GetCharacterFolderPath()
    {
        return $"Assets/Pixem/Resources/CharacterResources/Character_{_saveNumber}";
    }

    public void ProcessAll()
    {
#if UNITY_EDITOR
        string characterFolder = GetCharacterFolderPath();
        if (!Directory.Exists(characterFolder))
            Directory.CreateDirectory(characterFolder);

        // 경로를 Character 폴더로 변경
        _sheetSavePath = Path.Combine(characterFolder, "Character_" + _saveNumber + "_Sheet.png");
        _assetSaveFolder = characterFolder + "/Animations";
        Directory.CreateDirectory(_assetSaveFolder);

        Texture2D sheet = CaptureSpriteSheet();
        SliceSpriteSheet(sheet);
        CreateModifiedAnimatorWithSheetSprites(sheet);
#endif
    }

    private Texture2D CaptureSpriteSheet()
    {
        List<Texture2D> allFrames = new();
        _framesPerClip.Clear();

        var orderedClips = new List<AnimationClip>();
        for (int i = 0; i < ClipOrder.Count; i++)
        {
            orderedClips.Add(SampleCharacter.GetModifiedClipByName(ClipOrder[i].name));
        }
        orderedClips.Reverse();

        foreach (var clip in orderedClips)
        {
            HashSet<float> times = new();
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName != "m_Sprite") continue;
                var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                foreach (var kf in keyframes) times.Add(kf.time);
            }

            List<float> sorted = times.OrderBy(t => t).ToList();
            if (sorted.Count > 1)
                sorted.RemoveAt(sorted.Count - 1);

            List<Texture2D> frames = new();
            foreach (var t in sorted)
            {
                clip.SampleAnimation(TargetObject, t);
                frames.Add(CaptureFrame());
            }

            allFrames.AddRange(frames);
            _framesPerClip.Add(frames.Count);
        }
        int cols = _framesPerClip.Max();
        int rows = _framesPerClip.Count;

        Texture2D sheet = new(cols * CellSize, rows * CellSize, TextureFormat.RGBA32, false);
        Color[] clear = Enumerable.Repeat(new Color(0, 0, 0, 0), sheet.width * sheet.height).ToArray();
        sheet.SetPixels(clear);

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            int count = _framesPerClip[row];
            for (int col = 0; col < count; col++)
            {
                Texture2D frame = allFrames[index++];
                sheet.SetPixels(col * CellSize, (rows - row - 1) * CellSize, CellSize, CellSize, frame.GetPixels());
            }
        }

        sheet.Apply();
        File.WriteAllBytes(_sheetSavePath, sheet.EncodeToPNG());
        AssetDatabase.ImportAsset(_sheetSavePath);

        TextureImporter importer = AssetImporter.GetAtPath(_sheetSavePath) as TextureImporter;
        if (importer != null)
        {
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 1024;
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(_sheetSavePath);
    }

    private Texture2D CaptureFrame()
    {
        RenderTexture rt = new(CellSize, CellSize, 24);
        CaptureCamera.targetTexture = rt;
        RenderTexture.active = rt;
        CaptureCamera.Render();

        Texture2D tex = new(CellSize, CellSize, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, CellSize, CellSize), 0, 0);
        tex.Apply();

        CaptureCamera.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
        return tex;
    }

    private void SliceSpriteSheet(Texture2D sheet)
    {
        string path = AssetDatabase.GetAssetPath(sheet);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 32;

        int cols = sheet.width / CellSize;
        int rows = sheet.height / CellSize;

        var reversedClipOrder = new List<AnimationClip>(ClipOrder);
        reversedClipOrder.Reverse();

        List<SpriteMetaData> metas = new();
        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            string clipName = reversedClipOrder[row].name;
            int frameCount = _framesPerClip[row];

            for (int col = 0; col < frameCount; col++)
            {
                metas.Add(new SpriteMetaData
                {
                    name = $"{clipName}_{col}",
                    rect = new Rect(col * CellSize, sheet.height - (row + 1) * CellSize, CellSize, CellSize),
                    pivot = new Vector2(0.6875f, 0.125f),
                    alignment = (int)SpriteAlignment.Custom
                });
                index++;
            }
        }

        importer.spritesheet = metas.ToArray();
        importer.SaveAndReimport();

        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        Sprite targetSprite = sprites
            .OfType<Sprite>()
            .FirstOrDefault(s => s.name == "Idle_0");

        if (targetSprite != null)
        {
            SaveCharacter.Body.sprite = targetSprite;
        }
    }

    private void CreateModifiedAnimatorWithSheetSprites(Texture2D sheet)
    {
        Sprite[] sheetSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(sheet))
            .OfType<Sprite>().ToArray();

        RuntimeAnimatorController source = SourceAnimator.runtimeAnimatorController;
        var overrideController = new AnimatorOverrideController(source);

        Dictionary<AnimationClip, AnimationClip> mapping = new();

        foreach (var clip in source.animationClips)
        {
            AnimationClip newClip = new AnimationClip();
            EditorUtility.CopySerialized(clip, newClip);

            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(newClip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName == "m_Sprite")
                {
                    if (!binding.path.Contains("Body"))
                    {
                        AnimationUtility.SetObjectReferenceCurve(newClip, binding, null);
                        continue;
                    }
                }
                else
                {
                    AnimationUtility.SetObjectReferenceCurve(newClip, binding, null);
                    continue;
                }

                var keyframes = AnimationUtility.GetObjectReferenceCurve(newClip, binding);
                for (int i = 0; i < keyframes.Length; i++)
                {
                    string expectedName;
                    if (i == keyframes.Length - 1)
                    {
                        expectedName = $"{clip.name}_{i - 1}";
                    }
                    else
                    {
                        expectedName = $"{clip.name}_{i}";
                    }
                    Sprite match = sheetSprites.FirstOrDefault(s => s.name == expectedName);
                    if (match != null)
                    {
                        keyframes[i].value = match;
                    }
                    else
                    {
                        Debug.LogWarning("Cannot find matching sprite in scene: " + expectedName);
                    }
                }

                var floatBindings = AnimationUtility.GetCurveBindings(newClip);
                foreach (var floatBinding in floatBindings)
                {
                    AnimationUtility.SetEditorCurve(newClip, floatBinding, null);
                }

                AnimationUtility.SetObjectReferenceCurve(newClip, binding, keyframes);
            }

            string clipPath = $"{_assetSaveFolder}/{clip.name}_Modified.anim";
            AssetDatabase.CreateAsset(newClip, clipPath);
            mapping[clip] = newClip;
        }

        foreach (var pair in mapping)
            overrideController[pair.Key.name] = pair.Value;

        string controllerPath = $"{_assetSaveFolder}/{source.name}_Modified.controller";
        AssetDatabase.CreateAsset(overrideController, controllerPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SaveCharacter.CharacterAnimator.runtimeAnimatorController = overrideController;
    }

    public void PlayTestAnimation(string value)
    {
        SampleCharacter.PlayTestAnimation(value);
    }

#endif
}
