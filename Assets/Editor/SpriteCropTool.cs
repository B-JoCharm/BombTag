using System.IO;
using UnityEditor;
using UnityEngine;

public class SpriteCropTool : EditorWindow
{
    private Sprite sourceSprite;

    private int cropFromTop;
    private int cropFromLeft;
    private int cropWidth;
    private int cropHeight;

    private string outputName = "profile";
    private string outputFolder = "Assets/Sprites/Profiles";

    private Texture2D previewTexture;

    [MenuItem("Tools/Sprite Crop Tool")]
    public static void Open()
    {
        GetWindow<SpriteCropTool>("Sprite Crop Tool").Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Crop Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        EditorGUI.BeginChangeCheck();
        sourceSprite = (Sprite)EditorGUILayout.ObjectField("Source Sprite", sourceSprite, typeof(Sprite), false);
        if (EditorGUI.EndChangeCheck() && sourceSprite != null)
        {
            cropFromLeft = 0;
            cropFromTop = 0;
            cropWidth = (int)sourceSprite.rect.width;
            cropHeight = (int)sourceSprite.rect.height;
            outputName = sourceSprite.name + "_profile";
            UpdatePreview();
        }

        if (sourceSprite == null)
        {
            EditorGUILayout.HelpBox("Source Sprite를 선택하세요.", MessageType.Info);
            return;
        }

        // ── 영역 설정 ──────────────────────────────
        EditorGUILayout.Space(8);
        GUILayout.Label("Crop Region  (Y: 스프라이트 위쪽 기준)", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        cropFromLeft = EditorGUILayout.IntField("Left (px)", cropFromLeft);
        cropFromTop  = EditorGUILayout.IntField("Top (px)",  cropFromTop);
        cropWidth    = EditorGUILayout.IntField("Width (px)", cropWidth);
        cropHeight   = EditorGUILayout.IntField("Height (px)", cropHeight);
        if (EditorGUI.EndChangeCheck())
            UpdatePreview();

        // ── 빠른 프리셋 ────────────────────────────
        EditorGUILayout.Space(4);
        GUILayout.Label("Quick Preset", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Full"))    ApplyPreset(0f, 0f, 1f, 1f);
        if (GUILayout.Button("Top 1/2")) ApplyPreset(0f, 0f, 1f, 0.5f);
        if (GUILayout.Button("Top 1/3")) ApplyPreset(0f, 0f, 1f, 0.33f);
        if (GUILayout.Button("Top 1/4")) ApplyPreset(0f, 0f, 1f, 0.25f);
        EditorGUILayout.EndHorizontal();

        // ── 미리보기 ───────────────────────────────
        EditorGUILayout.Space(8);
        GUILayout.Label("Preview", EditorStyles.boldLabel);
        if (previewTexture != null)
        {
            float maxSize = 160f;
            float aspect  = (float)previewTexture.width / previewTexture.height;
            float pw = aspect >= 1f ? maxSize : maxSize * aspect;
            float ph = aspect >= 1f ? maxSize / aspect : maxSize;

            Rect r = GUILayoutUtility.GetRect(position.width, ph + 8f);
            r.x = (position.width - pw) * 0.5f;
            r.width  = pw;
            r.height = ph;
            GUI.DrawTexture(r, previewTexture, ScaleMode.ScaleToFit);
        }

        // ── 출력 설정 ──────────────────────────────
        EditorGUILayout.Space(8);
        GUILayout.Label("Output", EditorStyles.boldLabel);
        outputName   = EditorGUILayout.TextField("File Name", outputName);
        outputFolder = EditorGUILayout.TextField("Folder",    outputFolder);

        EditorGUILayout.Space(8);
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Export Profile Sprite", GUILayout.Height(32)))
            ExportSprite();
        GUI.backgroundColor = Color.white;
    }

    private void ApplyPreset(float leftRatio, float topRatio, float widthRatio, float heightRatio)
    {
        if (sourceSprite == null) return;
        int sw = (int)sourceSprite.rect.width;
        int sh = (int)sourceSprite.rect.height;
        cropFromLeft = Mathf.RoundToInt(sw * leftRatio);
        cropFromTop  = Mathf.RoundToInt(sh * topRatio);
        cropWidth    = Mathf.RoundToInt(sw * widthRatio);
        cropHeight   = Mathf.RoundToInt(sh * heightRatio);
        UpdatePreview();
        Repaint();
    }

    private void UpdatePreview()
    {
        if (sourceSprite == null) return;

        Texture2D readable = GetReadableTexture(sourceSprite);
        if (readable == null) return;

        int x, y, w, h;
        GetTexCoords(readable, out x, out y, out w, out h);

        Color[] pixels = readable.GetPixels(x, y, w, h);
        DestroyImmediate(readable);

        if (previewTexture != null) DestroyImmediate(previewTexture);
        previewTexture = new Texture2D(w, h) { filterMode = FilterMode.Point };
        previewTexture.SetPixels(pixels);
        previewTexture.Apply();

        Repaint();
    }

    private void ExportSprite()
    {
        if (sourceSprite == null) return;

        Texture2D readable = GetReadableTexture(sourceSprite);
        if (readable == null)
        {
            EditorUtility.DisplayDialog("Error", "텍스처를 읽을 수 없습니다.", "OK");
            return;
        }

        int x, y, w, h;
        GetTexCoords(readable, out x, out y, out w, out h);

        Color[] pixels = readable.GetPixels(x, y, w, h);
        DestroyImmediate(readable);

        Texture2D output = new Texture2D(w, h);
        output.SetPixels(pixels);
        output.Apply();
        byte[] bytes = output.EncodeToPNG();
        DestroyImmediate(output);

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string path = $"{outputFolder}/{outputName}.png";
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType      = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode       = FilterMode.Point;
            importer.SaveAndReimport();
        }

        Debug.Log($"[SpriteCropTool] Exported: {path}");
        EditorUtility.DisplayDialog("완료", $"저장됨:\n{path}", "OK");
    }

    // sprite rect 기준으로 텍스처 좌표 계산 (Y from top → Unity bottom-left 변환)
    private void GetTexCoords(Texture2D tex, out int x, out int y, out int w, out int h)
    {
        int spriteX = (int)sourceSprite.rect.x;
        int spriteY = (int)sourceSprite.rect.y;
        int spriteW = (int)sourceSprite.rect.width;
        int spriteH = (int)sourceSprite.rect.height;

        w = Mathf.Clamp(cropWidth,  1, spriteW - cropFromLeft);
        h = Mathf.Clamp(cropHeight, 1, spriteH - cropFromTop);
        x = spriteX + Mathf.Clamp(cropFromLeft, 0, spriteW - 1);
        // Y: Unity는 하단 기준 → "top 기준 cropFromTop"을 변환
        y = spriteY + (spriteH - cropFromTop - h);
        y = Mathf.Clamp(y, 0, tex.height - 1);
    }

    // Read/Write 여부 관계없이 텍스처 픽셀을 읽는 방법
    private Texture2D GetReadableTexture(Sprite sprite)
    {
        RenderTexture rt  = RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, 0);
        Graphics.Blit(sprite.texture, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readable = new Texture2D(sprite.texture.width, sprite.texture.height);
        readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readable.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return readable;
    }

    private void OnDestroy()
    {
        if (previewTexture != null) DestroyImmediate(previewTexture);
    }
}
