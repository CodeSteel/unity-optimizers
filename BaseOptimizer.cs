using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steelbox.Optimizer
{
    public abstract class BaseOptimizer : MonoBehaviour
    {
        [SerializeField, Tooltip("Inside this distance → visibility == 1")] 
        private float _optimizerMinDistance = 10f;

        [SerializeField, Tooltip("Outside this distance → visibility == 0")] 
        private float _optimizerMaxDistance = 15f;

        private CullingGroup[] _cullingGroups;
        private BoundingSphere[] _boundingSpheres;
        private int[] _visibleCounts;
        private Camera[] _camerasViewing;
        private bool[] _camerasViewingVisible;
        private int _currentNearestSphereIndex = -1;
        private bool _hasBuiltCullGroups;
        private Camera _lastVisibleCamera;

        protected virtual float OptimizerMinDistance => _optimizerMinDistance;
        protected virtual float OptimizerMaxDistance => _optimizerMaxDistance;
        protected virtual bool UseDistance => true;
        
        protected virtual List<float> OptimizerRadiusPerSphere => _optimizerRadiusPerSphere;
        private readonly List<float> _optimizerRadiusPerSphere = new List<float>();
        
        private void Awake()
        {
            OptimizerManager.OnTargetCamerasChange += BuildCullingGroups;
        }

        private void OnDestroy()
        {
            OptimizerManager.OnTargetCamerasChange -= BuildCullingGroups;
            
            if (_cullingGroups != null)
            {
                foreach (var cg in _cullingGroups)
                {
                    cg.Dispose();
                }
                _cullingGroups = null;
            }
        }

        protected virtual void Start()
        {
            if (!_hasBuiltCullGroups)
            {
                StartCoroutine(DelayedStart());
            }
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();
            BuildCullingGroups();
        }

        private void OnDrawGizmosSelected()
        {
            if (OptimizerRadiusPerSphere.Count == 0) return;
            
            Gizmos.color = Color.grey;
            foreach (int range in OptimizerRadiusPerSphere)
            {
                Gizmos.DrawWireSphere(transform.position, range);
            }
        }

        private void BuildCullingGroups()
        {
            if (!OptimizerManager.Instance) return;
            
            // dispose old groups
            if (_cullingGroups != null)
            {
                foreach (var group in _cullingGroups)
                {
                    group?.Dispose();
                }
                _cullingGroups = null;
            }
            
            int sphereCount = OptimizerRadiusPerSphere.Count;
            int camCount = OptimizerManager.Instance.TargetCameras.Count;

            _cullingGroups = new CullingGroup[camCount];
            _camerasViewing = new Camera[camCount];
            _camerasViewingVisible = new bool[camCount];
            
            _boundingSpheres = new BoundingSphere[sphereCount];
            _visibleCounts = new int[sphereCount];
            _currentNearestSphereIndex = -1;

            for (int i = 0; i < sphereCount; i++)
            {
                _boundingSpheres[i] = new BoundingSphere(transform.position, OptimizerRadiusPerSphere[i]);
            }

            for (int i = 0; i < camCount; i++)
            {
                var cam = OptimizerManager.Instance.TargetCameras[i];
                var camIndex = i;
                var group = new CullingGroup();
                group.targetCamera = cam;
                group.SetBoundingSphereCount(sphereCount);
                group.SetBoundingSpheres(_boundingSpheres);

                _camerasViewing[i] = cam;
                _camerasViewingVisible[i] = false;

                group.onStateChanged += evt =>
                {
                    _camerasViewingVisible[camIndex] = evt.isVisible;
                    OnGroupStateChanged(evt.index, evt.isVisible);
                };

                _cullingGroups[i] = group;
            }

            _hasBuiltCullGroups = true;
        }

        private void OnGroupStateChanged(int sphereIndex, bool nowVisible)
        {
            if (nowVisible)
                _visibleCounts[sphereIndex]++;
            else if (_visibleCounts[sphereIndex] > 0)
                _visibleCounts[sphereIndex]--;

            int newNearest = -1;
            for (int i = 0; i < _visibleCounts.Length; i++)
            {
                if (_visibleCounts[i] > 0)
                {
                    newNearest = i;
                    break;
                }
            }

            _currentNearestSphereIndex = newNearest;
        }

        private void LateUpdate()
        {
            if (!_hasBuiltCullGroups) return;

            Vector3 position = transform.position;

            for (int i = 0; i < _boundingSpheres.Length; i++)
            {
                _boundingSpheres[i].position = position;
#if UNITY_EDITOR
                _boundingSpheres[i].radius = OptimizerRadiusPerSphere[i];
#endif
            }

            if (!_lastVisibleCamera || Time.frameCount % 30 == 0)
            {
                _lastVisibleCamera = GetClosestVisibleCamera();
            }

            if (_currentNearestSphereIndex >= 0 && _lastVisibleCamera)
            {
                float norm = UseDistance ? ComputeNormalizedDistance(_lastVisibleCamera, position) : 0;
                OnVisible(_currentNearestSphereIndex, norm);
            }
            else
            {
                OnInvisible();
            }
        }

        private float ComputeNormalizedDistance(Camera cam, Vector3 thisPosition)
        {
            float d = Vector3.Distance(cam.transform.position, thisPosition);
            if (d <= OptimizerMinDistance) return 1f;
            if (d >= OptimizerMaxDistance) return 0f;
            return 1f - ((d - OptimizerMinDistance) / (OptimizerMaxDistance - OptimizerMinDistance));
        }

        private Camera GetClosestVisibleCamera()
        {
            if (_camerasViewingVisible.Length == 1)
            {
                return _camerasViewingVisible[0] ? _camerasViewing[0] : null;
            }

            float closestDistance = float.MaxValue;
            Camera closetCamera = null;
            
            for (int i = 0; i < _camerasViewingVisible.Length; i++)
            {
                if (!_camerasViewingVisible[i]) continue;
                
                var cam = _camerasViewing[i];
                if (!cam) continue;
                
                float distance = (cam.transform.position - transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closetCamera = cam;
                }
            }

            return closetCamera;
        }

        #region Events

        protected abstract void OnVisible(int index, float distance);

        protected abstract void OnInvisible();

        #endregion
    }
}
