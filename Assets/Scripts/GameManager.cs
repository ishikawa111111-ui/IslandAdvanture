using UnityEngine;

// ゲームの状態
public enum GameState   //enum列挙型。この自作型はpublic他のクラスから存在は認識
{
    InGame,         // ゲーム中
    GameClear,      // ゲームクリア
    GameOver,       // ゲームオーバー
    GameEnd,        // ゲーム終了
}

public class GameManager : MonoBehaviour
{
    public static GameState gameState = GameState.InGame;   //初期値InGame
    // static 唯一無二、誰からも参照できる。ゲーム中ずっと確保される
    // gameStateをstatic化する

    void Start()
    {
        // Application.targetFrameRate = 10;   //勝手にフレームレートを落とす


        //初期状態はゲーム中からはじまる
        gameState = GameState.InGame;
    }
}