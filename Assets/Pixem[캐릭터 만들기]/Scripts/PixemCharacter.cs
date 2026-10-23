using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PixemCharacter : MonoBehaviour
{
    public PixemCharacterData_SO CharacterData;
    public Animator CharacterAnimator;

    protected AnimatorOverrideController _overrideController;
    protected List<AnimationClip> _modifiedClips = new List<AnimationClip>();

#if UNITY_EDITOR
    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        CharacterData.T_Body = PixemManager.GetInstance.Base_Body;
        CharacterData.T_Cape = PixemManager.GetInstance.Empty_Cape;
        CharacterData.T_Face = PixemManager.GetInstance.Base_Face;
        CharacterData.T_FaceAcc_1 = PixemManager.GetInstance.Empty_FaceAcc_1;
        CharacterData.T_FaceAcc_2 = PixemManager.GetInstance.Empty_FaceAcc_2;
        CharacterData.T_Hair = PixemManager.GetInstance.Empty_Hair;
        CharacterData.T_HairAcc = PixemManager.GetInstance.Empty_HairAcc;
        CharacterData.T_Helmet = PixemManager.GetInstance.Empty_Helmet;
        CharacterData.T_Top = PixemManager.GetInstance.Empty_Top;
        CharacterData.T_Pants = PixemManager.GetInstance.Empty_Pants;
        CharacterData.T_RightHandWeapon = PixemManager.GetInstance.Empty_RightHandWeapon;
        CharacterData.T_Shield = PixemManager.GetInstance.Empty_Shield;
        CharacterData.T_LeftHandWeapon = PixemManager.GetInstance.Empty_LeftHandWeapon;

        CharacterData.C_Cape = Color.white;
        CharacterData.C_Face = Color.white;
        CharacterData.C_FaceAcc_1 = Color.white;
        CharacterData.C_FaceAcc_2 = Color.white;
        CharacterData.C_Hair = Color.white;
        CharacterData.C_HairAcc = Color.white;
        CharacterData.C_Helmet = Color.white;
        CharacterData.C_Top = Color.white;
        CharacterData.C_Pants = Color.white;
        CharacterData.C_RightHandWeapon = Color.white;
        CharacterData.C_Shield = Color.white;
        CharacterData.C_LeftHandWeapon = Color.white;
    }

    public void PlayTestAnimation(string value)
    {
        CharacterAnimator.SetTrigger(value);
    }

    public void SetAnimation()
    {
        if (CharacterAnimator == null)
        { 
            CharacterAnimator = GetComponent<Animator>();
        }
        _overrideController = new AnimatorOverrideController(CharacterAnimator.runtimeAnimatorController);

        if (CharacterAnimator == null || CharacterAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("Animator or Controller is null");
            return;
        }

        RuntimeAnimatorController controller = CharacterAnimator.runtimeAnimatorController;
        AnimationClip[] clips = controller.animationClips;

        _modifiedClips.Clear();

        foreach (var clip in clips)
        {
            ReplaceSprites(clip);
        }
    }

    private void ReplaceSprites(AnimationClip clip)
    {
        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        AnimationClip uniqueClip = Instantiate(clip);
        uniqueClip.name = clip.name;

        _modifiedClips.Add(uniqueClip);

        foreach (var binding in bindings)
        {
            string[] parts = binding.path.Split('/');
            string last = parts[parts.Length - 1];

            if (last == "RightHand"
                || last == "LeftHand"
                || last == "LeftArm")
            {
                last = "Body";
            }

            PartsTypes partsType;
            if (!Enum.TryParse(last, true, out partsType)) continue;

            Texture2D texture = CharacterData.GetTexture(partsType);
            if (texture == null) continue;

            var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            for (int i = 0; i < keyframes.Length; i++)
            {
                Sprite originalSprite = keyframes[i].value as Sprite;
                if (originalSprite != null)
                {
                    Rect rectInTexture = originalSprite.textureRect;
                    Sprite[] sprites = Resources.LoadAll<Sprite>("Items/" + partsType.ToString() + "/" + texture.name);

                    foreach (Sprite sprite in sprites)
                    {
                        Rect spriteRect = sprite.textureRect;
                        if (ApproximatelyEqualRects(spriteRect, rectInTexture))
                        {
                            keyframes[i].value = sprite;
                            break;
                        }
                    }
                }
            }
            AnimationUtility.SetObjectReferenceCurve(uniqueClip, binding, keyframes);
        }

        _overrideController[clip.name] = uniqueClip;
        CharacterAnimator.runtimeAnimatorController = _overrideController;
    }

    public AnimationClip GetModifiedClipByName(string clipName)
    {
        return _modifiedClips.Find(x => x.name == clipName);
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
