using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ManagerScript : MonoBehaviour 
{

	public int numberOfPoints;
	public float radius;

	public float[][] points; //array containing arrays of points
	public Vector3[] vectorPoints; //vectorization of the points

	public List<Vector3[]> pairs; //list of 2-d vector arrays of the pairs
	public List<Vector3[]> triangles; //list of 2-d vector arrays of the triangles

	public GameObject PointPrefab; //The agent prefab
	public Material LineMaterial;
	public Material TriangleMaterial;

	public GameObject PointButton;
	public GameObject LinesButton;
	public GameObject TrianglesButton;
	public GameObject ResetButton;

	public Slider PointSlider; //GUI Slider
	public Slider RadiusSlider; //GUI Slider

	public Text PointsText;
	public Text RadiusText;

	void Start()
	{

		Button pbtn = PointButton.GetComponent<Button> ();
		pbtn.onClick.AddListener (GenerateData);

		Button lbtn = LinesButton.GetComponent<Button> ();
		lbtn.onClick.AddListener (FindPairs);

		Button tbtn = TrianglesButton.GetComponent<Button> ();
		tbtn.onClick.AddListener (FindTriangles);

		Button rbtn = ResetButton.GetComponent<Button> ();
		rbtn.onClick.AddListener (Reset);

		GenerateData ();

	}

	void Update()
	{

		PointsText.text = ((int)(250 * PointSlider.value)).ToString();
		RadiusText.text = (10 * RadiusSlider.value).ToString("#.##");

	}

	//Generate random data points
	//Each point is a tuple containing a x and y value, stored as an array
	//Store the points in the
	void GenerateData()
	{

		float val = 250 * PointSlider.value;
		numberOfPoints = (int)val;

		//declare jagged array with number of elements being equal to desired numberOfPoints
		points = new float[numberOfPoints][];

		for (int i = 0; i < numberOfPoints; i++) 
		{

			//Initialize the elements of the array
			points[i] = new float[3];

			//Assign random value for the x and y values in the array
			points [i] [0] = Random.Range (-20f, 20f);
			points [i] [1] = Random.Range (-20f, 20f);
			points [i] [2] = Random.Range (-20f, 20f);

			//Debug.Log("Point " + i + " is located at " + points[i][0] + "/" + points[i][1]);
		}

		VisualizePoints ();
	}


	//Visualize the data points
	//Vectorize points
	void VisualizePoints()
	{

		GameObject[] ls = GameObject.FindGameObjectsWithTag("Line");
		foreach(GameObject li in ls)
			GameObject.Destroy(li);

		GameObject[] dataP = GameObject.FindGameObjectsWithTag("Point");
		foreach(GameObject dp in dataP)
			GameObject.Destroy(dp);

		GameObject[] tr = GameObject.FindGameObjectsWithTag("Triangle");
		foreach(GameObject t in tr)
			GameObject.Destroy(t);

		//Element 0 = vector points, element 1 = pairs, element 2 = triangles, element 4 = tetrahedrons
		vectorPoints = new Vector3[numberOfPoints];

		for (int i = 0; i < numberOfPoints; i++) 
		{

			Vector3 pointPos = new Vector3 (points[i][0], points[i][1], points[i][2]); //Vectorize the spawn position
			vectorPoints [i] = pointPos; //Store spawn vector in array
			Instantiate (PointPrefab, pointPos, Quaternion.identity);

			//Debug.Log(i + " is located at " + vectorPoints [0] [i]);
		}
	}

	//Iterate through every point and compare the distance to all other points
	//If the distance is smaller than the radius, create new set.
	//LATER - create the entire rips complex in the same method
	void FindPairs()
	{

		radius = 10 * RadiusSlider.value;

		//Initialize list
		pairs = new List<Vector3[]>();
		//triangles = new List<Vector3[]>();

		for (int i = 0; i < vectorPoints.Length; i++) 
		{
			//Debug.Log(i + " is located at " + vectorPoints [i]);

			for (int j = i+1; j < vectorPoints.Length; j++)
			{

				//gets the distance between a vector point
				float distance = Vector3.Distance (vectorPoints [i], vectorPoints [j] );
				//Debug.Log("Distance between " + i + " and " + j + " is " + distance);

				if (distance < radius) 
				{
					//stores the connected points as an array
					Vector3[] connected = new Vector3[] {vectorPoints [i], vectorPoints [j]};
					//Debug.Log("Distance between " + i + " and " + j + " is " + distance + " -- connection made");

					//adds the array to the list
					pairs.Add (connected);
				}
			}
		}

		Debug.Log("Connections: " + pairs.Count);
		DrawLines ();

	}

	void DrawLines()
	{

		GameObject[] ls = GameObject.FindGameObjectsWithTag("Line");
		foreach(GameObject li in ls)
			GameObject.Destroy(li);


		GameObject[] tr = GameObject.FindGameObjectsWithTag("Triangle");
		foreach(GameObject t in tr)
			GameObject.Destroy(t);


		//Create new game objects that draw the lines
		for (int i = 0; i < pairs.Count; i++) 
		{
			var line = new GameObject ("Line " + i);
			line.tag = "Line";
			line.AddComponent<LineRenderer>();
			LineRenderer lineR = line.GetComponent<LineRenderer> ();

			lineR.sortingLayerName = "OnTop";
			lineR.sortingOrder = 5;
			lineR.positionCount = 2;
			lineR.widthMultiplier = 0.5f;
			lineR.useWorldSpace = true;
			lineR.startColor = Color.white;
			lineR.material = LineMaterial;

			lineR.SetPosition(0, pairs[i][0]);
			lineR.SetPosition(1, pairs[i][1]);
		}

	}


	void FindTriangles()
	{

		triangles = new List<Vector3[]>();

		for (int i = 0; i < vectorPoints.Length; i++) 
		{
			for (int j = i+1; j < vectorPoints.Length; j++)
			{
				float distance = Vector3.Distance (vectorPoints [i], vectorPoints [j] );

				if (distance < radius) 
				{
					//Find triangles
					for (int k = i+1; k < vectorPoints.Length; k++) 
					{
						if (Vector3.Distance (vectorPoints [i], vectorPoints [k]) < radius && Vector3.Distance (vectorPoints [j], vectorPoints [k]) < radius) {

							if (vectorPoints [j] != vectorPoints [k]) {

								if (triangles.Count == 0) 
								{
									Debug.Log ("Triangle between " + vectorPoints [i] + " and " + vectorPoints [k] + " and " + vectorPoints [j]);
									Vector3[] connection = new Vector3[] { vectorPoints [i], vectorPoints [j], vectorPoints [k] };
									triangles.Add (connection);
								}

								//if the second element of of the new triangle set is not equal to the third element of the previous set
								else if(vectorPoints [j] != triangles[triangles.Count-1][2])
								{
									Debug.Log ("Triangle between " + vectorPoints [i] + " and " + vectorPoints [k] + " and " + vectorPoints [j]);
									Vector3[] connection = new Vector3[] { vectorPoints [i], vectorPoints [j], vectorPoints [k] };
									triangles.Add (connection);
								}

								//LATER IMPROVEMENT -- Makes a new triangle connection ONLY IF ALL comparisons between elements are made, that is compare second element to all third elements
								//				else
								//				{
								//					for (int l = 0; l < triangles.Count; l++) 

								//					{
								//						if (vectorPoints [j] != triangles[l][2])
								//						{
								//						Debug.Log ("Triangle between " + vectorPoints [i] + " and " + vectorPoints [k] + " and " + vectorPoints [j]);
								//						Vector3[] connection = new Vector3[] { vectorPoints [i], vectorPoints [j], vectorPoints [k] };
								//						triangles.Add (connection);
								//						}

								//					}
								//				}

							}
						}
					}
				}
			}
		}

		//Debug.Log("Connections: " + pairs.Count);
		Debug.Log("Triangles: " + triangles.Count);
		if (triangles.Count > 0) {
			DrawTriangles ();
		}

	}

	void DrawTriangles()
	{

		GameObject[] tr = GameObject.FindGameObjectsWithTag("Triangle");
		foreach(GameObject t in tr)
			GameObject.Destroy(t);

		//Create new game objects that draw the lines
		for (int i = 0; i < triangles.Count; i++) 
		{
			var tri = new GameObject ("Triangle " + i);
			tri.tag = "Triangle";
			tri.AddComponent<MeshFilter>();
			tri.AddComponent<MeshRenderer>();
			Mesh triM = tri.GetComponent<MeshFilter>().mesh;
			MeshRenderer triR = tri.GetComponent<MeshRenderer> ();
			triM.Clear();
			triR.material = TriangleMaterial;
			Color newColor = new Color (0.3f, 0.5f, 1f, 1f);
			triR.material.color = newColor;

			triM.vertices = triangles [i];
			triM.uv = new Vector2[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)};
			triM.triangles = new int[] {0, 1, 2};
		}
	}


	void Reset()
	{

		SceneManager.LoadScene("MainScene");

		GameObject[] dataP = GameObject.FindGameObjectsWithTag("Point");
		foreach(GameObject dp in dataP)
			GameObject.Destroy(dp);

		GameObject[] ls = GameObject.FindGameObjectsWithTag("Line");
		foreach(GameObject li in ls)
			GameObject.Destroy(li);

		points = null; //array containing arrays of points
		vectorPoints = null; //vectorization of the points
		pairs = null; //list of 2-d vector arrays of the pairs

		Start ();
		//SceneManager.LoadScene("MainScene");

	}
}
