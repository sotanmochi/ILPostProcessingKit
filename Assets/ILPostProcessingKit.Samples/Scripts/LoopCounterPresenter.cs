using UnityEngine;
using UnityEngine.UI;

namespace ILPostProcessingKit.Samples
{
    public class LoopCounterPresenter : MonoBehaviour
    {
        [SerializeField] Text _countViewA;
        [SerializeField] Text _countViewB;
        [SerializeField] Sample01.LoopCounter01A _counterA;
        [SerializeField] Sample01.LoopCounter01B _counterB;

        void LateUpdate()
        {
            _countViewA.text = $"FrameCount: {_counterA.LoopCount}";
            _countViewB.text = $"OddFrames: {_counterB.OddFrameCount}, EvenFrames: {_counterB.EvenFrameCount}";
        }
    }
}
