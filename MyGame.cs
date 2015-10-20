using System.Collections.Generic;
using System.Collections.ObjectModel;

class MyGame
{
	private static readonly MyGame instance = new MyGame();
	public static MyGame Instance { get { return instance; } }

	private MyGame()
	{
		gameState = GameState.Global;
	}

	//public Collection<RPoint> rPoints = new Collection<RPoint>();
	//public Queue<RMove> rMoves = new Queue<RMove>();

	public enum GameState { Global, Local, Dialog };
	public GameState gameState;
}
