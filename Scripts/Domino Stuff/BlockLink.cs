using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a server-side-active script that helps blocks keep track of which
/// dominos are sitting upon them and which are not.
/// </summary>
public class BlockLink : MonoBehaviour {

    BlockFace face;

    public void SetLink(BlockFace onBlock) {
        face = onBlock;
    }

    private void OnDestroy() {
        if (face != null)
            face.RemoveDomino(gameObject);
    }
}
