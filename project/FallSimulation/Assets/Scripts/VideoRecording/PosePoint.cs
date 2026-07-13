using UnityEngine;

namespace VideoRecording {
    public class PosePoint : MonoBehaviour {
        [field:SerializeField] public int PoseID { get; private set; }
        
        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.01f);
        }

        public Vector3 GetPosition() {
            return transform.position;
        }
    }
}
