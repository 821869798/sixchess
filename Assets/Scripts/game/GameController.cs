using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Runtime.Remoting.Messaging;

public enum GameState
{
	WHITE,
	BLACK,
	OVER
}

public class GameController : MonoBehaviour
{

	public static GameController instance = null;
	public GameObject boardCell;
	public GameObject chessWhite;
	public GameObject chessBlack;
	public GameObject chessChecked;
	private int[,]initChessMan = new int[4, 4]{  //白棋为1,黑棋为2
		{1,1,1,1},
		{1,0,0,1},
		{2,0,0,2},
		{2,2,2,2}
	};
//	private int[,]initChessMan = new int[4, 4]{  //白棋为1,黑棋为2
//		{1,1,1,1},
//		{1,2,1,0},
//		{2,0,2,2},
//		{2,0,0,2}
//	};
	private int[,]gameBoard = new int[4, 4];
	private GameObject[,]chessManArray = new GameObject[4, 4];
	private Stack<ChessData> chessDataList = new Stack<ChessData>();
	private GameState state;
	private GameObject checkedCell;
	private bool isChecked ;
	private int selectX, selectY;
	private int whiteCount, blackCount;

	public AudioClip eatAudio;
	public AudioClip selectAudio;
	public AudioClip moveAudio;
	public AudioClip deadAudio;
	public GameObject gameOverPanel;
	private int aiEffect;

	public Text turnText;

	void Awake ()
	{
		instance = this;
	}

	void Start ()
	{
		initBoardCell ();
		initGame ();
	}

	private void initBoardCell () //初始化棋子按钮和标记框
	{
		checkedCell = GameObject.Instantiate (chessChecked, transform.position, Quaternion.identity) as GameObject;
		checkedCell.transform.SetParent (this.transform);
		checkedCell.transform.SetSiblingIndex (0);
		checkedCell.transform.localScale = new Vector3 (1, 1, 1);
		for (int i=0; i<4; i++) {
			for (int j=0; j<4; j++) {
				GameObject cell = GameObject.Instantiate (boardCell, transform.position, Quaternion.identity) as GameObject;
				cell.transform.SetParent (this.transform);
				cell.transform.SetSiblingIndex (1);
				cell.transform.localScale = new Vector3 (1, 1, 1);
				cell.GetComponent<BoardCell> ().setPosition (i, j);
			}
		}
	}

	private void initGame () //初始化游戏
	{
		gameOverPanel.SetActive(false);
		//游戏数据初始化
		checkedCell.GetComponent<Image> ().enabled = false;
		state = GameState.WHITE;
		isChecked = false;
		whiteCount = 6;
		blackCount = 6;
		chessDataList.Clear();
		initTurnTitle();

		//棋牌棋子初始化
		for (int i=0; i<4; i++)
			for (int j=0; j<4; j++)
				gameBoard [i, j] = initChessMan [i, j];
		for (int i=0; i<4; i++) {
			for (int j=0; j<4; j++) {
				if (gameBoard [i, j] == 0) {
					chessManArray [i, j] = null;
				} else {
					GameObject chessMan = null;
					if (gameBoard [i, j] == 1)
						chessMan = GameObject.Instantiate (chessWhite, transform.position, Quaternion.identity) as GameObject;
					else
						chessMan = GameObject.Instantiate (chessBlack, transform.position, Quaternion.identity) as GameObject;
					chessMan.transform.SetParent (this.transform);
					chessMan.transform.SetSiblingIndex (0);
					chessMan.transform.localScale = new Vector3 (1, 1, 1);
					chessMan.GetComponent<BoardCell> ().setPosition (i, j);
					chessManArray [i, j] = chessMan;
				}
			}
		}
	}

	private void initWithChessData(ChessData cd){

		GameObject []go = GameObject.FindGameObjectsWithTag("chess");
		foreach(GameObject obj in go)
		{
			Destroy(obj);
		}
		for (int i=0; i<4; i++)
			for (int j=0; j<4; j++)
				gameBoard [i, j] = cd.gameBoard [i, j];
		isChecked = false;
		state = cd.state;
		whiteCount = cd.whiteCount;
		blackCount = cd.blackCount;
		initTurnTitle();
		for (int i=0; i<4; i++) {
			for (int j=0; j<4; j++) {
				if (gameBoard [i, j] == 0) {
					chessManArray [i, j] = null;
				} else {
					GameObject chessMan = null;
					if (gameBoard [i, j] == 1)
						chessMan = GameObject.Instantiate (chessWhite, transform.position, Quaternion.identity) as GameObject;
					else
						chessMan = GameObject.Instantiate (chessBlack, transform.position, Quaternion.identity) as GameObject;
					chessMan.transform.SetParent (this.transform);
					chessMan.transform.SetSiblingIndex (0);
					chessMan.transform.localScale = new Vector3 (1, 1, 1);
					chessMan.GetComponent<BoardCell> ().setPosition (i, j);
					chessManArray [i, j] = chessMan;
				}
			}
		}
		checkedCell.GetComponent<Image> ().enabled = true;
		checkedCell.GetComponent<BoardCell> ().setPosition (cd.checkedY,cd.checkedX);
	}

	private void initTurnTitle(){
		if(state==GameState.WHITE){
			turnText.text="轮到白棋";
		}else if(state==GameState.BLACK){
			turnText.text="轮到黑棋";
		}else{
			turnText.text="游戏结束";
		}
	}

	public void dealChessClick (int y, int x) //处理点击事件
	{
		if (state == GameState.OVER) {
			print ("over");
			return;
		}
		if (gameBoard [y, x] == 0 && isChecked) {  //处理走棋
			if ((Mathf.Abs (selectX - x) + Mathf.Abs (selectY - y)) == 1) {
				isChecked = false;
				if (state == GameState.WHITE) {
					moveChessMan (y, x, 1);
				} else {
					moveChessMan (y, x, 2);
				}
				chessDataList.Push(new ChessData(gameBoard,y,x,state,whiteCount,blackCount));
			}
		} else if (gameBoard [y, x] == 1 && state == GameState.WHITE) { //选中白棋
			if(Tool.getInstance().effectVolume!=0)
				AudioSource.PlayClipAtPoint(selectAudio,Vector3.zero,Tool.getInstance().effectVolume);
			checkedCell.GetComponent<Image> ().enabled = true;
			checkedCell.GetComponent<BoardCell> ().setPosition (y, x);
			isChecked = true;
			selectX = x;
			selectY = y;
		} else if (gameBoard [y, x] == 2 && state == GameState.BLACK && Tool.getInstance().gameType==0) { //选中黑棋
			if(Tool.getInstance().effectVolume!=0&&Tool.getInstance().gameType!=1)
				AudioSource.PlayClipAtPoint(selectAudio,Vector3.zero,Tool.getInstance().effectVolume);
			checkedCell.GetComponent<Image> ().enabled = true;
			checkedCell.GetComponent<BoardCell> ().setPosition (y, x);
			isChecked = true;
			selectX = x;
			selectY = y;
		}
	}

	private  void moveChessMan (int y, int x, int turn) //走棋 turn为是谁走的棋
	{
		gameBoard [y, x] = turn;
		gameBoard [selectY, selectX] = 0;
		chessManArray [y, x] = chessManArray [selectY, selectX];
		chessManArray [selectY, selectX] = null;
		chessManArray [y, x].GetComponent<BoardCell> ().setPosition (y, x);
		checkedCell.GetComponent<Image> ().enabled = true;
		checkedCell.GetComponent<BoardCell> ().setPosition (y, x);
		bool isEat = false;
		//横向判断有无吃子
		int [] tmp = new int[4];  //用于判断的临时数组
		if (turn == 1) {          //白子走棋
			for (int i=0; i<4; i++)
				tmp [i] = gameBoard [y, i];
			Tool tool = Tool.getInstance ();
			//对比吃子状态的数组
			if (tool.judgeEqual (tmp, 0) || tool.judgeEqual (tmp, 1) || tool.judgeEqual (tmp, 2) || tool.judgeEqual (tmp, 3)) {
				blackCount--;
				isEat = true;
				CheckChessToClear (y, -1, 2);  //清除被吃的棋子
			}
			for (int i=0; i<4; i++)
				tmp [i] = gameBoard [i, x];
			if (tool.judgeEqual (tmp, 0) || tool.judgeEqual (tmp, 1) || tool.judgeEqual (tmp, 2) || tool.judgeEqual (tmp, 3)) {
				isEat = true;
				blackCount--;
				CheckChessToClear (-1, x, 2);
			}
			if (blackCount <= 1 || !Tool.getInstance().isAbleToMove(gameBoard,2))  //棋子数量小于2或者被围住无棋可走
			{
				gameOver();  //游戏结束
				return ;
			}
			else
				nextTurn ();  //轮到下一位
		} else {           //黑子走棋
			for (int i=0; i<4; i++)
				tmp [i] = gameBoard [y, i];
			Tool tool = Tool.getInstance ();
			if (tool.judgeEqual (tmp, 4) || tool.judgeEqual (tmp, 5) || tool.judgeEqual (tmp, 6) || tool.judgeEqual (tmp, 7)) {
				isEat = true;
				whiteCount--;
				CheckChessToClear (y, -1, 1);
			}
			for (int i=0; i<4; i++)
				tmp [i] = gameBoard [i, x];
			if (tool.judgeEqual (tmp, 4) || tool.judgeEqual (tmp, 5) || tool.judgeEqual (tmp, 6) || tool.judgeEqual (tmp, 7)) {
				isEat = true;
				whiteCount--;
				CheckChessToClear (-1, x, 1);
			}
			if (whiteCount <= 1 || !Tool.getInstance().isAbleToMove(gameBoard,1)) 
			{
				gameOver();
				return ;
			}
			else
				nextTurn ();
		}
		initTurnTitle();  //更新标题
		if(isEat){
			if(Tool.getInstance().effectVolume!=0&&!(Tool.getInstance().gameType==1&&state==GameState.WHITE))
			{
				AudioSource.PlayClipAtPoint(eatAudio,Vector3.zero,Tool.getInstance().effectVolume);
				print("bo");
			}
				
			aiEffect = 1;
		}else{
			if(Tool.getInstance().effectVolume!=0&&!(Tool.getInstance().gameType==1&&state==GameState.WHITE))
				AudioSource.PlayClipAtPoint(moveAudio,Vector3.zero,Tool.getInstance().effectVolume);
			aiEffect = 0;
		}
	}

	private void CheckChessToClear (int y, int x, int targetType)
	{
		if (x == -1) { //清除y轴棋子
			for (int i=0; i<4; i++) {
				if (gameBoard [y, i] == targetType) {
					gameBoard [y, i] = 0;
					GameObject.Destroy (chessManArray [y, i]);
					chessManArray [y, i] = null;
				}
			}
		} else {
			for (int i=0; i<4; i++) {
				if (gameBoard [i, x] == targetType) {
					gameBoard [i, x] = 0;
					GameObject.Destroy (chessManArray [i, x]);
					chessManArray [i, x] = null;
				}
			}
		}
	}

	private void nextTurn ()
	{
		if (state == GameState.WHITE)
		{
			state = GameState.BLACK;
			if(Tool.getInstance().gameType==1)
			{
				StartCoroutine(this.doAI());
			}
		}
		else 
			state = GameState.WHITE;
	}

	IEnumerator doAI()
	{
		yield return 0;
		if(Tool.getInstance().AILevel==0)
			yield return new WaitForSeconds(0.1f);
		Tool.getInstance().getBestScore(gameBoard,2,Tool.getInstance().AILevel);
		MoveDate md = Tool.getInstance().bestMove;
		isChecked=true;
		selectY = md.fromY;
		selectX = md.fromX;
		dealChessClick(md.toY,md.toX);
		yield return 0;
		if(Tool.getInstance().effectVolume!=0)
		{
			if(aiEffect==0){
				AudioSource.PlayClipAtPoint(moveAudio,Vector3.zero,Tool.getInstance().effectVolume);
			}else if(aiEffect==1){
				AudioSource.PlayClipAtPoint(eatAudio,Vector3.zero,Tool.getInstance().effectVolume);
			}else{
				AudioSource.PlayClipAtPoint(deadAudio,Vector3.zero,Tool.getInstance().effectVolume);
			}
		}

	}
	

//	public void onButtonClick()
//	{
//		MoveDate md = null;
//		if(state == GameState.WHITE)
//		{
//			md = Tool.getInstance().getBestMove(gameBoard,1);
//		}
//		else if(state == GameState.BLACK)
//		{
//			md = Tool.getInstance().getBestMove(gameBoard,2);
//			isChecked=true;
//			selectY = md.fromY;
//			selectX = md.fromX;
//			dealChessClick(md.toY,md.toX);
//		}
//		print("from :("+md.fromY+","+md.fromX+");");
//		print("To :("+md.toY+","+md.toX+");");
//	}

	private void gameOver(){
		gameOverPanel.SetActive(true);
		if(Tool.getInstance().gameType==0){
			if(state==GameState.WHITE)
				gameOverPanel.GetComponentInChildren<Text>().text = "白棋胜!";
			else
				gameOverPanel.GetComponentInChildren<Text>().text = "黑棋胜!";
		}
			else if (Tool.getInstance().gameType==1){
			if(state==GameState.WHITE)
				gameOverPanel.GetComponentInChildren<Text>().text = "你赢了!";
			else
				gameOverPanel.GetComponentInChildren<Text>().text = "你输了!";
		}
		state = GameState.OVER;
		if(Tool.getInstance().effectVolume!=0)
			AudioSource.PlayClipAtPoint(deadAudio,Vector3.zero,Tool.getInstance().effectVolume);
		aiEffect = 2;
	}

	public void renewGame(){

		GameObject []go = GameObject.FindGameObjectsWithTag("chess");
		foreach(GameObject obj in go)
		{
			Destroy(obj);
		}
		initGame ();
	}

	public void quitGame(){
		Application.LoadLevel(0);
	}

	public void takeBackMove(){
		if(state!=GameState.OVER){
			if(Tool.getInstance().gameType==1){
				if(chessDataList.Count<=2)
					renewGame();
				else{
					chessDataList.Pop();
					chessDataList.Pop();
					initWithChessData(chessDataList.Peek());
				}
			}
			else{
				if(chessDataList.Count>1)
				{
					chessDataList.Pop();
					initWithChessData(chessDataList.Peek());
				}
				else
					renewGame();
			}
		}
	}
}
