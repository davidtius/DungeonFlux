using UnityEngine;
using UnityEditor;

public class ForceCreateMat
{
    // Ini akan menambah menu baru di toolbar atas Unity
    [MenuItem("Tools/Create Bouncy Material")]
    public static void CreateAsset()
    {
        PhysicsMaterial2D mat = new PhysicsMaterial2D();
        mat.bounciness = 0.8f;
        mat.friction = 0.4f;
        
        // Simpan file ke folder Assets
        AssetDatabase.CreateAsset(mat, "Assets/BouncyMat.physicsMaterial2D");
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = mat;
        
        Debug.Log("Berhasil membuat BouncyMat di folder Assets!");
    }
}