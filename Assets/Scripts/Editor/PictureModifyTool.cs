using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureSizeEditorWindow  {
    [MenuItem("Tools/ShrinkTexSize")]
    private static void CreateTexture() {
        var objs = Selection.objects;
        foreach (var obj in objs) {
            var sourceTexture = obj as Texture2D;
            if (sourceTexture == null) continue;

            var resultWidth = sourceTexture.width / 2;
            var resultHeight = sourceTexture.height / 2;
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace("Assets/", "");
            string filePathName = sourcePath.Replace("/" + Path.GetFileName(sourcePath), "");
            var sourceFolerPath = filePathName + "/Changes/";
            var resultTexture = new Texture2D(resultWidth, resultHeight, TextureFormat.RGBAHalf, false);
            resultTexture.name = sourceTexture.name;
            for (int wIndex = 0; wIndex < resultWidth; wIndex++) {
                for (int hIndex = 0; hIndex < resultHeight; hIndex++) {
                    Color sum = Color.black;
                    var notNormalTex = false;
                    if (notNormalTex) {
                        for (int i = 0; i < 2; i++) {
                            for (int j = 0; j < 2; j++) {
                                var x = wIndex * 2 + i;
                                var y = hIndex * 2 + j;
                                sum+= sourceTexture.GetPixel(x, y);
                            }
                        }
                        sum = sum *0.25f;
                    }
                    else {
                        sum = sourceTexture.GetPixel(wIndex, hIndex);
                    }

                    resultTexture.SetPixel(wIndex, hIndex,sum/4);
                }
            }

            string ext = ".png";
            string floderPath = Application.dataPath + "/" + sourceFolerPath;
            if (Directory.Exists(floderPath) == false) {
                Directory.CreateDirectory(floderPath);
            }

            string foldeName = floderPath + resultTexture.name + ext;
            byte[] textuteByte = resultTexture.EncodeToPNG();
            if (File.Exists(foldeName)) {
                File.Delete(foldeName);
            }
            File.WriteAllBytes(foldeName,textuteByte);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}