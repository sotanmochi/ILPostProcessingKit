using UnityEngine;

namespace ILPostProcessingKit.Samples.Sample01
{
    public class LoopCounter01A : MonoBehaviour
    {
        [SerializeField] int _loopCountMax = 3000;

        private int _loopCount;
        public int LoopCount => _loopCount;

        public bool IsLoopEnd { get; private set; }

        void Update()
        {
            if (_loopCount > _loopCountMax)
            {
                IsLoopEnd = true;
                return;
            }

            _loopCount++;
        }

        void OnDestroy()
        {
            Debug.Log($"{nameof(LoopCounter01A.OnDestroy)}");
        }
    }
}
