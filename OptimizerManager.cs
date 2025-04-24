using System;
using System.Collections.Generic;
using UnityEngine;

namespace Steelbox.Optimizer
{
    public class OptimizerManager : MonoBehaviour
    {
        public static OptimizerManager Instance;
        
        public List<Camera> TargetCameras = new List<Camera>();

        public static Action OnTargetCamerasChange;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            AddCamera(Camera.main);
        }

        public void AddCamera(Camera cam)
        {
            TargetCameras.Add(cam);
            OnTargetCamerasChange?.Invoke();
        }

        public void RemoveCamera(Camera cam)
        {
            TargetCameras.Remove(cam);
            OnTargetCamerasChange?.Invoke();
        }
    }
}