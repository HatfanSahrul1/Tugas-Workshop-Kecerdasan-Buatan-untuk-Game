using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Helper script untuk fix masalah nested collider pada wall
public class WallSetupHelper : MonoBehaviour
{
    [SerializeField] string wallLayerName = "Wall";
    [SerializeField] bool autoFixOnStart = false;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixAllWalls();
        }
    }
    
    [ContextMenu("Fix All Walls - Copy Colliders to Parent")]
    public void FixAllWalls()
    {
        int layerIndex = LayerMask.NameToLayer(wallLayerName);
        if (layerIndex == -1)
        {
            Debug.LogError($"❌ Layer '{wallLayerName}' tidak ditemukan!");
            return;
        }
        
        Debug.Log("🔧 Starting wall fix process...");
        
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int fixedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Hanya proses object di layer Wall yang TIDAK punya collider sendiri
            if (obj.layer == layerIndex && obj.GetComponent<Collider>() == null)
            {
                // Cari collider di children
                Collider[] childColliders = obj.GetComponentsInChildren<Collider>();
                
                if (childColliders.Length > 0)
                {
                    Debug.Log($"📦 Processing: {obj.name}");
                    
                    foreach (Collider childCol in childColliders)
                    {
                        // Copy collider ke parent
                        CopyColliderToParent(childCol, obj);
                        fixedCount++;
                    }
                }
            }
        }
        
        Debug.Log($"✅ Fixed {fixedCount} walls!");
        Debug.Log("⚠️  PENTING: Sekarang rebuild graph di SimplePathfinding!");
    }
    
    void CopyColliderToParent(Collider childCollider, GameObject parent)
    {
        // Get world space bounds dari child collider
        Bounds worldBounds = childCollider.bounds;
        
        // Konversi ke local space parent
        Vector3 localCenter = parent.transform.InverseTransformPoint(worldBounds.center);
        Vector3 localSize = parent.transform.InverseTransformVector(worldBounds.size);
        
        // Pastikan size positif
        localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
        
        // Buat BoxCollider di parent
        BoxCollider newCollider = parent.GetComponent<BoxCollider>();
        if (newCollider == null)
        {
            newCollider = parent.AddComponent<BoxCollider>();
        }
        
        newCollider.center = localCenter;
        newCollider.size = localSize;
        
        Debug.Log($"   ✓ Copied collider from {childCollider.name} to {parent.name}");
        Debug.Log($"     Center: {localCenter}, Size: {localSize}");
    }
    
    [ContextMenu("Alternative: Set Layer Recursively")]
    public void SetLayerRecursively()
    {
        int layerIndex = LayerMask.NameToLayer(wallLayerName);
        if (layerIndex == -1)
        {
            Debug.LogError($"❌ Layer '{wallLayerName}' tidak ditemukan!");
            return;
        }
        
        Debug.Log("🔧 Setting layer recursively on all wall hierarchies...");
        
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == layerIndex)
            {
                // Set layer ke semua children juga
                SetLayerRecursivelyHelper(obj.transform, layerIndex);
                count++;
            }
        }
        
        Debug.Log($"✅ Processed {count} wall hierarchies!");
    }
    
    void SetLayerRecursivelyHelper(Transform trans, int layer)
    {
        trans.gameObject.layer = layer;
        
        foreach (Transform child in trans)
        {
            SetLayerRecursivelyHelper(child, layer);
        }
    }
    
    [ContextMenu("Check Wall Setup")]
    public void CheckWallSetup()
    {
        int layerIndex = LayerMask.NameToLayer(wallLayerName);
        if (layerIndex == -1)
        {
            Debug.LogError($"❌ Layer '{wallLayerName}' tidak ditemukan!");
            return;
        }
        
        Debug.Log("╔════════════════════════════════════════════╗");
        Debug.Log("║          WALL SETUP DIAGNOSTIC             ║");
        Debug.Log("╚════════════════════════════════════════════╝");
        
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int wallObjects = 0;
        int withCollider = 0;
        int withChildCollider = 0;
        int noCollider = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == layerIndex)
            {
                wallObjects++;
                
                Collider col = obj.GetComponent<Collider>();
                Collider[] childCols = obj.GetComponentsInChildren<Collider>();
                
                if (col != null)
                {
                    withCollider++;
                    Debug.Log($"✅ {obj.name} - Has collider on parent");
                }
                else if (childCols.Length > 0)
                {
                    withChildCollider++;
                    Debug.LogWarning($"⚠️  {obj.name} - Collider only in children ({childCols.Length})");
                    
                    foreach (Collider childCol in childCols)
                    {
                        Debug.Log($"    └─ {GetPath(childCol.transform)}");
                    }
                }
                else
                {
                    noCollider++;
                    Debug.LogError($"❌ {obj.name} - NO COLLIDER AT ALL!");
                }
            }
        }
        
        Debug.Log("───────────────────────────────────────────");
        Debug.Log($"Total wall objects: {wallObjects}");
        Debug.Log($"✅ With collider on parent: {withCollider}");
        Debug.Log($"⚠️  With collider only in children: {withChildCollider}");
        Debug.Log($"❌ Without any collider: {noCollider}");
        Debug.Log("───────────────────────────────────────────");
        
        if (withChildCollider > 0)
        {
            Debug.LogWarning("⚠️  MASALAH DITEMUKAN: Ada wall dengan collider nested di children!");
            Debug.LogWarning("💡 SOLUSI: Klik kanan script ini → 'Fix All Walls - Copy Colliders to Parent'");
        }
        else if (withCollider == wallObjects)
        {
            Debug.Log("✅ SETUP SEMPURNA! Semua wall punya collider di parent level.");
        }
    }
    
    string GetPath(Transform trans)
    {
        string path = trans.name;
        Transform current = trans.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WallSetupHelper))]
public class WallSetupHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        WallSetupHelper helper = (WallSetupHelper)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Tool untuk fix masalah nested collider pada wall", MessageType.Info);
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("1. Check Wall Setup", GUILayout.Height(30)))
        {
            helper.CheckWallSetup();
        }
        
        if (GUILayout.Button("2. Fix All Walls (Copy Colliders)", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Fix All Walls", 
                "Ini akan copy semua collider dari children ke parent. Lanjutkan?", 
                "Yes", "Cancel"))
            {
                helper.FixAllWalls();
            }
        }
        
        if (GUILayout.Button("3. Set Layer Recursively", GUILayout.Height(30)))
        {
            helper.SetLayerRecursively();
        }
    }
}
#endif