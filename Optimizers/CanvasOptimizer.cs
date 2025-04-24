using System.Collections.Generic;
using UnityEngine;

namespace Game.Optimizer
{
    public class CanvasOptimizer : BaseOptimizer
    {
        [SerializeField] private float _size;
        [SerializeField, HideInInspector] private Canvas _canvas;

        private void OnValidate()
        {
            if (_canvas) return;
            _canvas = GetComponent<Canvas>();
        }

        protected override bool UseDistance => false;

        protected override List<float> OptimizerRadiusPerSphere => new List<float>()
        {
            _size,
        };

        protected override void OnVisible(int index, float distance)
        {
            _canvas.enabled = true;
        }

        protected override void OnInvisible()
        {
            _canvas.enabled = false;
        }
    }
}