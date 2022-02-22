using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAutoDestroy : MonoBehaviour
{

    public void destroyOnFinish()
    {
        Destroy(this.gameObject);
    }
}
