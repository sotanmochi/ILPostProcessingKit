using UnityEngine;

namespace ILPostProcessingKit.Samples.Sample02
{
    public class LoopCounter02A : MonoBehaviour
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
            Debug.Log($"{nameof(LoopCounter02A.OnDestroy)}");
        }
    }
}
