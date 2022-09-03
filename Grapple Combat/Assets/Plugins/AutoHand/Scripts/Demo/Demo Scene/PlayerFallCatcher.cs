using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Autohand.Demo{
    public class PlayerFallCatcher : MonoBehaviour{
        Vector3 startPos;

        void Start(){
            if(AutoHandPlayer._Instance != null) {
                startPos = AutoHandPlayer._Instance.transform.position;
                if(!SceneManager.GetActiveScene().name.ToLower().Contains("demo"))
                    enabled = false;
            }
        }
        
        void Update() {
            if(AutoHandPlayer._Instance != null) {
                if(AutoHandPlayer._Instance.transform.position.y < -10f) {
                    AutoHandPlayer._Instance.SetPosition(startPos);
                }
            }
        }
    }
}
