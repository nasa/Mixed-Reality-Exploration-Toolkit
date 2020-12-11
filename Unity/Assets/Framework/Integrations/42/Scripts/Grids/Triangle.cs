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