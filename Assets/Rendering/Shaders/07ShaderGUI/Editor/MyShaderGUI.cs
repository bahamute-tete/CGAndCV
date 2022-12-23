
using UnityEngine;
using UnityEditor;
using System;

public class MyShaderGUI : ShaderGUI
{
    MaterialEditor editor;
    MaterialProperty[] properties;
    Material target;

    enum SmoothnessSource {Uniform, Albedo,Metallic }

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        this.editor = editor;
        this.properties = properties;
        this.target = editor.target as Material;//MaterialEditor.target is Object type need cast 
        DoMain();
        DoSecondary();
        
    }

    

    void DoMain()
    {
        GUILayout.Label("Main Map",EditorStyles.boldLabel);

        MaterialProperty mainTex = FindProperty("_Texture");//GetProperty
        //MaterialProperty tint = FindProperty("_Tint", properties);

        //GUIContent albedoLabel = new GUIContent(mainTex.displayName, "Albedo (RGB)");//create Content

        //The TexturePropertySingleLine method has variants that work with more than one property, up to three.
        //The first should be a texture, but the others can be something else.
        //They will all be put on the same line.
        editor.TexturePropertySingleLine(MakeLable(mainTex,"Albedo(RGB)"), mainTex,FindProperty("_Tint"));//ShowProperty
        DoMetallic();
        DoSmoothness();
        DoNormals();
        DoOcclusion();
        DoEmission();
        DoDetailMask();
        editor.TextureScaleOffsetProperty(mainTex);
    }

   

    private void DoOcclusion()
    {
        MaterialProperty map = FindProperty("_OcclusionMap");

        EditorGUI.BeginChangeCheck();

        editor.TexturePropertySingleLine(MakeLable(map, "Occlusion(G)"), map, map.textureValue ?   FindProperty("_OcclusionStrength"):null);

        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_OCCLUSSION_MAP", map.textureValue);
        }
    }

    MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, properties);
    }

    static GUIContent staticLable = new GUIContent();
    static GUIContent MakeLable(MaterialProperty property, string tooltip = null)
    {
        staticLable.text = property.displayName;
        staticLable.tooltip = tooltip;
        return staticLable;
    }


    private void DoNormals()
    {
        MaterialProperty map = FindProperty("_NormalMap");

        EditorGUI.BeginChangeCheck();


        editor.TexturePropertySingleLine(MakeLable(map), map,map.textureValue? FindProperty("_BumpScale"):null);//ShowProperty

        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_NORMAL_MAP", map.textureValue);
        }
    }

    private void DoSmoothness()
    {

        SmoothnessSource source = SmoothnessSource.Uniform;

        if (IsKeyWordEnable("_SMOOTHNESS_ALBEOD"))
        {
            source = SmoothnessSource.Albedo;
        }
        else if (IsKeyWordEnable("_SMOOTHNESS_METALLIC"))
        {
            source = SmoothnessSource.Metallic;
        }

        MaterialProperty slider = FindProperty("_Smoothness");
        EditorGUI.indentLevel += 2;//indent
        editor.ShaderProperty(slider, MakeLable(slider));
        
        EditorGUI.indentLevel += 1;//indent

        EditorGUI.BeginChangeCheck();
        source = (SmoothnessSource) EditorGUILayout.EnumPopup(new GUIContent().text="Source", source);


        if (EditorGUI.EndChangeCheck())
        {
            RecordAction("Smoothness Source");
            SetKeyword("_SMOOTHNESS_ALBEOD", source == SmoothnessSource.Albedo);
            SetKeyword("_SMOOTHNESS_METALLIC", source == SmoothnessSource.Metallic);
        }
        EditorGUI.indentLevel -= 3;


    }

    private bool IsKeyWordEnable(string keyword)
    {
        return target.IsKeywordEnabled(keyword);
    }

    private void DoMetallic()
    {
        MaterialProperty map = FindProperty("_MetallicMap");

        EditorGUI.BeginChangeCheck();
        
        editor.TexturePropertySingleLine(MakeLable(map,"Metallic(R)"),map,map.textureValue? null:FindProperty("_Metallic"));

        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_METALLIC_MAP", map.textureValue);
        }
       
    }

   
    private void DoEmission()
    {
        MaterialProperty map = FindProperty("_EmissionMap");

        EditorGUI.BeginChangeCheck();

        editor.TexturePropertyWithHDRColor(MakeLable(map, "Emission(RGB)"), map,
                                            FindProperty("_Emission"),
                                            false);
              
        

        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_EMISSION_MAP", map.textureValue);
        }

    }


    private void DoDetailMask()
    {
        MaterialProperty map = FindProperty("_DetailMask");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(MakeLable(map, "DetailMask(A)"), map);

        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_DETAIL_MASK", map.textureValue);
        }
    }

    private void DoSecondary()
    {
        GUILayout.Label("Secondary Maps", EditorStyles.boldLabel);
        MaterialProperty detailTex = FindProperty("_DetailTexture");

        EditorGUI.BeginChangeCheck();

        editor.TexturePropertySingleLine(
            MakeLable(detailTex, "Albedo(RGB)multiplied by 2"), detailTex
            );


        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_DETAIL_ALBEDO_MAP", detailTex.textureValue);
        }

        DoSecondaryNormals();

        editor.TextureScaleOffsetProperty(detailTex);
    }

    void DoSecondaryNormals()
    {
        MaterialProperty map = FindProperty("_NormalDetailMap");

        EditorGUI.BeginChangeCheck();

        editor.TexturePropertySingleLine(MakeLable(map), map, map.textureValue ? FindProperty("_NormalDetailScale") : null);

        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_DETAIL_NORMAL_MAP", map.textureValue);
        }
    }


    void SetKeyword(string keyword, bool state)
    {
        if (state)
        {
            foreach (Material m in editor.targets)
            {
                m.EnableKeyword(keyword);
            }
            
        }
        else
        {
            foreach (Material m in editor.targets)
            {
                m.DisableKeyword(keyword);
            }
            
        }
    }


    void RecordAction(string label)
    {
        editor.RegisterPropertyChangeUndo(label);
    }
}
