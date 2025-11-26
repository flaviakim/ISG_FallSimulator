using UnityEngine;

namespace VideoRecording {
    public class PosePoint : MonoBehaviour {
        [field:SerializeField] public int PoseID { get; private set; }
        
        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
        }

        public Vector3 GetPosition() {
            return transform.position;
        }
    }
}