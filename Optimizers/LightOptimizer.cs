using System.Collections.Generic;
using UnityEngine;

namespace Steelbox.Optimizer
{
    public class LightOptimizer : BaseOptimizer
    {
        [SerializeField, HideInInspector] private Light _light;

        private float _initialIntensity;
        private float _initialRange;
        
        private void OnValidate()
        {
            if (_light) return;
            _light = GetComponent<Light>();
        }

        protected override void Start()
        {
            base.Start();
            _initialIntensity = _light.intensity;
            _initialRange = _light.range;
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
            
            _light.intensity  = Mathf.Lerp(_light.intensity, visiblePercentage * _initialIntensity, Time.deltaTime);
            _light.range  = Mathf.Lerp(_light.range, visiblePercentage * _initialRange, Time.deltaTime);
            _light.enabled = true;
        }

        protected override void OnInvisible()
        {
            _light.enabled = false;
        }
    }
}