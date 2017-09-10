using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class Pathfinding : MonoBehaviour {
	public GameObject camino,pared;
	StreamWriter file;
	public GameObject[,] road;
	public Node[,] grid;

	public int[] startPos;
	public int[] endpos;
	GameObject startGO,endGO;

	Transform  maze;
	void Start(){
		maze =  GameObject.Find("maze").transform;
	}
	void Update(){

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit = new RaycastHit();
		if(Physics.Raycast(ray,out hit) && Input.GetButtonDown("Fire1")){
			if(!hit.transform.CompareTag("wall")){
				hit.transform.GetComponent<Renderer>().material.color = Color.blue;					
				print(hit.transform.position);
				startPos[0] = (int) hit.transform.position.x;
				startPos[1] = (int) hit.transform.position.y;
				if(startGO != null)
					startGO.transform.GetComponent<Renderer>().material.color = Color.white;	
				startGO = hit.transform.gameObject;
			}
		}else if(Physics.Raycast(ray,out hit) && Input.GetButtonDown("Fire2")){
			if(!hit.transform.CompareTag("wall")){
				hit.transform.GetComponent<Renderer>().material.color = Color.green;					
				endpos[0] = (int) hit.transform.position.x;
				endpos[1] = (int) hit.transform.position.y;
				if(endGO != null)
					endGO.transform.GetComponent<Renderer>().material.color = Color.white;	
				endGO = hit.transform.gameObject;
			}
		}
	}
	
	public void GenerateMap(string _fileName){
		foreach(Transform child in maze.transform){
			Destroy(child.gameObject);			
		}
		CreateMap(Application.dataPath+@"\"+_fileName);

	}
	public void GenerateMapCustom(InputField _fileName){
		foreach(Transform child in maze.transform){
			Destroy(child.gameObject);			
		}
		if(string.IsNullOrEmpty(_fileName.text))
			return;
		CreateMap(Application.dataPath+@"\"+_fileName.text);

	}
	public void FixCamera(int type){
		if(type == 1){
			Camera.main.transform.position = new Vector3(28.4f,32.2f,-10);
			Camera.main.orthographicSize = 41.1f;
		}else{
			Camera.main.transform.position = new Vector3(13.8f,15.67f,-10);
			Camera.main.orthographicSize = 17.28f;			
		}
	}

	


	public void CreateMap(string _path){

		string[] lines = System.IO.File.ReadAllLines(_path);

		int mY =0;
		
		road = new GameObject[lines.Length,lines[5].Length];
		grid = new Node[lines.Length,lines[5].Length];

		print("matriz de: "+lines.Length +"X" +lines[5].Length);
		for (int x = 0; x < lines.Length; x++){
			mY = 0;
			for(int y = 0; y < lines[x].Length;y++){
				mY++;
				if (lines[x][y] == '\t') {
					
					GameObject go =  Instantiate(pared,new Vector3( x,mY ,0),Quaternion.identity) as GameObject;
					go.transform.SetParent(maze);
					road[x,mY] = go;
					grid[x,mY] = new Node(x,mY,false);
				}
				if (lines[x][y] == 'F') {
					mY--;
					GameObject go = Instantiate(camino,new Vector3( x,mY,0),Quaternion.identity) as GameObject;
					go.transform.SetParent(maze);
					if(road[x,mY] != null){
						grid[x,mY] = null;
						Destroy(road[x,mY].gameObject);
					}
					road[x,mY] = go;
					grid[x,mY] = new Node(x,mY,true);
				}
			}
		}
	}

	public void FindPath(){
		if(maze.childCount == 0 || startGO == null || endGO == null)
			return;
			
		Node startNode = null;
		Node endNode = null;

		foreach(var node in grid){
			
			if(node != null){
				if(node.x == startPos[0] && node.y == startPos[1]){
					startNode = node;
					road[node.x, node.y].GetComponent<Renderer>().material.color = Color.red;
				}
				if(node.x == endpos[0] && node.y == endpos[1]){
					endNode = node;
					road[node.x, node.y].GetComponent<Renderer>().material.color = Color.green;					
				}
			}
		}
		
		
		List<Node> openNodes = new List<Node>();
		List<Node> closedNodes = new List<Node>();

		openNodes.Add(startNode);

		while(openNodes.Count > 0){
			int min = openNodes.Min(i => i.f_cost);
			Node current =  openNodes.First(i => i.f_cost == min);

			openNodes.Remove(current);
			closedNodes.Add(current);

			if(current == endNode){
				print("prey Found!");
				StartCoroutine(RetracePath(startNode,endNode));
				return;
			}

			foreach(var neighbour in GetNeighbours(current)){
				
				if(!neighbour.walkable || closedNodes.Contains(neighbour))
					continue;
				
				int newCostToNeighbour = current.g_cost + GetDistance(current, neighbour);
				if (newCostToNeighbour < neighbour.g_cost || !openNodes.Contains(neighbour)) {
					neighbour.g_cost = newCostToNeighbour;
					neighbour.h_cost = GetDistance(neighbour, endNode);
					neighbour.parent = current;

					if (!openNodes.Contains(neighbour))
						openNodes.Add(neighbour);
				}
			}
		}
	}

	public List<Node> GetNeighbours(Node _node){
		List<Node> neighbours  = new List<Node>();

		foreach(var node in grid){
			
			if(node != null){
				
				int xCNode = _node.x;
				int yCNode = _node.y;

				//left
				if((node.x == xCNode && node.y == yCNode-1) && node.walkable){
					neighbours.Add(node);
				}else if((node.x == xCNode && node.y == yCNode+1) && node.walkable){ // right
					neighbours.Add(node);
				}else if((node.x == xCNode -1 && node.y == yCNode) && node.walkable){ //up
					neighbours.Add(node);
				}else if((node.x == xCNode +1 && node.y == yCNode) && node.walkable){ //down
					neighbours.Add(node);
				}else if((node.x == xCNode -1 && node.y == yCNode -1) && node.walkable){ //up-left
					neighbours.Add(node);
				}else if((node.x == xCNode -1 && node.y == yCNode +1) && node.walkable){ //up-right
					neighbours.Add(node);
				}else if((node.x == xCNode +1 && node.y == yCNode -1) && node.walkable){ //down-left
					neighbours.Add(node);
				}else if((node.x == xCNode +1 && node.y == yCNode +1) && node.walkable){ //down-right
					neighbours.Add(node);
				}
			}
		}
		
		return neighbours;
	}

	public int GetDistance(Node nodeA, Node nodeB){
		int dstX = Mathf.Abs(nodeA.x - nodeB.x);
		int dstY = Mathf.Abs(nodeA.y - nodeB.y);

		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}

	IEnumerator RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();

		string pathResult = "";
		foreach(var node in path){
			road[node.x, node.y].GetComponent<Renderer>().material.color = Color.red;
			pathResult +="["+node.x+","+node.y+"],"; 
			yield return new WaitForSeconds(0.05f);
		}
		pathResult = pathResult.Remove(pathResult.Length -1);
		SaveResult(pathResult);
	}
	public void SaveResult (string result) {
		file = new System.IO.StreamWriter(Application.dataPath.Replace("Hound Maze_Data","pathResult.txt"),false);
		file.WriteLine(result);
		file.Close ();
	}
	public void Exit(){
		Application.Quit();
	}
}

public class Node{
	public int x;
	public int y;

	public bool walkable = false;

	public int g_cost = 0;
	public int h_cost = 0;
	public int f_cost {
		get{
			return g_cost + h_cost;
		}
	}

	public Node parent;

	public Node(int _x, int _y, bool _walkable){
		x = _x;
		y = _y;
		walkable = _walkable;
	}

}

