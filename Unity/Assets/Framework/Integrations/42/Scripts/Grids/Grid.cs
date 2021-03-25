// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TestTest {
	public class Grid : MonoBehaviour {
		
		public List<Face> faces;
		public List<Triangle> triangles;
		public List<Edge> edges;
		public List<Vertex> vertices;
        public Color color;

		public float radius = 1f;
		public int nbLong = 24;
		public int nbLat = 16;
        public int choice = 0;

		public const float Pi = Mathf.PI;
		public const float TwoPi = Mathf.PI * 2f;

		protected void generateVertices() {
			var vertexIndex = 0;
			vertices.Add(new Vertex(Vector3.up * radius, vertexIndex++));

			for (var lat = 0; lat < nbLat; lat++) {
				var a1 = Pi * (lat + 1) / (nbLat + 1);
				var sin1 = Mathf.Sin(a1);
				var cos1 = Mathf.Cos(a1);

				for (var lon = 0; lon <= nbLong; lon++) {
					var a2 = TwoPi * (lon == nbLong ? 0 : lon) / nbLong;
					var sin2 = Mathf.Sin(a2);
					var cos2 = Mathf.Cos(a2);
					vertices.Add(new Vertex(new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius, vertexIndex++));
				}
			}
			vertices.Add(new Vertex(Vector3.up * -radius, vertexIndex++));
			
			foreach (var vertex in vertices){
				vertex.similarVertices = findRelatedVertices(vertex);
			}
		}
		
		protected virtual void generateEdges() {
			foreach (var face in faces) {
				foreach (var triangle in face.triangles) {
					for (var vertexIndex = 0; vertexIndex < triangle.vertices.Count; vertexIndex++) {
						var edge = new Edge(triangle.vertices[vertexIndex], triangle.vertices[vertexIndex + 1 < triangle.vertices.Count ? vertexIndex + 1 : 0]);
						edges.Add(edge);
						face.edges.Add(edge);
					}
				}
			}
		
			foreach (var face in faces) {
				face.cleanEdges();
			}

			foreach (var edge in edges) {
				edge.similarEdges = findRelatedEdges(edge);
			}
		}
		
		protected void generateTriangles() {
			// Top Cap
			var topCapTriangles = new List<Triangle>();

			for (var lon = 0; lon < nbLong; lon++) {
				var topCapTriangle = new Triangle(new List<Vertex> { vertices[lon + 2], vertices[lon + 1], vertices[0] });
				triangles.Add(topCapTriangle);
				topCapTriangles.Add(topCapTriangle);
			}
			
			faces.Add(new Face(topCapTriangles, this));

			//Middle
			for (var lat = 0; lat < nbLat - 1; lat++) {
				for (var lon = 0; lon < nbLong; lon++) {
					var current = lon + lat * (nbLong + 1) + 1;
					var next = current + nbLong + 1;

					var sideTriangleOne = new Triangle(new List<Vertex> { vertices[current], vertices[current + 1], vertices[next + 1] });
					triangles.Add(sideTriangleOne);
					
					var sideTriangleTwo = new Triangle(new List<Vertex> { vertices[current], vertices[next + 1], vertices[next] });
					triangles.Add(sideTriangleTwo);
					faces.Add(new Face(new List<Triangle> { sideTriangleOne, sideTriangleTwo }, this));
				}
			}

			//Bottom Cap
			var bottomCapTriangles = new List<Triangle>();

			for (var lon = 0; lon < nbLong; lon++) {
				var bottomCapTriangle = new Triangle(new List<Vertex> { vertices[vertices.Count - 1], vertices[vertices.Count - (lon + 2) - 1], vertices[vertices.Count - (lon + 1) - 1] });
				triangles.Add(bottomCapTriangle);
				bottomCapTriangles.Add(bottomCapTriangle);
			}
			
			faces.Add(new Face(bottomCapTriangles, this));
		}
		
		protected List<Vertex> findRelatedVertices(Vertex vertex) {
			var relatedVertices = new List<Vertex>();

			foreach (var compareToVertex in vertices) {
				if (Vector3.Distance(vertex.position, compareToVertex.position) < 0.0001f) {
					relatedVertices.Add(compareToVertex);
				}
			}

			return relatedVertices;
		}
		
		protected List<Edge> findRelatedEdges(Edge edge) {
			var relatedEdges = new List<Edge>();
		
			foreach (var compareToEdge in edges) {
				if (edge == compareToEdge) continue;
				if (edge.center == compareToEdge.center) {
					relatedEdges.Add(compareToEdge);
				}
			}

			return relatedEdges;
		}
		
		public List<Edge> findUniqueEdges(){
			var uniqueEdges = new List<Edge>();

			var usedEdges = new List<Edge>();

			foreach (var edge in edges){
				if(usedEdges.Contains(edge)) continue;
			
				uniqueEdges.Add(edge);
				usedEdges.Add(edge);

				foreach (var similarEdge in edge.similarEdges){
					usedEdges.Add(similarEdge);
				}
			}

			return uniqueEdges;
		}
		
		public List<GameObject> editModeEdges;
		public GameObject edgePrefab;
		
		public void generateEditModeEdges(int choice){
			var uniqueEdges = findUniqueEdges();
		
			editModeEdges = new List<GameObject>();

			foreach (var uniqueEdge in uniqueEdges) {
                var edgeObj = Instantiate(edgePrefab, transform);

                if (choice == 0)
                    edgeObj.GetComponent<LineRenderer>().material.color = color; //colors[0]; //new Color(245f, 28f, 28f);
                if (choice == 1)
                    edgeObj.GetComponent<LineRenderer>().material.color = color; //colors[1]; //new Color(245f, 19f, 230f);
                if (choice == 2)
                    edgeObj.GetComponent<LineRenderer>().material.color = color;  //colors[2]; //new Color(171f, 119f, 253f);
                if (choice == 3)
                    edgeObj.GetComponent<LineRenderer>().material.color = color; //colors[3]; //new Color(7f, 238f, 32f);
                if (choice == 4)
                    edgeObj.GetComponent<LineRenderer>().material.color = color; //colors[4]; //Color.blue;
                edgeObj.transform.localPosition = uniqueEdge.center;
				edgeObj.transform.localScale = new Vector3(.02f, .02f, .02f);
				edgeObj.GetComponent<LineRenderer>().startWidth = 0.01f;
				edgeObj.GetComponent<LineRenderer>().endWidth = 0.01f;
                edgeObj.GetComponent<LineRenderer>().SetPosition(0, uniqueEdge.startVertex.position);
				edgeObj.GetComponent<LineRenderer>().SetPosition(1, uniqueEdge.endVertex.position);
			
				uniqueEdge.edgeSphereGameObject = edgeObj;
				
				edgeObj.name = "Edge";
				editModeEdges.Add(edgeObj);
			}
		}

		public void Start() {
			
			faces = new List<Face>();
			triangles = new List<Triangle>();
			edges = new List<Edge>();
			vertices = new List<Vertex>();

			generateVertices();
			generateTriangles();
			generateEdges();
            generateEditModeEdges(choice);
		}
	}
}
