
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;//contains the RenderQueue enum, which contains the correct values.

public class RealTimeGIGUI : ShaderGUI
{
    MaterialEditor editor;
    MaterialProperty[] properties;
    Material target;

    enum SmoothnessSource {Uniform, Albedo,Metallic }
    enum RenderingMode {Opaque,Cutout,Fade , Transparent }

    bool ShouldShowAlphaCutoff;

    struct RenderingSettings
    {
        public RenderQueue queue;
        public string renderType;

        public BlendMode srcBlend, dstBlend;
        public bool zWrite;


        public static RenderingSettings[] modes =
            {
                new RenderingSettings() {
                    queue = RenderQueue.Geometry,
                    renderType = "",
                    srcBlend = BlendMode.One,
                    dstBlend = BlendMode.Zero,
                    zWrite = true
                    },

                new RenderingSettings(){
                    queue = RenderQueue.AlphaTest,
                    renderType = "TransparentCutout",
                    srcBlend = BlendMode.One,
                    dstBlend = BlendMode.Zero,
                    zWrite = true
                    },

                new RenderingSettings(){
                    queue = RenderQueue.Transparent,
                    renderType = "Transparent",
                    srcBlend = BlendMode.SrcAlpha,
                    dstBlend = BlendMode.OneMinusSrcAlpha,
                    zWrite = false
                    },

                  new RenderingSettings(){
                    queue = RenderQueue.Transparent,
                    renderType = "Transparent",
                    srcBlend = BlendMode.One,
                    dstBlend = BlendMode.OneMinusSrcAlpha,
                    zWrite = false
                    }
            };
    }

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        this.editor = editor;
        this.properties = properties;
        this.target = editor.target as Material;//MaterialEditor.target is Object type need cast
        DoRenderingMode();
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
        editor.TexturePropertySingleLine(MakeLable(mainTex,"Albedo(RGB)"), mainTex,FindProperty("_Color"));//ShowProperty

        if (ShouldShowAlphaCutoff)
        {
            DoAlphaCutoff();
        }
       
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

    static GUIContent MakeLable(string text, string tooltip = null)
    {
        staticLable.text = text;
        staticLable.tooltip = tooltip;
        return staticLable;
    }

    private void DoAlphaCutoff()
    {
        MaterialProperty slider = FindProperty("_Cutoff");

        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, MakeLable(slider));

        EditorGUI.indentLevel -= 2;

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

            foreach (Material m in editor.targets)
            {
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
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


    void DoRenderingMode()
    {
        RenderingMode mode = RenderingMode.Opaque;

        ShouldShowAlphaCutoff = false;

        if (IsKeyWordEnable("_RENDERING_CUTOUT"))
        {
            mode = RenderingMode.Cutout;
            ShouldShowAlphaCutoff = true;
        }
        else if (IsKeyWordEnable("_RENDERING_FADE"))
        {
            mode = RenderingMode.Fade;
        }
        else if (IsKeyWordEnable("_RENDERING_TRANSPARENT"))
        {
            mode = RenderingMode.Transparent;
        }

        EditorGUI.BeginChangeCheck();

   
        mode = (RenderingMode)EditorGUILayout.EnumPopup(
            new GUIContent().text = "Rendering Mode", mode
            );

        if (EditorGUI.EndChangeCheck())
        {
            RecordAction("Rendering Mode");
            SetKeyword("_RENDERING_CUTOUT", mode == RenderingMode.Cutout);
            SetKeyword("_RENDERING_FADE", mode == RenderingMode.Fade);
            SetKeyword("_RENDERING_TRANSPARENT", mode == RenderingMode.Transparent);

            //RenderQueue queue = mode == RenderingMode.Opaque ? RenderQueue.Geometry : RenderQueue.AlphaTest;//Lower queues are rendered first.

            //string renderType = mode == RenderingMode.Opaque ? "" : "TransparentCutout";

            RenderingSettings settings = RenderingSettings.modes[(int)mode];

            foreach (Material m in editor.targets)
            {
                m.renderQueue = (int)settings.queue;
                m.SetOverrideTag("RenderType", settings.renderType);
                m.SetInt("_SrcBlend", (int)settings.srcBlend);
                m.SetInt("_DstBlend", (int)settings.dstBlend);
                m.SetInt("_Zwrite", settings.zWrite ? 1 : 0);
            }
        }

        if (mode == RenderingMode.Fade || mode == RenderingMode.Transparent)
        {
            DoSemitransparentShadows();
        }
            
    }

    void DoSemitransparentShadows()
    {
        EditorGUI.BeginChangeCheck();
        bool semitransparentShadows =
            EditorGUILayout.Toggle(MakeLable("Semitransp. Shadows", "Semitransparent Shadows"),
            IsKeyWordEnable("_SEMITRANSPARENT_SHADOWS"));

        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_SEMITRANSPARENT_SHADOWS", semitransparentShadows);
        }

        if (!semitransparentShadows)
        {
            ShouldShowAlphaCutoff = true;
        }

    }
}
