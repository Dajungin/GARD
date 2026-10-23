using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Zombie Data", menuName = "Scriptable Object/Zombie Data", order = int.MaxValue)]
public class PixemCharacterData_SO : ScriptableObject
{
    // Back
    public Texture2D T_Cape;

    // Body
    public Texture2D T_Body;

    // Head
    public Texture2D T_Face;
    public Texture2D T_FaceAcc_1;
    public Texture2D T_FaceAcc_2;
    public Texture2D T_Hair;
    public Texture2D T_HairAcc;
    public Texture2D T_Helmet;

    // Top
    public Texture2D T_Top;

    // Pants
    public Texture2D T_Pants;

    // RightHand
    public Texture2D T_RightHandWeapon;
    public Texture2D T_Shield;
        
    // LeftHand
    public Texture2D T_LeftHandWeapon;

    // Color
    public Color C_Body;
    public Color C_Cape;
    public Color C_Face;
    public Color C_FaceAcc_1;
    public Color C_FaceAcc_2;
    public Color C_Hair;
    public Color C_HairAcc;
    public Color C_Helmet;
    public Color C_Top;
    public Color C_Pants;
    public Color C_RightHandWeapon;
    public Color C_Shield;
    public Color C_LeftHandWeapon;

    // Pos
    public Vector3 P_Face;
    public Vector3 P_FaceAcc_1;
    public Vector3 P_FaceAcc_2;
    public Vector3 P_Hair;
    public Vector3 P_HairAcc;
    public Vector3 P_Helmet;


    public Texture2D GetTexture(PartsTypes partsType)
    {
        Texture2D texture = null;

        switch (partsType)
        {
            case PartsTypes.Cape:
                texture = T_Cape;
                break;
            case PartsTypes.Body:
                texture = T_Body;
                break;
            case PartsTypes.Pants:
                texture = T_Pants;
                break;
            case PartsTypes.Top:
                texture = T_Top;
                break;
            case PartsTypes.Face:
                texture = T_Face;
                break;
            case PartsTypes.FaceAcc_1:
                texture = T_FaceAcc_1;
                break;
            case PartsTypes.FaceAcc_2:
                texture = T_FaceAcc_2;
                break;
            case PartsTypes.Hair:
                texture = T_Hair;
                break;
            case PartsTypes.HairAcc:
                texture = T_HairAcc;
                break;
            case PartsTypes.Helmet:
                texture = T_Helmet;
                break;
            case PartsTypes.RightHandWeapon:
                texture = T_RightHandWeapon;
                break;
            case PartsTypes.Shield:
                texture = T_Shield;
                break;
            case PartsTypes.LeftHandWeapon:
                texture = T_LeftHandWeapon;
                break;
            default:
                break;
        }

        return texture;
    }
}
