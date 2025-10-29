using Leap;
using System.Collections.Generic;
using UnityEngine;

public class Example: MonoBehaviour
{
    private void Update()
{
    Hand _hand = Hands.Provider.GetHand(Chirality.Left); //Get just the left hand.

    float _pinchStrength = _hand.PinchStrength;

    bool _isPinching = false;

    if(_pinchStrength > 0.8 && !_isPinching) //We check _isPinching so this code isn't called if we are already pinching.
    {
      _isPinching = true; // grab strength is over 0.8 threshold so it is true
    }
    else if(_pinchStrength < 0.7 && _isPinching) //We check _isPinching so this code isn't called if we aren't already pinching.
    {
      _isPinching = false; // pinch strength is less than 0.7 so no chance of jittering grab.
    }
}
}