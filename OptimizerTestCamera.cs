using UnityEngine;

namespace Steelbox.Optimizer
{
    public class OptimizerTestCamera : MonoBehaviour
    {
        [ContextMenu("Add Camera")]
        public void AddCamera()
        {
            OptimizerManager.Instance.AddCamera(GetComponent<Camera>());
        }
        
        [ContextMenu("Remove Camera")]
        public void RemoveCamera()
        {
            OptimizerManager.Instance.RemoveCamera(GetComponent<Camera>());
        }
    }
}