using UnityEngine;

namespace ILPostProcessingKit.Samples.Sample01
{
    public class LoopCounter01B : MonoBehaviour
    {
        private int _oddFrameCount;
        private int _evenFrameCount;

        public int OddFrameCount => _oddFrameCount;
        public int EvenFrameCount => _evenFrameCount;

        void Update()
        {
            if (Time.frameCount % 2 == 0)
            {
                _evenFrameCount++;
            }
            else
            {
                _oddFrameCount++;
            }
        }

        void OnDestroy()
        {
            Debug.Log($"{nameof(LoopCounter01B.OnDestroy)}");
        }
    }
}
