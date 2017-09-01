using UnityEngine;
using System.Collections;

public class ChoiseCard : MonoBehaviour {

    public int cardIndex;

    public void SendIndexToGameManager()
    {
        GameManager.inatance.SetTakeCardIndex(cardIndex);
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
