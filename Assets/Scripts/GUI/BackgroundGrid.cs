using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

// This component should be attached to a 2D camera GameObject

public class BackgroundGrid : MonoBehaviour 
{

	private VectorLine _vectorLine;	// The actual grid lines
	private List<Vector2> _vectorLinePoints;

	// Use this for initialization
	void Start () 
	{
		_vectorLinePoints = new List<Vector2>();
		_vectorLine = new VectorLine("Grid", _vectorLinePoints, 2.0f);

		BuildNewVectorLinePoints();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void OnPostRender()
	{
		
	}

	void BuildNewVectorLinePoints()
	{

	}
}
