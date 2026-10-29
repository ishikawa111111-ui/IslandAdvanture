using UnityEngine;
using UnityEngine.SceneManagement;

public class LightController : MonoBehaviour
{
    [Header("ライトオブジェクト・使用フラグ")]
    [SerializeField] private GameObject _lightObj;     // ライトのゲームオブジェクト
    [SerializeField] private bool _isUseLight;         // ライト使用フラグ

    [Header("Light使用対象Scene名")]
    [SerializeField] private string[] _hasLightSceneNames; //Light使用対象Scene名の配列

    private PlayerMove _playerMove;　//PlayerMoveコンポーネントを参照する変数

    void Start()
    {
        _playerMove = GetComponent<PlayerMove>();　//変数playerMoveがPlayerのPlayerMoveコンポーネントを参照

        _isUseLight = false; //ライトの使用をいったんOFF

        string currentSceneName = SceneManager.GetActiveScene().name; //現在シーン名を取得
        //配列_hasLightSceneNamesの中を全点検
        foreach (string names in _hasLightSceneNames)   //foreach で[]入れて順番に全点検
        {
            //現在シーン名が使用対象と一致していればライト使用をON
            // if (currentSceneName == names) _isUseLight = true;   //わざとOFFにしてみた
        }

        //使用フラグによってLightオブジェクトの表示/非表示
        // if (_isUseLight) _lightObj.SetActive(true); //ライト点ける
        // else _lightObj.SetActive(false);            //ライト消す
        //↑と同じ。短く書けた
        _lightObj.SetActive(_isUseLight);   //ライト点ける/消す
    }

    void Update()
    {
        // ライト使用フラグがON
        if (_isUseLight)
        {
            float currentAngleZ = _playerMove.GetAngle();　//現在のPlayer角度（スティック入力方向）を取得
            // Player角度に合わせてライトを回転させる。下向き固定しているので、本来の角度に戻すため+90度
            _lightObj.transform.localEulerAngles = new Vector3(0, 0, currentAngleZ + 90);
        }
    }
}