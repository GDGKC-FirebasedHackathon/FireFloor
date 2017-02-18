using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour {

    public float ratio;
    public float Toque;

    public Image progressBar;
    public Image freezeNotifier;

    private AsyncOperation loadManager;

	// Use this for initialization
	void Start () {
        //StartCoroutine(LoadGameScene());
	}
	
	// Update is called once per frame
	void Update () {
        progressBar.rectTransform.sizeDelta = new Vector2(300 * ratio - 8, progressBar.rectTransform.rect.height);
        rotate();
        //300은 프로레스바 전체의 길이

        //리씨빙 데이터
        /*
        흐음;
        */
    }

    IEnumerator LoadGameScene()
    {
        loadManager = SceneManager.LoadSceneAsync("game");

        yield return loadManager;
    }

    void rotate()
    {
        freezeNotifier.rectTransform.Rotate(Vector3.back * Toque * Time.deltaTime);
    }
}
