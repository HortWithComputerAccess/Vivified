using System.Collections.Generic;
using UnityEngine;

namespace Vivified
{
    /// <summary>
    /// Applies active Vivify Blit materials as real post-processing on the
    /// editor camera (built-in render pipeline OnRenderImage chain).
    /// </summary>
    public class VivifyBlitRenderer : MonoBehaviour
    {
        public struct Entry
        {
            public Material Material;
            public int Pass;
        }

        public readonly List<Entry> Active = new List<Entry>();

        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (Active.Count == 0)
            {
                Graphics.Blit(src, dst);
                return;
            }

            RenderTexture current = src;
            RenderTexture previousTemp = null;
            for (int i = 0; i < Active.Count; i++)
            {
                bool last = i == Active.Count - 1;
                RenderTexture target = last
                    ? dst
                    : RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
                var entry = Active[i];
                if (entry.Material == null) Graphics.Blit(current, target);
                else if (entry.Pass >= 0) Graphics.Blit(current, target, entry.Material, entry.Pass);
                else Graphics.Blit(current, target, entry.Material);
                if (previousTemp != null) RenderTexture.ReleaseTemporary(previousTemp);
                previousTemp = last ? null : target;
                current = target;
            }
        }
    }

    /// <summary>Wireframe bounds highlight for the selected Vivify object.</summary>
    public class VivifySelectionHighlight : MonoBehaviour
    {
        public Bounds? Target;
        private static Material lineMaterial;

        private static void EnsureMaterial()
        {
            if (lineMaterial != null) return;
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            lineMaterial.SetInt("_ZWrite", 0);
        }

        private void OnRenderObject()
        {
            if (!Target.HasValue) return;
            EnsureMaterial();
            var b = Target.Value;
            var min = b.min;
            var max = b.max;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(new Color(0.35f, 0.85f, 0.75f, 0.95f));

            // bottom rectangle
            Line(min.x, min.y, min.z, max.x, min.y, min.z);
            Line(max.x, min.y, min.z, max.x, min.y, max.z);
            Line(max.x, min.y, max.z, min.x, min.y, max.z);
            Line(min.x, min.y, max.z, min.x, min.y, min.z);
            // top rectangle
            Line(min.x, max.y, min.z, max.x, max.y, min.z);
            Line(max.x, max.y, min.z, max.x, max.y, max.z);
            Line(max.x, max.y, max.z, min.x, max.y, max.z);
            Line(min.x, max.y, max.z, min.x, max.y, min.z);
            // verticals
            Line(min.x, min.y, min.z, min.x, max.y, min.z);
            Line(max.x, min.y, min.z, max.x, max.y, min.z);
            Line(max.x, min.y, max.z, max.x, max.y, max.z);
            Line(min.x, min.y, max.z, min.x, max.y, max.z);

            GL.End();
            GL.PopMatrix();
        }

        private static void Line(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            GL.Vertex3(x1, y1, z1);
            GL.Vertex3(x2, y2, z2);
        }
    }
}
