using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("追跡対象")]
    public GameObject otherTarget;      // ターゲット

    void Update()
    {
        //インゲームプレイ中でなければ何もしない
        if (GameManager.gameState != GameState.InGame) return;

        if (otherTarget != null)　//対象がいる場合
        {
            //プレイヤーと対象の中間でカメラ位置を固定
            Vector2 pos = Vector2.Lerp(transform.position, otherTarget.transform.position, 0.5f);
            // Lerp(進捗率)を使って、プレイヤー位置x,y、対象位置x,y、その0.5f(半分の進捗)

            Camera.main.transform.position = new Vector3(pos.x, pos.y, -10);
        }
        else //特に対象がいない場合は
        {
            // カメラをプレイヤーの位置と連動させる
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
            // Camera.mainというスタティック変数で誰でもメインカメラを探せる。
            // z軸は-10固定。プレイヤーの位置にすると、カメラのz軸もプレイヤーのところに移動してしまうため
        }
    }
}
