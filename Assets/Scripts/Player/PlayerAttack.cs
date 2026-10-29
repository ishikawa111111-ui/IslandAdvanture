using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("矢の速度・発射間隔")]
    [SerializeField] private float _shootSpeed = 12.0f;    // 矢の速度
    [SerializeField] private float _shootDelay = 0.25f;    // 発射間隔

    [Header("矢・弓のプレハブ")]
    [SerializeField] private GameObject _arrowPrefab;      // 矢のプレハブ
    [SerializeField] private GameObject _bowPrefab;        // 弓のプレハブ

    [Header("剣・斬撃のプレハブ")]
    [SerializeField] private GameObject _swordPrefab;        // 剣のプレハブ
    [SerializeField] private GameObject _swordEffectPrefab;        // 斬撃のプレハブ

    //発動したコルーチン(クローン生成)情報が入る変数
    private Coroutine _swordAttackCoroutine;    
    private Coroutine _arrowAttackCoroutine;

    private GameObject _bowObj;         // 弓のゲームオブジェクト
    private GameObject _swordObj;         // 剣のゲームオブジェクト

    //InputSystem上の特定のActionが入る変数
    private InputAction _bowAttackAction;           // 弓Attackアクション
    private InputAction _swordAttackAction;           // 剣Attackアクション

    private float _pressTime;                    // 長押し時間記録

    private PlayerMove _playerMove;　//PlayerMoveコンポーネントを参照する変数

    private UIController _uiCnt; //UIControllerコンポーネントを参照する変数

    // 弓Attackアクションのコールバック
    void OnAttackCallback(InputAction.CallbackContext context)
    {
        // ItemKeeper.hasArrows += 1; // 矢を無限に撃てるように1本増やすデバッグ用

        //矢を持っている本数チェック
        if (ItemKeeper.hasArrows > 0)
        {
            if (_swordAttackCoroutine == null)
                ShootArrow();       //矢を発射
            // Debug.Log("矢の本数：" + ItemKeeper.hasArrows);
        }
        
    }

    // 剣Attackアクションのコールバック
    void OnLongPressStarted(InputAction.CallbackContext context)
    {
        Slash();
    }

    //矢を発射
    void ShootArrow()
    {
        if (_arrowAttackCoroutine == null)  //コルーチンがnullなら＝コルーチン発動中なら何もしない
        {
            SoundManager.soundManager.SEPlay(SEType.Shoot);


            float currentAngleZ = _playerMove.GetAngle(); //現在のPlayer角度を取得
            //オイラーデグリー　何度という情報を得る

            _bowObj.transform.localScale = new Vector3(1, 1, 1); //弓の変形を戻す＝localScaleを1倍
            ItemKeeper.hasArrows -= 1; // 矢を減らす

                            // 矢のゲームオブジェクトを作る(進行方向に回転)
            Quaternion r = Quaternion.Euler(0, 0, currentAngleZ);   //クォータニオンのオイラー
            GameObject arrowObj = Instantiate(_arrowPrefab, transform.position, r);

            //if (_pressTime >= 1)    //チャージショット　1秒以上だったら
            if (_pressTime >= 1)    //チャージショット　1秒以上だったら
            {
                Debug.Log("ため打ち成立");
                ArrowController arrow = arrowObj.GetComponent<ArrowController>();
                arrow.attackPower *= 2;     //攻撃力２倍
                Debug.Log("攻撃力：" + arrow.attackPower);
            }

            // 矢を発射するベクトルを作る
            float x = Mathf.Cos(currentAngleZ * Mathf.Deg2Rad); //角度をVector底辺ラジアンに置き換えている
            float y = Mathf.Sin(currentAngleZ * Mathf.Deg2Rad); //角度をVector高さラジアンに置き換えている
            Vector3 v = new Vector3(x, y) * _shootSpeed;    //Vector2でもOK
            //長辺を１とした場合の数値が出るので、↓

            // 矢に力を加える
            Rigidbody2D body = arrowObj.GetComponent<Rigidbody2D>();
            body.AddForce(v, ForceMode2D.Impulse);


            _uiCnt.UpdateItemCount(ItemType.arrow, ItemKeeper.hasArrows);  // 矢を更新
            _uiCnt.UpdatePower(0);                                         // パワーを更新

            //SoundManager.soundManager.SEPlay(SEType.Shoot);     // SE再生（矢を射る）

            _arrowAttackCoroutine = StartCoroutine(ArrowAttack());  //コルーチンを発動してその情報を変数に入れる
            //StartCoroutine指定したコルーチンを発動させるメソッド（すぐ下のIEnumerator ArrowAttack()参照）
            //_arrowAttackCoroutineは0.2秒後にnullになる＝再度矢が撃てる
        }
    }

    //コルーチン
    //コルーチンは単体でやりたいこと（wait処理）と他の処理を混ぜることができる
    //Invokeはやりたいことを別のメソッドで書かないといけないので違いがある
    IEnumerator ArrowAttack()   //アイイナムレーター(←これは任意の名前)
    {
        yield return new WaitForSeconds(0.2f);  //イールドリターン（コルーチン専用）
        //0.2秒、待ってから、下の処理(null)をしてリターン
        _arrowAttackCoroutine = null;   //矢の発射中をリセット
    }

    //剣を振る
    void Slash()
    {
        //ソードも、矢もコルーチンがまだ発動してなければ実行(null)
        if(_swordAttackCoroutine == null && _arrowAttackCoroutine == null)
        {
            SoundManager.soundManager.SEPlay(SEType.Shoot);

            _bowObj.SetActive(false);   //弓の存在を一時消しておく
            _swordObj.SetActive(false); //剣の存在を一時消しておく

            float currentAngleZ = _playerMove.GetAngle(); //現在のPlayer角度を取得
            Quaternion r = Quaternion.Euler(0, 0, currentAngleZ + 90); //角度をラジアン値に変換
            //しかるべき角度でソードエフェクトを生成
            GameObject swordEffectObj = Instantiate(_swordEffectPrefab, transform.position, r);
            swordEffectObj.transform.SetParent(transform);　//Playerをエフェクトの親とする

            //Playerの角度情報（すなわち向き）に応じて生成したエフェクトの位置を調整
            if (currentAngleZ >= -135 && currentAngleZ <= -45)  //プレイヤーが下向きの時
            {
                swordEffectObj.transform.localPosition = new Vector3(0,0.3f,0); //絵の都合で位置を微調整
            }
            else if (currentAngleZ >= 45 && currentAngleZ <= 135)  //プレイヤーが上向きの時
            {
                swordEffectObj.transform.localPosition = new Vector3(0, 0.2f, 0); //絵の都合で位置を微調整
            }

            //ソードを維持→やがて消滅　を時限式に行うコルーチンを発動
            _swordAttackCoroutine = StartCoroutine(SwordAttack(swordEffectObj));    //引数にswordEffectObj
        }
    }

    //コルーチンSwordAttackの作成（ソードを維持→やがて消滅）
    IEnumerator SwordAttack(GameObject swordEffectObj)
    {
        yield return new WaitForSeconds(0.3f); //0.3秒経つまで待ってから以下を実行
        Destroy(swordEffectObj); //ソードエフェクトを消す
        _swordAttackCoroutine = null; //コルーチン情報をnullにする

        _bowObj.SetActive(true);  //一時的に非表示にしていた弓を復活
        _swordObj.SetActive(true);  //一時的に非表示にしていた剣を復活
    }

    //このスクリプトが無くなることがあれば　Playerオブジェごといなくなる、など
    //=攻撃が予約されていたが、攻撃前にプレイヤーが死んだ場合など
    //誤作動防止のために(念のため)コールバックを解放しておく
    void OnDisable()    //もう何もやらなくていい（矢と剣）
    {
        if (_bowAttackAction != null)   //nullではない＝変数にアクションが指定されていれば
        {
            _bowAttackAction.canceled -= OnAttackCallback;    // キャンセルコールバック削除
        }
        if (_swordAttackAction != null)
        {
            _swordAttackAction.started -= OnLongPressStarted; // キャンセルコールバック削除
        }
    }

    void Start()
    {
        _playerMove = GetComponent<PlayerMove>(); //変数playerMoveがPlayerのPlayerMoveコンポーネントを参照

        // 攻撃アイテムをプレイヤーキャラクターに配置
        Vector3 pos = transform.position;

        //弓の位置
        _bowObj = Instantiate(_bowPrefab, pos, Quaternion.identity);    //インスタンティエート
        _bowObj.transform.SetParent(transform); // 弓の親にプレイヤーキャラクターを設定する
        //プレイヤーの基準点は画像中心、弓の基準点は弓の根本あたり

        //剣の位置
        _swordObj = Instantiate(_swordPrefab, pos, Quaternion.identity);
        _swordObj.transform.SetParent(transform); // 剣の親にプレイヤーキャラクターを設定する

        //Attackアクションにコールバックを設定
        PlayerInput input = GetComponent<PlayerInput>();

        //PlayerのアクションセットからAttackを探し（＝西ボタン）、_bowAttackActionで使えるようにする
        _bowAttackAction = input.currentActionMap.FindAction("Attack");

        //_bowAttackActionがキャンセルされたとき＝ボタンが離されたとき、OnAttackCallbackが足される（予約登録）
        _bowAttackAction.canceled += OnAttackCallback;     // キャンセルコールバック（自作メソッド）登録

        //PlayerのアクションセットからJumpを探し（＝南ボタン）、_bowAttackActionで使えるようにする
        _swordAttackAction = input.currentActionMap.FindAction("Jump");

        //_swordAttackActionがstarted押されたとき、OnAttackCallbackが足される（予約登録）
        _swordAttackAction.started += OnLongPressStarted;  // 開始コールバック（自作メソッド）登録

        //↑複数のボタンが操作される可能性があるので、+=で発動するものを予約登録している



        InputActionMap uiMap = input.actions.FindActionMap("UI");   // UIマップ取得
        uiMap.Disable();                                            // UIマップ無効化

        _uiCnt = GameObject.FindFirstObjectByType<UIController>();   // シーン中のUIController取得
    }

    void Update()
    {
        float currentAngleZ = _playerMove.GetAngle();　//現在のPlayer角度を取得

        DrawAttackItemPosition(currentAngleZ);  //武器の描画優先度を変更して描き直す

        //矢を持っている
        if (ItemKeeper.hasArrows > 0)
        {
            //長押し時間 GetTimeoutCompletionPercentage 長押し計測メソッド
            _pressTime = _bowAttackAction.GetTimeoutCompletionPercentage();

            Debug.Log("長押し：" + _pressTime);


            if (_pressTime > 0)
            {
                //長押し時間に合わせて弓の画像を変形させる
                _bowObj.transform.localScale = new Vector3(1 - (_pressTime * 0.2f), 1 + (_pressTime * 0.5f), 1);

                _uiCnt.UpdatePower(_pressTime);	// パワーを更新

                float percent = _bowAttackAction.GetTimeoutCompletionPercentage() * 100f;
                Debug.Log($"{percent:F0}%");
            }
        }
    }

    void DrawAttackItemPosition(float currentAngleZ)    //前提：プレイヤーのレイヤーは３、currentAngleZ角度
    {
        int orderNumber = 4; // 描画アイテムの オーダーインレイヤー(キャラクターの手前にする)

        // 弓描画の優先順位
        if (currentAngleZ > 30 && currentAngleZ < 150)  //プレイヤー上向き
        {
            // 上向き
            orderNumber = 2; // 描画アイテムの オーダーインレイヤー(キャラクターの後ろにする)
        }
        // 弓の回転
        _bowObj.transform.rotation = Quaternion.Euler(0, 0, currentAngleZ); //プレイヤーの向きと同じ角度
        // 弓の配置
        _bowObj.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        // 描画の優先順位
        _bowObj.GetComponent<SpriteRenderer>().sortingOrder = orderNumber;

        orderNumber = 4;    //リセット


        // 剣描画の優先順位 それぞれの剣の位置は絵に合わせて微調整。左手の位置
        if (currentAngleZ >= -45 && currentAngleZ < 45) // 右向き
        {
            _swordObj.transform.localPosition = new Vector3(0.2f, 0.53f, 0);
            orderNumber = 2; // 描画アイテムの オーダーインレイヤー(キャラクターの後ろにする)
        }
        else if (currentAngleZ >= 45 && currentAngleZ <= 135)  // 上向き
        {
            _swordObj.transform.localPosition = new Vector3(-0.3f, 0.53f, 0);
            orderNumber = 2; // 描画アイテムの オーダーインレイヤー(キャラクターの後ろにする)
        }
        else if (currentAngleZ >= -135 && currentAngleZ <= -45) // 下向き
        {
            _swordObj.transform.localPosition = new Vector3(0.25f, 0.25f, 0);
        }
        else  // 左向き
        {
            _swordObj.transform.localPosition = new Vector3(-0.05f, 0.18f, 0);
        }
        _swordObj.GetComponent<Sword>().GetSwordObject().GetComponent<SpriteRenderer>().sortingOrder = orderNumber;

    }
}
