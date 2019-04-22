using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class ResetSlashTrigger : StateMachineBehaviour
    {
        [SerializeField]
        bool isOnExitOnly;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ClearTrigger(animator);
        }
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (isOnExitOnly)
                ClearTrigger(animator);
        }

        void ClearTrigger(Animator animator)
        {
            animator.ResetTrigger("Slash");
        }
    }
}
