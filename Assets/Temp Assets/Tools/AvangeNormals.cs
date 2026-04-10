using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AvangeNormals : MonoBehaviour
{
    private object tempWarning = TempWarning.Log("AvangeNormals should be removed and all meshes with avange normals in vert colors should be exported and reimported.");

    void Start()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        var dictVertNormal = new Dictionary<Vector3, Vector3>();
        for (int i = 1; i < mesh.vertexCount; i++)
        {
            var vert = mesh.vertices[i];
            if (dictVertNormal.ContainsKey(vert))
                dictVertNormal[vert] += mesh.normals[i];
            else
                dictVertNormal[vert] = mesh.normals[i];
        }

        Color[] colors = new Color[mesh.vertexCount];
        for (int i = 1; i < mesh.vertexCount; i++)
        {
            Vector3 avangeNormal = dictVertNormal[mesh.vertices[i]].normalized;
            colors[i] = new Color(avangeNormal.x, avangeNormal.y, avangeNormal.z);
        }
        mesh.SetColors(colors);
    }

    void OnEnable() { }
}
