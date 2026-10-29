using System;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [SerializeField] private Sprite _openImage;
    [SerializeField] private GameObject _itemprefab;    //中に入っているアイテム
    private bool _isClosed = true;  //true＝宝箱閉じている
    private string _id;     // セーブ用のID

    void Start()
    {
        // このシーンが読み込まれた場合、毎回最初に処理
        // すでに開けた箱なら開いた状態にする
        _id = SaveDataManager.MakeId(gameObject);
        if (SaveDataManager.IsUsed(_id))
        {
            GetComponent<SpriteRenderer>().sprite = _openImage; //開いた絵に切り替え
            _isClosed = false;
            SpawnItem();    // 中身をまだ取っていなければもう一度出す
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)  //CollisionからTriggerに変更（）
    // private void OnCollisionEnter2D(Collision2D collision)

    {
        if (_isClosed && collision.gameObject.tag == "Player")  //宝箱閉じている＆プレイヤータグに接触したら
        {
            GetComponent<SpriteRenderer>().sprite = _openImage; //開ける絵
            _isClosed = false;
            SaveDataManager.SetUsed(_id);   // 開けたことを記録
            SpawnItem();
        }
    }

    // 中身のアイテムを出す
    void SpawnItem()
    {
        if(_itemprefab == null) return; //中身のアイテムが指定されていなければ、何もせずリターン

        string itemId = _id + "_Item";  // 箱から出るアイテムは箱のIDをもとにする

        if (SaveDataManager.IsUsed(itemId)) return;     // セーブ確認してすでに取っていれば出さない
        
        GameObject itemObj = Instantiate(_itemprefab, transform.position, Quaternion.identity);
        //インスタンティエート＝複製クローン

        itemObj.GetComponent<ItemData>().id = itemId;
    }
}
