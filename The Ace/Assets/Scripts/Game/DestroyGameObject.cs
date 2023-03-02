using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObject : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Destroy(animator.transform.parent.gameObject, stateInfo.length);
    }
}
