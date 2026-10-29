using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    [Header("移動先シーン名")]
    [SerializeField] private string _sceneName;


    [Header("出入り口の識別番号・方向")]
    public int doorNumber;
    public Direction direction; // enum Direction down=0,left=1,up=2,right=3

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //static変数に自分の識別番号を授ける
            RoomManager.currentDoorNumber = doorNumber;

            // Load(); 即 Loadではなく、0.2秒後に移動させたいので、Invokeに書き換え
            Invoke("Load", 0.2f);
        }
    }

    private void Load() //シーンを切り替えるメソッド。Invokeに入れたいのでメソッド化
    {
        SceneManager.LoadScene(_sceneName);
    }
}
