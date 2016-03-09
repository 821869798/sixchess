using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MoveDate  //走棋数据
{
	public int fromX;  //原来坐标的X
	public int fromY;  //原来坐标的Y
	public int toX;    //走到的坐标X
	public int toY;    //走到的坐标Y
	public int score;
	public MoveDate ()
	{

	}

	public MoveDate (int fromY, int fromX, int toY, int toX)
	{
		this.fromY = fromY;
		this.fromX = fromX;
		this.toY = toY;
		this.toX = toX;
		this.score = 0;
	}
}

public class ChessData
{
	public int [,] gameBoard;  //保存棋盘
	public int checkedY;      //保存选择提示框的Y
	public int checkedX;      //保存选择提示框的X
	public GameState state;   //游戏状态
	public int whiteCount,blackCount;  //黑白棋的数量
	public ChessData(){
	}
	public ChessData(int [,]array,int y,int x,GameState state,int wc,int bc){
		gameBoard = (int [,])array.Clone();
		checkedY = y;
		checkedX = x;
		this.state = state;
		whiteCount = wc;
		blackCount = bc;
	}
}

public class Tool
{
	public const int victory = 1000;
	public const int eatChess = 10;
	public const int eatDoubleChess = 20;
	public int boardCellWidth = 142;
	public int boardCellWidthHalf = 71;
	public int AILevel = 1;
	public int gameType = 0; //0表示双人游戏，1表示人机对战
	public float effectVolume = 1;  //音效的音量
	public MoveDate bestMove = null; //最好的步子
	//public float bgmVolume = 1;
	bool flag = false;
	private int[,]judge = new int[,] {
		{1,1,2,0},
		{0,1,1,2},
		{2,1,1,0},
		{0,2,1,1},
		{2,2,1,0},
		{0,2,2,1},
		{1,2,2,0},
		{0,1,2,2}
	};
	private static Tool instance = null;

	public  static Tool getInstance ()
	{
		if (instance == null) {
			instance = new Tool ();
		}
		return instance;
	}

	public bool judgeEqual (int[]src, int index)
	{
		for (int i=0; i<4; i++) {
			if (src [i] != judge [index, i])
				return false;
		}
		return true;
	}

	public bool isChessEnough(int [,]gameBoard,int type)
	{
		int count = 0 ;
		for(int i=0;i<4;i++)
		{
			for(int j=0;j<4;j++)
			{
				if(gameBoard[i,j]==type)
				{
					count++;
				}
			}
		}
		if(count<=1)
			return false;
		else
			return true;
	}

	public bool isAbleToMove (int[,] gameBoard, int type)
	{
		for (int i=0; i<4; i++) {
			for (int j=0; j<4; j++) {
				if (gameBoard [i, j] == type && ischessAbleMove (gameBoard, i, j)) {
					return true;
				}
			}
		}
		return false;
	}

	private bool ischessAbleMove (int[,]gameBoard, int y, int x)
	{
		if (y - 1 >= 0 && gameBoard [y - 1, x] == 0)
			return true;
		if (y + 1 < 4 && gameBoard [y + 1, x] == 0)
			return true;
		if (x - 1 >= 0 && gameBoard [y, x - 1] == 0)
			return true;
		if (x + 1 < 4 && gameBoard [y, x + 1] == 0)
			return true;
		return false;
	}

	private List<MoveDate> getAbleMoveDataList (int[,]gameBoard, int type)
	{
		List<MoveDate> moveList = new List<MoveDate> ();
		for (int y=0; y<4; y++) {
			for (int x=0; x<4; x++) {
				if (gameBoard [y, x] != type)
					continue;
				if (x - 1 >= 0 && gameBoard [y, x - 1] == 0) {  //棋子是否可以往左走
					MoveDate tp = new MoveDate (y, x, y, x - 1);
					moveList.Add (tp);
				}
				if (x + 1 < 4 && gameBoard [y, x + 1] == 0) {   //往右走
					MoveDate tp = new MoveDate (y, x, y, x + 1);
					moveList.Add (tp);
				}
				if (y - 1 >= 0 && gameBoard [y - 1, x] == 0) {   //往下走
					MoveDate tp = new MoveDate (y, x, y - 1, x);
					moveList.Add (tp);
				}
				if (y + 1 < 4 && gameBoard [y + 1, x] == 0) {    //往上走
					MoveDate tp = new MoveDate (y, x, y + 1, x);
					moveList.Add (tp);
				}
			}
		}
		return moveList;
	}

	
	public int getBestScore(int [,]gameBoard,int type,int level)  //参数分别对应棋盘数组，谁走棋，博弈树的深度*2，通俗的可以理解为AI的等级
	{
		int bestScore = -1000000;  //初始分数
		int bestIndex = -1;       //初始下标
		int enemy = 1;         //用户转换敌人
		if(type==1)
			enemy = 2;
		List<MoveDate> moveList1 = getAbleMoveDataList (gameBoard, type);  //获取所有能走的步法
		do{
			if(moveList1.Count==0)    //没有则无棋可走，直接输了
			{
				bestScore=-victory;
				break;
			}
			for(int i=0;i<moveList1.Count;i++)
			{
				MoveDate md1 = moveList1[i];
				int [,] tmpGame1 = (int [,])gameBoard.Clone();
				int ownScore = tryMoveChess(tmpGame1,md1,type);   //对AI的步法进行基础评分
				List<MoveDate> moveList2 = getAbleMoveDataList (tmpGame1, enemy);  //获取对手所有可走的步法
				if(moveList2.Count==0)   //对手无棋可走，直接获胜
				{
					bestScore=victory;
					moveList1[i].score=bestScore;
					bestIndex=i;
					break;
				}
				int bestEnemyScore = -1000000;  //对手的初始分
				int bestEnemyIndex = -1;
				for(int j=0;j<moveList2.Count;j++)  //寻找对手最优的得分，即对自己最不利的步法
				{
					MoveDate md2 = moveList2[j];
					int [,]tmpGame2 = (int [,])tmpGame1.Clone();
					int enemyScore =  tryMoveChess(tmpGame2,md2,enemy);
					if(enemyScore>=victory)
					{
						bestEnemyScore = victory;
						break;
					}
					if(level>0)   //递归获取更深节点博弈树的分数
					{
						int tmpBestScore = getBestScore(tmpGame2,type,level-1);
						enemyScore -= tmpBestScore;
					}
					if(enemyScore>bestEnemyScore)
					{
						bestEnemyScore = enemyScore;
						bestEnemyIndex = j;
					}
				}
				moveList1[i].score = ownScore-bestEnemyScore;
				if(bestEnemyScore>=victory)
				{
					continue;
				}
				else if(ownScore-bestEnemyScore>bestScore)
				{
					bestScore = ownScore-bestEnemyScore;
					bestIndex = i;
				}
				
			}	
		}while(false);
		if(level==AILevel){
			if(bestIndex!=-1)
				bestMove=getRandBestMove(moveList1,bestIndex);
			else
				bestMove=moveList1[0];
		}
		return bestScore-1;
	}

	private MoveDate getRandBestMove(List<MoveDate> moveList,int bestIndex){
		List<int> bestData = new List<int>();
		for(int i=0;i<moveList.Count;i++)
		{
			if(moveList[i].score==moveList[bestIndex].score)
				bestData.Add(i);
		}
		return moveList[bestData[UnityEngine.Random.Range(0,bestData.Count)]];
	}


	private int tryMoveChess (int[,]gameBoard,MoveDate md,int type)
	{
		int enemy = 1;
		if(type==1)
			enemy = 2;
		int score = 0;
		gameBoard [md.fromY, md.fromX] = 0;
		gameBoard [md.toY, md.toX] = type;
		int [] tmp = new int[4];
		int judgeIndex = 0;
		if(type==2)
		{
			judgeIndex = 4;
		}
		int eat = 0;
		for (int i=0; i<4; i++)
			tmp [i] = gameBoard [md.toY, i];
		if (judgeEqual (tmp, judgeIndex+0) || judgeEqual (tmp, judgeIndex+1) || judgeEqual (tmp, judgeIndex+2) || judgeEqual (tmp, judgeIndex+3)) {
			eat++;
			for(int i=0;i<4;i++)
				if(gameBoard[md.toY,i]==enemy)
					gameBoard[md.toY,i]=0;
		}
		for (int i=0; i<4; i++)
			tmp [i] = gameBoard [i, md.toX];
		if (judgeEqual (tmp, judgeIndex+0) || judgeEqual (tmp, judgeIndex+1) || judgeEqual (tmp, judgeIndex+2) || judgeEqual (tmp, judgeIndex+3)) {
			eat++;
			for(int i=0;i<4;i++)
				if(gameBoard[i, md.toX]==enemy)
					gameBoard[i, md.toX]=0;
		}
		if(eat==2)
			score += eatDoubleChess;
		else if(eat==1)
			score += eatChess;
		if(!isAbleToMove(gameBoard,enemy))
		{
			score += victory;
		}
		if(!isChessEnough(gameBoard,enemy))
		{
			score += victory;
		}
		return score;
	}
}
