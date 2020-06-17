using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationNormal : Destination
{
    //Return destination type
    public override DestinationType DestinationType => DestinationType.Normal;

    public override bool isUpdatingContent => false;
}
