// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.FortyTwo.Test
{
    public class Face
    {
        public List<Triangle> triangles;
        public List<Vertex> uniqueVertices;
        public List<Vertex> allVertices;
        public List<Edge> edges;
        public Grid grid;

        // Only for pie
        public Vertex pieCenterVertex;

        public Face(List<Triangle> triangles, Grid grid)
        {
            this.triangles = triangles;
            this.grid = grid;

            edges = new List<Edge>();
            calculateUniqueVertices();
        }

        private void calculateUniqueVertices()
        {
            uniqueVertices = new List<Vertex>();
            allVertices = new List<Vertex>();
            var usedVertices = new List<Vertex>();

            foreach (var triangle in triangles)
            {
                foreach (var vertex in triangle.vertices)
                {
                    allVertices.Add(vertex);
                    vertex.faces.Add(this);

                    if (usedVertices.Contains(vertex)) continue;

                    uniqueVertices.Add(vertex);

                    usedVertices.Add(vertex);
                    foreach (var similarVertex in vertex.similarVertices)
                    {
                        usedVertices.Add(similarVertex);
                    }
                }
            }
        }

        public void cleanEdges()
        {
            var edgesToRemove = new List<Edge>();
            foreach (var edge in edges)
            {
                foreach (var edge2 in edges)
                {
                    if (edge == edge2) continue;
                    if (edgesToRemove.Contains(edge2)) continue;
                    if (edge.center != edge2.center) continue;
                    //if (!edge.similarEdges.Contains(edge2)) continue;
                    edgesToRemove.Add(edge2);
                    edgesToRemove.Add(edge);
                }
            }

            foreach (var edge in edgesToRemove)
            {
                grid.edges.Remove(edge);
                edges.Remove(edge);
            }
        }
    }
}