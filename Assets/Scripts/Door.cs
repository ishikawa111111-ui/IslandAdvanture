using UnityEngine;

public class Door : MonoBehaviour
{
    private UIController _uiCnt;    //UIControllerスクリプトを参照できる変数
    private string _id; //セーブID


    void Start()
    {
        //UIControllerスクリプトを探してくる
        _uiCnt = GameObject.FindFirstObjectByType<UIController>();

        //セーブデータMaanger
        _id = SaveDataManager.MakeId(gameObject);
        if (SaveDataManager.IsUsed(_id))    //すでに使われているIDなら
        {
        }

    }
    //ドアに接触したとき
    //1. 相手がPlayerタグ
    //2. 鍵が1以上
    //ドア消滅、UIControllerの鍵テキスト更新

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (ItemKeeper.hasKeys > 0)
            {
                Destroy(gameObject);    //ドア消滅
                ItemKeeper.hasKeys -= 1;
                _uiCnt.UpdateItemCount(ItemType.key, ItemKeeper.hasKeys);
                SaveDataManager.SetUsed(_id);   //開錠したのでリストに刻まれる
                SoundManager.soundManager.SEPlay(SEType.DoorOpen);
            }
            else
            {
                SoundManager.soundManager.SEPlay(SEType.DoorClosed);
            }
        }
    }

}
