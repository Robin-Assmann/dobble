using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {

	public static Manager manager;

	public Sprite[] Images;
	private List<int> Possible_Ids;

	[SerializeField]
	public GameObject buttons, pictures, PlayerScoreText, EnemyScoreText, wait;

	public Game currentGame;

	public bool isHost = false;

	void Awake(){
	
		DontDestroyOnLoad (gameObject);
		manager = this;

		this.Images = Resources.LoadAll<Sprite> ("Sprites");
		this.Possible_Ids = new List<int>{0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24};
	}

	public void CreateGame(){
	
		currentGame = new Game ();
	}

	public void ChangeGameState(string s){
	
		currentGame.ChangeState (s);
	}

	public void SendEvent(string msg){
	
		gameObject.GetComponent<NetworkManagers> ().SendMessage (msg);

	}

	public void ResetRound(){

		gameObject.GetComponent<NetworkManagers> ().ResetServerRound();
		//SendEvent ("Next");
	}

	public void LoadScene(int id){
	
		SceneManager.LoadScene (id);

	}

	public void Disconnect(){
	
	
	}

	#region Returns

	public List<Sprite> GetPlayerImages(List<int> Image_Ids){

		List<Sprite> tp_list = new List<Sprite> ();

		for (int i = 0; i < Image_Ids.Count; i++) {
		
			tp_list.Add (Images [Image_Ids[i]]);
		}
		return tp_list;	
	}

	public List<int> GetPlayerIds(int Count){

		List<int> tp_list = new List<int>(this.Possible_Ids);
		List<int> return_list = new List<int> ();
		Debug.Log (tp_list.Count);
		for (int i = 0; i < Count; i++) {
			int id = Random.Range (0, tp_list.Count);
			int value = tp_list [id];
			//print (value);
			return_list.Add (value);
			tp_list.Remove (value);		
		}
		return return_list;	
	}

	public List<Sprite> GetGameImages(List<int> Image_Ids, int Count){

		List<Sprite> tp_list = new List<Sprite> ();
		List<int> tp_Ids = new List<int>(this.Possible_Ids);

		for (int i = 0; i < Image_Ids.Count; i++) {

			tp_Ids.Remove (Image_Ids [i]);
		}

		for (int i = 0; i < Count; i++) {
			int id = Random.Range (0, tp_Ids.Count);
			int value = tp_Ids [id];
			tp_list.Add (Images[value]);
			tp_Ids.Remove (value);
		}
		return tp_list;	
	}

	public List<int> GetGamePictures(List<int> player, List<int> enemy, int player_id, int enemy_id){

		List<int> pos = new List<int>(this.Possible_Ids);
		print (pos.Count);
		print ("Stats :" + player.Count + "/" + enemy.Count + "/" + player_id + "/" + enemy_id);
		for (int i = 0; i < enemy.Count; i++) {
			if (pos.Contains (enemy [i]))
				pos.Remove (enemy [i]);
		}
		for (int i = 0; i < player.Count; i++) {
			if (pos.Contains (player [i]))
				pos.Remove (player [i]);
		}

		List<int> return_list = new List<int> ();
		int u = 0;
		if (player_id != enemy_id) {
			return_list.Add (player_id);
			return_list.Add (enemy_id);
		} else {
			return_list.Add (player_id);
			u = 1;
		}
		for (int i = 0; i < 6+u; i++) {
		
			int o = Random.Range (0, pos.Count);
			return_list.Add (pos [o]);
			pos.RemoveAt (o);		
		}

		for (int i = 0; i < return_list.Count; i++) {
			int temp = return_list [i];
			int randomIndex = Random.Range (i, return_list.Count);
			return_list [i] = return_list [randomIndex];
			return_list [randomIndex] = temp;		
		}
		return return_list;
	}

	#endregion
}
