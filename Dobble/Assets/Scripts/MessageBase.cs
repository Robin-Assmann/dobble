using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class MessageBase{


	public virtual void Deserialize(NetworkReader reader){
	
	
	}
	public virtual void Serialize(NetworkReader writer){


	}
}

