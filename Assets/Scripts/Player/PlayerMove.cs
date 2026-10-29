using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 方向
public enum Direction   //列挙型,数字も持っている形にしている
{
    down = 0,    // 下
    left = 1,    // 左
    up = 2,    // 上
    right = 3,    // 右
}

public class PlayerMove : MonoBehaviour
{
    [Header("スピード・方向")] //インスペクタに表示する見出し(後で思い出せるようにmemo)
    [SerializeField] private float _speed = 3.0f;          // 移動スピード
    [SerializeField] private Direction _direction = 0;     // 移動方向
                                                           // [SerializeField] private だと、publicじゃなくてプライベートの型でもインスペクタに表示できる

    [Header("UIスライダー")]
    [SerializeField] private Slider _lifeSlider;
    [SerializeField] private TextMeshProUGUI _lifeText; //頭上ライフテキスト版

    private Vector2 _moveVec;            // 移動ベクトル
    private float _angleZ = -90.0f;      // 回転角度

    private Rigidbody2D _rbody;       // 指定したRigidbody2Dコンポ―ネントを参照できる変数
    private Animator _animator;       // 指定したAnimatorコンポ―ネントを参照できる変数

    public static float life = 1.0f;    // プレイヤーのライフ 1が満タン、0がゲームオーバー
    private bool _inDamage = false;              // ダメージ中フラグ

    private UIController _uiCnt;  //あとでUIコントローラーを作る

    RectTransform rect; //頭上にUI出せるかな


    // Moveアクションを受け取る
    void OnMove(InputValue value)
    {
        _moveVec = value.Get<Vector2>();    //スティックの傾きをVeector2にしてGETした(Vector2なので、x,y )
    }

    // Z軸の角度をfloatで返す
    public float GetAngle()
    {
        float angle = _angleZ;       // とりあえず前フレームの角度を代入
        if (_moveVec != Vector2.zero) //移動ベクトルがゼロでなければ書き替え=「方向キー入力がない」でなければ
        {
            //スティックの傾き方向を求める　↓縦y、横xの順に書く
            float rad = Mathf.Atan2(_moveVec.y, _moveVec.x);  // Atan2で現フレームの向きのZ軸の角度(ラジアン)を求める
            angle = rad * Mathf.Rad2Deg; // Z軸のラジアンをデグリー値（度）に変換
        }
        return angle;
    }

    // Z軸の角度からDirection値を算出して返す
    Direction AngleToDirection()    //スティックの入力角度がわかったので、４方向に上下左右（アニメ切り替え用）
    {
        Direction dir;
        if (_angleZ >= -45 && _angleZ < 45) dir = Direction.right;  // 右向き
        else if (_angleZ >= 45 && _angleZ <= 135) dir = Direction.up;     // 上向き
        else if (_angleZ >= -135 && _angleZ <= -45) dir = Direction.down;   // 下向き
        else dir = Direction.left;   // 左向き
        return dir;
    }

    private void Awake()    //プレイヤーが死んでやり直すときに呼び出される
    {
        if (life <= 0)
        {
            life = 1.0f;    // ライフが0の場合1に戻す　1.0f=100%
        }
    }

    void Start()
    {
        _rbody = GetComponent<Rigidbody2D>();    // PalyerのRigidbody2Dを変数rbodyで参照
        _animator = GetComponent<Animator>();    // PlayerのAnimatorを変数animatorで参照

        _uiCnt = GameObject.FindFirstObjectByType<UIController>();   // シーン中のUIController取得

        rect = GetComponent<RectTransform>();   //頭上にUI出せるかな
    }

    void Update()
    {
        // ゲーム中以外とダメージ中は何もしない
        if (GameManager.gameState != GameState.InGame || _inDamage) //InGameではない or ダメージ中である
        {
            if (_inDamage)
            {
                // ダメージ中点滅させる
                float val = Mathf.Sin(Time.time * 50);  //三角関数(Sin)を使って一定周期(リズム)で切り替える
                // val が、正の値と負の値をサイン波でひょろひょろする。角度（長辺を1としたときの高さ）
                // ゲームの経過時間をx座標、valをy座標のグラフと考えると理解できる

                if (val > 0)    // valが正の値なら
                {
                    GetComponent<SpriteRenderer>().enabled = true;  // スプライト表示
                }
                else            // valが正の値ではない(=だいたい負)
                {
                    GetComponent<SpriteRenderer>().enabled = false; // スプライト非表示
                }
            }
            return; // 現フレームのUpdateを中断する（Updateは次フレームでまた動くけどね）
        }

        //InGameだった場合なおかつダメージ中ではない場合

        _angleZ = GetAngle();  // 現フレームの角度を取得
        Direction dir = AngleToDirection(); // 現フレームのDirection値(4方向)を取得
        if (dir != _direction)　// 前フレームと向いている方向が違う
        {
            _direction = dir;      // 向いている方向を更新            
            _animator.SetInteger("Direction", (int)_direction);　//アニメーション更新（intに型変換)
        }

        _lifeText.text = ("HP"+ (life * 100) + "%").ToString();
        //rect.localPosition += new Vector3(10, 0, 0);
        //        _lifeSlider.value = life;
    }

    void FixedUpdate()
    {
        // ゲーム中以外とダメージ中は何もしない
        if (GameManager.gameState != GameState.InGame || _inDamage)
        {
            return;
        }

        // ダメージ中じゃないand InGame宙の場合の処理
        //_moveVecはVector2型でスティック入力方向、_rbodyに力(_moveVec * _speed)を加える
        _rbody.linearVelocity = _moveVec * _speed; //velocity値を決定
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Enemyタグとぶつかったら
        if (collision.gameObject.tag == "Enemy")
        {
            //ダメージを負うメソッド
            GetDamage(collision.gameObject);
        }
    }

    // ダメージ処理
    void GetDamage(GameObject target)
    {
        if (GameManager.gameState == GameState.InGame)  //InGame中なら（クリア時などではない）
        {
            SoundManager.soundManager.SEPlay(SEType.GetDamage);

            life -= 0.25f; //ライフを減らす
            //ライフを更新
            _uiCnt.UpdateLife(PlayerMove.life);

            if (life > 0) //もしライフが残っていれば
            {
                _rbody.linearVelocity = new Vector2(0, 0);   // 移動停止

                // ぶつかった相手の反対方向にヒットバックさせる・自分の位置マイナス敵の位置の角度がノックバック方向
                // 方向を取得して距離を強制的に1に整える（.normalized 正規化）方向だけが欲しいので。
                // 敵と自分の距離が影響すると、ノックバックの距離にも影響が出てしまう
                Vector3 v = (transform.position - target.transform.position).normalized;
                _rbody.AddForce(new Vector2(v.x * 4, v.y * 4), ForceMode2D.Impulse); //正規化されているので4倍 
                _inDamage = true;            // ダメージフラグ ON
                Invoke("DamageEnd", 0.25f); // 0.25秒後にInDamageを終了させる
            }
            else
            {
                GameOver();　//体力がなければゲームオーバー
            }
        }
    }
    // ダメージ終了
    void DamageEnd()
    {
        _inDamage = false; // ダメージフラグ OFF
        GetComponent<SpriteRenderer>().enabled = true; // スプライトを元に戻す。点滅中に消えたままだと困るので。
    }

    // ゲームオーバー
    void GameOver()
    {
        GameManager.gameState = GameState.GameOver; //ゲームステータスをGameOverに変更

        // ゲームオーバー演出
        GetComponent<CircleCollider2D>().enabled = false;       // 当たり判定を消す
        _rbody.linearVelocity = new Vector2(0, 0);               // 移動停止
        _rbody.gravityScale = 1;                                 // 重力を戻す（画面外へ落としたいので復活させている）
        _rbody.AddForce(new Vector2(0, 5), ForceMode2D.Impulse); // プレイヤーを上に少し跳ね上げる

        _animator.SetTrigger("IsDead"); // IsDeadトリガーをONにしている。アニメーションを切り替える

        // Playerマップの無効化とUIマップの有効化
        PlayerInput input = GetComponent<PlayerInput>();        // PlayerInput取得
        input.currentActionMap.Disable();                       // Player無効化
        input.SwitchCurrentActionMap("UI");                     // アクションマップをUIに変更
        input.currentActionMap.Enable();                        // UIマップ有効化

        Invoke("PlayerGravityEnd", 2.0f); //アニメーション演出が終わるころ落下は停止
                                          //永久に死体が落ち続けないように止める

        SoundManager.soundManager.StopBgm();
        SoundManager.soundManager.SEPlay(SEType.GameOver);
    }

    void PlayerGravityEnd()
    {
        _rbody.gravityScale = 0; // 重力をなくして静止させる
    }
}