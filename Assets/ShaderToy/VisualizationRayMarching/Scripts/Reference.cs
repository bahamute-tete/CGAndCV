using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Reference : MonoBehaviour
{
   public Texture3D texture;
    public float alpha = 1.0f;
    public float quality = 1.0f;
    public FilterMode filterMode;
    public bool useColorRamp;
    public bool useCustomColorRamp;

    public Gradient customColorRampGradient;
}


[CanEditMultipleObjects]
[CustomEditor(typeof(Reference))]
public class Handle : Editor
{
    private void OnSceneViewGUI(SceneView sv)
    {
        Object[] objects = targets;
        foreach (var obj in objects)
        {
            Reference reference = obj as Reference;
            if (reference != null && reference.texture != null)
            {
                Handles.matrix = reference.transform.localToWorldMatrix;
                Handles.DrawTexture3DVolume(reference.texture, reference.alpha, reference.quality, reference.filterMode,
                    reference.useColorRamp, reference.useCustomColorRamp ? reference.customColorRampGradient : null);
            }
        }
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneViewGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneViewGUI;
    }

}
