using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game{

	private bool running;
	private string GameState ="";

	private WaitUntil WaitingForPlayer, WaitingForReady, WaitingForResult, WaitingNextRound;

	private List<int> EnemyIds;
	private List<int> PlayerIds;
	private bool enemyIds_loaded = false;
	private bool pictures_Loaded = false;

	private int Target = 0;
	private List<int> Pictures;

	private int PlayerScore, EnemyScore;

	public Game(){
		PlayerScore = EnemyScore = 0;
		InitializeWaitUntils ();
		StartGameLoop ();
	}

	private void StartGameLoop(){
		this.running = true;
		Manager.manager.StartCoroutine (GameLoop ());
	}

	private void InitializeWaitUntils(){
		this.WaitingForPlayer = new WaitUntil (() => (GameState.Equals ("WaitingForPlayer")));
		this.WaitingForReady = new WaitUntil (() => (GameState.Equals ("WaitingForReady")));
		this.WaitingForResult = new WaitUntil (() => (GameState.Equals ("WaitingForResult")));
		this.WaitingNextRound = new WaitUntil (() => (GameState.Equals ("WaitingNextRound")));
	}

	IEnumerator GameLoop(){
		Manager.manager.SendEvent ("Connect");
		yield return WaitingForPlayer;
		Manager.manager.LoadScene (1);

//		Manager.manager.PlayerScoreText = GameObject.FindGameObjectWithTag ("PlayerScore");
//		Manager.manager.EnemyScoreText = GameObject.FindGameObjectWithTag ("EnemyScore");

		while (this.running) {
			Debug.Log("next round");
			PlayerIds = Manager.manager.GetPlayerIds (6);
			if (Manager.manager.isHost) {
				yield return new WaitUntil (() => enemyIds_loaded);
			
				int enemy_target = EnemyIds [Random.Range (0, EnemyIds.Count)];

				if (!PlayerIds.Contains (enemy_target)) {
					this.Target = PlayerIds [Random.Range (0, EnemyIds.Count)];
				} else {
					this.Target = enemy_target;
				}

				if (EnemyIds.Contains (this.Target)) {
					enemy_target = this.Target;
				}

				Pictures = Manager.manager.GetGamePictures (PlayerIds, EnemyIds, this.Target, enemy_target);

				string s = "";
				foreach (int i in Pictures.ToArray()) {
					s=s+i+"/";
				}
				s = s.Remove (s.Length - 1);
				Debug.Log (s);
				Manager.manager.SendEvent ("Data+"+s+"+"+enemy_target);

			} else {
				string s = "";
				foreach (int i in PlayerIds.ToArray()) {
					s=s+i+"/";
				}
				s = s.Remove (s.Length - 1);
				Manager.manager.SendEvent ("Data+"+s);
				yield return new WaitUntil (() => pictures_Loaded);

			}
			ApplyButtons ();
			ApplyPictures ();
			Debug.Log("ready");

			Manager.manager.SendEvent ("Ready");

			yield return WaitingForReady;
			if(Manager.manager.wait==null)
			Manager.manager.wait = GameObject.FindGameObjectWithTag ("Wait");
			Manager.manager.wait.SetActive (false);
			//Manager.manager.SendEvent ("Result");

			yield return WaitingForResult;
			Debug.Log ("got result");
			if (Manager.manager.isHost) {
				Manager.manager.ResetRound ();
			}
			this.enemyIds_loaded = false;
			this.pictures_Loaded = false;
			//yield return WaitingNextRound;
			Debug.Log ("next got");
		}
		Debug.Log ("game done");
	}

	public void UpdateScore(bool isPlayer, int value){
	
		if (Manager.manager.PlayerScoreText == null) {
			Manager.manager.PlayerScoreText = GameObject.FindGameObjectWithTag ("PlayerScore");
			Manager.manager.EnemyScoreText = GameObject.FindGameObjectWithTag ("EnemyScore");
		}

		if (Manager.manager.isHost) {
		
			if (!isPlayer) { 
				if (value == 1) {
					PlayerScore++;
				} else {
					EnemyScore++;
				}
			} else {
				if (value == 1) {
					EnemyScore++;
				} else {
					PlayerScore++;
				}
			}
		}else{
			if (!isPlayer) { 
				if (value == 1) {
					EnemyScore++;
				} else {
					PlayerScore++;
				}
			} else {
				if (value == 1) {
					PlayerScore++;
				} else {
					EnemyScore++;
				}
			}
		}

		Manager.manager.PlayerScoreText.GetComponent<Text> ().text = this.PlayerScore+"";
		Manager.manager.EnemyScoreText.GetComponent<Text> ().text = this.EnemyScore+"";
	}

	private void ApplyButtons(){
		if (Manager.manager.buttons == null)
			Manager.manager.buttons = GameObject.FindGameObjectWithTag ("Buttons");

		for (int i = 0; i < Manager.manager.buttons.transform.childCount; i++) {
			if (PlayerIds [i] == this.Target) {
				Manager.manager.buttons.transform.GetChild (i).GetComponent<ButtonManager> ().Init (true);
			} else {
				Manager.manager.buttons.transform.GetChild (i).GetComponent<ButtonManager> ().Init (false);
			}
			Manager.manager.buttons.transform.GetChild (i).GetChild (0).GetComponent<Image> ().sprite = Manager.manager.Images [PlayerIds [i]];
		}
	}

	private void ApplyPictures(){
		if (Manager.manager.pictures == null)
			Manager.manager.pictures = GameObject.FindGameObjectWithTag ("Pictures");

		for (int i = 0; i < Manager.manager.pictures.transform.GetChild (0).childCount; i++) {
			Manager.manager.pictures.transform.GetChild (0).GetChild (i).GetComponent<Image> ().sprite = Manager.manager.Images [Pictures [i]];
		}
	}


	public void SetIds(string s){
		Debug.Log ("EnemyIds" + s);
	
		EnemyIds = new List<int> ();
		string[] t = s.Split ("/" [0]);

		for (int i = 0; i < t.Length; i++) {
		
			EnemyIds.Add (int.Parse (t [i]));
		}
		this.enemyIds_loaded = true;
	}

	public void SetPictures(string s){
		Debug.Log (s);
		Pictures = new List<int> ();
		string[] t = s.Split ("/" [0]);

		for (int i = 0; i < t.Length; i++) {

			Pictures.Add (int.Parse (t [i]));
		}
		this.pictures_Loaded = true;
	}

	public void SetTarget(int s){
	
		this.Target = s;
	
	}

	public void ChangeState(string change){
	
		Debug.Log ("change to" + change);
		this.GameState = change;
	}

}
