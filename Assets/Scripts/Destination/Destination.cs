using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Destination : GameTileContent
{
    //Get destination type
    public abstract DestinationType DestinationType { get;}

    //Tell if the floor need to be updated
    public abstract bool isUpdatingContent { get; }
    
}
