using System.Collections.Generic;
using UnityEngine;

namespace Steelbox.Optimizer
{
    public class AnimatorOptimizer : BaseOptimizer
    {
        [SerializeField] private float _optimizerRadius;
        [SerializeField, HideInInspector] private Animator _animator;

        private void OnValidate()
        {
            if (_animator) return;
            _animator = GetComponent<Animator>();
        }

        protected override bool UseDistance => false;

        protected override List<float> OptimizerRadiusPerSphere => new List<float>()
        {
            _optimizerRadius
        };

        protected override void OnVisible(int index, float distance)
        {
            _animator.enabled = true;
        }

        protected override void OnInvisible()
        {
            _animator.enabled = false;
        }
    }
}