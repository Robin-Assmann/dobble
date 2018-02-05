using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour {

	private bool rightOne = false;


	public void Init(bool isRightOne){
	
		this.rightOne = isRightOne;	
	}

	public void SubmitResult(){
	
		if (Manager.manager.isHost) {
			if (!rightOne)
				Manager.manager.SendEvent ("Result/0");
			else
				Manager.manager.SendEvent ("Result/1");
		} else {
			if (!rightOne)
				Manager.manager.SendEvent ("Result1/0");
			else
				Manager.manager.SendEvent ("Result1/1");
		
		}
	}

	public void Back(){
	
		Manager.manager.Disconnect ();
		Manager.manager.LoadScene (0);
	
	}



}
