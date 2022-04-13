using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;

public class BakeTextureWindow : EditorWindow {
    [MenuItem("Tools/Bake material to texture")]
    private static void OpenWindow() {
        var window = GetWindow<BakeTextureWindow>();
        
        window.Show();
    }

    private Material material;
    private string filePath = "Assets/MaterialImage.png";
    private Vector2Int resolution;

    private void OnGUI() {
        material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), false);
        resolution = EditorGUILayout.Vector2IntField("Image Resolution", resolution);
        filePath = EditorGUILayout.TextField("Image Path", filePath);

        if (!GUILayout.Button("Bake"))
            return;
        
        var renderTexture = RenderTexture.GetTemporary(resolution.x, resolution.y);
            
        Graphics.Blit(null, renderTexture, material);
        
        var texture = new Texture2D(resolution.x, resolution.y, TextureFormat.R16, false);
        
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(Vector2.zero, resolution), 0, 0);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);
        
        byte[] png = texture.EncodeToPNG();
        
        DestroyImmediate(texture);
        File.WriteAllBytes(filePath, png);
        AssetDatabase.Refresh();
    }
}
