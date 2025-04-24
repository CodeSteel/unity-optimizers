using System.Collections.Generic;
using UnityEngine;

namespace Game.Optimizer
{
    public class LightControllerOptimizer : BaseOptimizer
    {
        [SerializeField, HideInInspector] private LightController _lightController;

        private float _initialRange;
        
        private void OnValidate()
        {
            if (_lightController) return;
            _lightController = GetComponent<LightController>();
        }

        protected override void Start()
        {
            base.Start();
            _initialRange = _lightController.Light.range;
        }

        protected override List<float> OptimizerRadiusPerSphere => new List<float>()
        {
            _initialRange / 3f,
            _initialRange / 1.5f,
            _initialRange,
        };

        protected override void OnVisible(int index, float distance)
        {
            float count = OptimizerRadiusPerSphere.Count;
            float invertedStep = (count - index);
            float visiblePercentage = (invertedStep / count) * distance;
            
            _lightController.Light.intensity = Mathf.Lerp(_lightController.Light.intensity, visiblePercentage * _lightController.Intensity, Time.deltaTime);
            _lightController.Light.range  = Mathf.Lerp(_lightController.Light.range, visiblePercentage * _initialRange, Time.deltaTime);
            _lightController.Light.enabled = true;
        }

        protected override void OnInvisible()
        {
            _lightController.Light.enabled = false;
        }
    }
}