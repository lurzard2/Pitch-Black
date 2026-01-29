using UnityEngine;
using System.Collections.Generic;

namespace PitchBlack;

public interface IManipulator
{
    // Manipulation behavior back-end
    void Act();
    // Manipulation of target front-end
    void ManipulateTarget(Cycle target);
    // Change stuff with my graphics
    void ManipulateGraphics(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam);
}
