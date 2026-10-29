using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    //シーンを移動時、何番出口かを記録しておくstatic変数
    public static int currentDoorNumber = 0;


    void Start()
    {
        //string sceneName= SceneManaer.GetActiveScene().name;
        //SoundManager.soundManager.PlayBgm(BGMType.B );


        //↓FindGameObjectsWithTag、sがつ付いているので必ず配列で使用。sが付かないのは単品
        //  Exitタグが付いているものを拾い集めて、該当GameObjectをすべて配列exitsに格納
        GameObject[] exits = GameObject.FindGameObjectsWithTag("Exit");

        //foreachは配列の中を全点検する配列特化。
        foreach (GameObject exitObj in exits) //配列exitsの中を順番にexitObjに入れて、
        {
            if (currentDoorNumber == exitObj.GetComponent<Exit>().doorNumber) 
                //Exit.csをアタッチしている、そこにdoorNumberが書かれている
            {
                //番号が一致したExitオブジェのXY座標を取得
                float x = exitObj.transform.position.x;
                float y = exitObj.transform.position.y;

                //プレイヤー出現位置（出入口と重ならない、1個ずれる位置に出現）
                switch (exitObj.GetComponent<Exit>().direction)
                {
                    case Direction.up:
                        y += 1;
                        break;
                    case Direction.down:
                        y -= 1;
                        break;
                    case Direction.left:
                        x -= 1;
                        break;
                    case Direction.right:
                        x += 1;
                        break;
                    default: //上記のどれでもない場合
                        break;
                }

                //シーン上にいるプレイヤーを探してくる
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if(player != null)  //一応、シーンにプレイヤーがいるときに処理
                {
                    //プレイヤーを加工した座標に配置
                    player.transform.position=new Vector2(x, y);
                }
                break;  //ifの中で、該当するExitが見つかったのでforeachを抜ける
            }
        }
    }
}
