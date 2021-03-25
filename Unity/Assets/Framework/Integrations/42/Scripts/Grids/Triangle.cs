// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;

namespace TestTest {
	public class Triangle {
		public List<Vertex> vertices;

		public Triangle(List<Vertex> vertices){
			this.vertices = vertices;

			foreach (var vertex in vertices){
				vertex.triangles.Add(this);
			}
		}
	}
}