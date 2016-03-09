using UnityEngine;
using System.Collections;

public class BoardCell : MonoBehaviour
{

	public int y;
	public int x;

	public void setPosition(int y,int x) //设置新的坐标
	{
	this.y = y;
		this.x = x;
		Tool tool = Tool.getInstance();
		this.transform.localPosition = new Vector3(tool.boardCellWidth*(x-2)+tool.boardCellWidthHalf,tool.boardCellWidth*(y-2)+tool.boardCellWidthHalf,0);
	}

	public void onBoardButtonClick () //棋子点击事件
	{
		//print(this.x+" "+this.y);
		GameController.instance.dealChessClick(y,x);
	}
}
