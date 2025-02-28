using System.Collections;
using UnityEngine;

namespace Tools
{
    public abstract class Visualise_GrowAndShrink
    {
        public static IEnumerator GrowAndShrink(Transform transform, float iterations = 3, 
            float growTime = 0.5f, float shrinkTime = 0.5f, float growAmount = 2f, float shrinkAmount = 2f)
        {
            for (var i = 0; i < iterations; i++)
            {
                var startScale = transform.localScale;
                
                yield return _scaleOverTime(transform, startScale * growAmount, growTime);
            
                yield return _scaleOverTime(transform, startScale / shrinkAmount, shrinkTime);
                
                yield return _scaleOverTime(transform, startScale, shrinkTime);
            }
        }

        static IEnumerator _scaleOverTime(Transform transform, Vector3 targetScale, float duration)
        {
            var startScale = transform.localScale;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }
    }
}