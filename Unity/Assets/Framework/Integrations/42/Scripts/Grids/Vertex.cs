// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace TestTest {
	public class Vertex {
		public List<Vertex> similarVertices;
		public Vector3 position;
		public int index;
		public List<Edge> edges;
		public List<Triangle> triangles;
		public List<Face> faces;

		public Vertex(Vector3 position, int index){
			this.position = position;
			this.index = index;
			edges = new List<Edge>();
			triangles = new List<Triangle>();
			faces = new List<Face>();
			similarVertices = new List<Vertex>();
		}
	}
}