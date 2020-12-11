using System.Collections.Generic;
using UnityEngine;

namespace TestTest {
	public class Edge {
		public Vertex startVertex, endVertex;
		public Vector3 center;
		public List<Edge> similarEdges;
		public GameObject edgeSphereGameObject;

		public List<Face> faces = new List<Face>();

		public Edge(Vertex startVertex, Vertex endVertex){
			this.startVertex = startVertex;
			this.endVertex = endVertex;
			center = calculateCenter();
			startVertex.edges.Add(this);
			endVertex.edges.Add(this);

			foreach (var startVertexSimilarVertex in startVertex.similarVertices) {
				startVertexSimilarVertex.edges.Add(this);
			}

			foreach (var endVertexSimilarVertex in endVertex.similarVertices) {
				endVertexSimilarVertex.edges.Add(this);
			}
		}

		private Vector3 calculateCenter(){
			return (startVertex.position + endVertex.position) / 2;
		}

	
	}
}