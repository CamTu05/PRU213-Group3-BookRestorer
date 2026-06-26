using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class CreateAnimationFromSprites : EditorWindow
{
    [MenuItem("Tools/Fast Anim Creator")]
    public static void CreateAnim()
    {
        // Lấy tất cả các Object đang được chọn ở cửa sổ Project
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Lỗi", "Hãy chọn các Sprite (ô nhỏ) trong Spritesheet trước!", "OK");
            return;
        }

        // Lọc ra các Object là Sprite và sắp xếp theo tên
        List<Sprite> sprites = selectedObjects.OfType<Sprite>().OrderBy(s => s.name).ToList();

        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Sprite nào trong vùng chọn.", "OK");
            return;
        }

        // Hỏi nơi lưu và tên file .anim
        string assetPath = AssetDatabase.GetAssetPath(sprites[0]);
        string directory = Path.GetDirectoryName(assetPath);
        string savePath = EditorUtility.SaveFilePanelInProject("Lưu Animation", "NewAnimation", "anim", "Chọn nơi lưu file animation", directory);

        if (string.IsNullOrEmpty(savePath)) return;

        // Tạo một Animation Clip mới
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 12; // Thay đổi số frame/giây tùy ý bạn (thường là 12 hoặc 24)

        // Cấu hình mảng các Keyframe
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            // Tính toán thời gian cho mỗi frame
            keyframes[i].time = i * (1.0f / clip.frameRate);
            keyframes[i].value = sprites[i];
        }

        // Bind keyframes vào thuộc tính Sprite của SpriteRenderer
        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "";
        binding.propertyName = "m_Sprite";

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // Lưu file asset
        AssetDatabase.CreateAsset(clip, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Thành công", $"Đã tạo xong Animation tại: {savePath}", "Ngon!");
    }
}
