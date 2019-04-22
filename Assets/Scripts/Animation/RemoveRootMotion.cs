using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class RemoveRootMotion : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.applyRootMotion = false;
        }
    }
}

