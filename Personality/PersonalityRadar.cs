using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Personality
{
    public class PersonalityRadar : MaskableGraphic
    {
        public Personality_Test personality = new();
        public float radius = 100f;
        public Color outlineColor = Color.gray;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var center = rectTransform.rect.center;

            var traitCount = personality.traits.Count;
            if (traitCount == 0) return;
            
            var axisCount = traitCount * 2;
            var angleStep = 360f / axisCount;

            var valuePoints = new Vector2[axisCount];
            
            var outlineStart = vh.currentVertCount;
            for (var i = 0; i < axisCount; i++)
            {
                var dir = AngleToVector2(90f - i * angleStep);
                vh.AddVert(center + dir * radius, outlineColor, Vector2.zero);
            }

            for (var i = 0; i < axisCount; i++)
                vh.AddTriangle(outlineStart + i, outlineStart + ((i + 1) % axisCount),
                    outlineStart + ((i + 2) % axisCount));

            var valuePointsList = new List<Vector2>();

            for (var i = 0; i < traitCount; i++)
            {
                var val = Mathf.Clamp(personality.traits[i].value, -1f, 1f);
                var mag = Mathf.Abs(val) * radius;

                var angle = 90f - i * angleStep;
                var dir = AngleToVector2(angle);
                if (val < 0)
                    dir = -dir;

                valuePointsList.Add(center + dir * mag);
            }
            
            for (var i = 0; i < axisCount; i++)
            {
                if (valuePoints[i] == Vector2.zero)
                    valuePoints[i] = center;
            }

            var valueStart = vh.currentVertCount;
            foreach (var p in valuePointsList)
                vh.AddVert(p, color, Vector2.zero);

            for (var i = 0; i < valuePointsList.Count - 2; i++)
                vh.AddTriangle(valueStart, valueStart + i + 1, valueStart + i + 2);
        }

        Vector2 AngleToVector2(float angleDegrees)
        {
            var rad = angleDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }

    [System.Serializable]
    public class Trait
    {
        public string name;
        [Range(-1f, 1f)] public float value;
    }
}