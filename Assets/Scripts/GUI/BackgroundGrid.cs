using System.Collections;
using UnityEngine;

public class BackgroundGrid : MonoBehaviour 
{
	public Material mat;

	private Camera _myCamera;

	private Vector3 startVertex;
	private Vector3 mousePos;

	// Use this for initialization
	void Start () {
		_myCamera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
		mousePos = Input.mousePosition;
		if( Input.GetKeyDown(KeyCode.Space))
		{
			startVertex = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);
		}
	}

	void OnPostRender() {
		if(!mat)
		{
			Debug.LogError("Please assign a material on the inspector");
			return;
		}

//		float orthoSize = _myCamera.

		GL.PushMatrix();
		mat.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.LINES);
		GL.Color(Color.red);
		GL.Vertex(startVertex);
		GL.Vertex(new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0));
		GL.End();
		GL.PopMatrix();

		Debug.LogFormat("MousePos {0}, {1}", mousePos.x, mousePos.y);
	}
}
