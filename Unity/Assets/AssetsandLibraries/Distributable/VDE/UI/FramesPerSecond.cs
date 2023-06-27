/* 
 * based on https://docs.unity3d.com/ScriptReference/Time-realtimeSinceStartup.html
 */
using UnityEngine;

namespace Assets.VDE.UI
{
    internal class FramesPerSecond : MonoBehaviour
    {
        const float fpsMeasurePeriod = 0.5f;
        int m_FpsAccumulator = 0;
        float m_FpsNextPeriod = 0;
        internal int currentFps;

        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }

        private void Update()
        {
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                currentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
            }
        }
    }
}
