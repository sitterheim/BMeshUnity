/*
 * Copyright (c) 2020 -- Élie Michel <elie@exppad.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Collections.Generic;
using UnityEngine;

/**
 * Non-manifold boundary representation of a 3D mesh with arbitrary attributes.
 * This structure intends to make procedural mesh creation and arbitrary edits
 * as easy as possible while remaining efficient enough. See other comments
 * along the file to see how to use it.
 * 
 * This file only contains the data structure and basic operations such as
 * adding/removing elements. For more advanced operations, see BMeshOperators.
 * For operations related to Unity, like converting to UnityEngine.Mesh, see
 * the BMeshUnity class.
 * 
 * The basic structure is described in the paper:
 * 
 *     Gueorguieva, Stefka and Marcheix, Davi. 1994. "Non-manifold boundary
 *     representation for solid modeling."
 *     
 * We use the same terminology as Blender's dev documentation:
 *     https://wiki.blender.org/wiki/Source/Modeling/BMesh/Design
 *     
 * Arbitrary attributes can be attached topological entity, namely vertices,
 * edges, loops and faces. If you are used to Houdini's terminology, note that
 * what is called "vertex" here corresponds to Houdini's points, while what
 * Houdini calls "vertex" is close to BMesh's "loops".
 *     
 * NB: The only dependency to Unity is the Vector3 structure, it can easily be
 * made independent.
 * 
 * NB: This class is not totally protected from misuse. We prefered fostering
 * ease of use over safety, so take care when you start feeling that you are
 * not fully understanding what you are doing, you'll likely mess with the
 * structure. For instance, do not add edges directly to the mesh.edges
 * list but use AddEdge, etc.
 * 
 * NB: This file is long, use the #region to fold it conveniently.
 */
public partial class BMesh
{
	// Topological entities
	public List<Vertex> vertices;
	public List<Edge> edges;
	public List<Loop> loops;
	public List<Face> faces;


	public BMesh()
	{
		vertices = new List<Vertex>();
		loops = new List<Loop>();
		edges = new List<Edge>();
		faces = new List<Face>();

		vertexAttributes = new List<AttributeDefinition>();
		edgeAttributes = new List<AttributeDefinition>();
		loopAttributes = new List<AttributeDefinition>();
		faceAttributes = new List<AttributeDefinition>();
	}

	/**
     * Add a new vertex to the mesh.
     */
	public Vertex AddVertex(Vertex vert)
	{
		EnsureVertexAttributes(vert);
		vertices.Add(vert);
		return vert;
	}

	public Vertex[] AddVertices(Vertex[] verts)
	{
		EnsureVertexAttributes(verts);
		vertices.AddRange(verts);
		return verts;
	}

	public Vertex AddVertex(Vector3 point) => AddVertex(new Vertex(point));

	public Vertex AddVertex(float x, float y, float z) => AddVertex(new Vector3(x, y, z));

	/**
     * Add a new edge between two vertices. If there is already such edge,
     * return it without adding a new one.
     * If the vertices are not part of the mesh, the behavior is undefined.
     */
	public Edge AddEdge(Vertex vert1, Vertex vert2)
	{
		Debug.Assert(vert1 != vert2);

		var edge = FindEdge(vert1, vert2);
		if (edge != null) return edge;

		edge = new Edge
		{
			vert1 = vert1,
			vert2 = vert2,
		};
		EnsureEdgeAttributes(edge);
		edges.Add(edge);

		// Insert in vert1's edge list
		if (vert1.edge == null)
		{
			vert1.edge = edge;
			edge.next1 = edge.prev1 = edge;
		}
		else
		{
			edge.next1 = vert1.edge.Next(vert1);
			edge.prev1 = vert1.edge;
			edge.next1.SetPrev(vert1, edge);
			edge.prev1.SetNext(vert1, edge);
		}

		// Same for vert2 -- TODO avoid code duplication
		if (vert2.edge == null)
		{
			vert2.edge = edge;
			edge.next2 = edge.prev2 = edge;
		}
		else
		{
			edge.next2 = vert2.edge.Next(vert2);
			edge.prev2 = vert2.edge;
			edge.next2.SetPrev(vert2, edge);
			edge.prev2.SetNext(vert2, edge);
		}

		return edge;
	}

	public Edge AddEdge(int v1, int v2) => AddEdge(vertices[v1], vertices[v2]);

	/**
     * Add a new face that connects the array of vertices provided.
     * The vertices must be part of the mesh, otherwise the behavior is
     * undefined.
     * NB: There is no AddLoop, because a loop is an element of a face
     */
	public Face AddFace(Vertex[] fVerts)
	{
		if (fVerts.Length == 0) return null;

		//foreach (var v in fVerts) Debug.Assert(v != null);

		var fEdges = new Edge[fVerts.Length];

		int i, i_prev = fVerts.Length - 1;
		for (i = 0; i < fVerts.Length; ++i)
		{
			fEdges[i_prev] = AddEdge(fVerts[i_prev], fVerts[i]);
			i_prev = i;
		}

		var f = new Face();
		EnsureFaceAttributes(f);
		faces.Add(f);

		for (i = 0; i < fVerts.Length; ++i)
		{
			var loop = new Loop(fVerts[i], fEdges[i], f);
			EnsureLoopAttributes(loop);
			loops.Add(loop);
		}

		f.vertcount = fVerts.Length;
		return f;
	}

	public Face AddFace(Vertex v0, Vertex v1) => AddFace(new[] { v0, v1 });

	public Face AddFace(Vertex v0, Vertex v1, Vertex v2) => AddFace(new[] { v0, v1, v2 });

	public Face AddFace(Vertex v0, Vertex v1, Vertex v2, Vertex v3) => AddFace(new[] { v0, v1, v2, v3 });

	public Face AddFace(int i0, int i1) => AddFace(new[] { vertices[i0], vertices[i1] });

	public Face AddFace(int i0, int i1, int i2) => AddFace(new[] { vertices[i0], vertices[i1], vertices[i2] });

	public Face AddFace(int i0, int i1, int i2, int i3) => AddFace(new[] { vertices[i0], vertices[i1], vertices[i2], vertices[i3] });

	/**
     * Return an edge that links vert1 to vert2 in the mesh (an arbitrary one
     * if there are several such edges, which is possible with this structure).
     * Return null if there is no edge between vert1 and vert2 in the mesh.
     */
	public Edge FindEdge(Vertex vert1, Vertex vert2)
	{
		Debug.Assert(vert1 != vert2);
		if (vert1.edge == null || vert2.edge == null) return null;

		var e1 = vert1.edge;
		var e2 = vert2.edge;
		do
		{
			if (e1.ContainsVertex(vert2)) return e1;
			if (e2.ContainsVertex(vert1)) return e2;

			e1 = e1.Next(vert1);
			e2 = e2.Next(vert2);
		} while (e1 != vert1.edge && e2 != vert2.edge);
		return null;
	}

	/**
     * Remove the provided vertex from the mesh.
     * Removing a vertex also removes all the edges/loops/faces that use it.
     * If the vertex was not part of this mesh, the behavior is undefined.
     */
	public void RemoveVertex(Vertex v)
	{
		while (v.edge != null)
		{
			RemoveEdge(v.edge);
		}

		vertices.Remove(v);
	}

	/**
     * Remove the provided edge from the mesh.
     * Removing an edge also removes all associated loops/faces.
     * If the edge was not part of this mesh, the behavior is undefined.
     */
	public void RemoveEdge(Edge e)
	{
		while (e.loop != null)
		{
			RemoveLoop(e.loop);
		}

		// Remove reference in vertices
		if (e == e.vert1.edge) e.vert1.edge = e.next1 != e ? e.next1 : null;
		if (e == e.vert2.edge) e.vert2.edge = e.next2 != e ? e.next2 : null;

		// Remove from linked lists
		e.prev1.SetNext(e.vert1, e.next1);
		e.next1.SetPrev(e.vert1, e.prev1);

		e.prev2.SetNext(e.vert2, e.next2);
		e.next2.SetPrev(e.vert2, e.prev2);

		edges.Remove(e);
	}

	/**
     * Removing a loop also removes associated face.
     * used internally only, just RemoveFace(loop.face) outside of here.
     */
	private void RemoveLoop(Loop l)
	{
		if (l.face != null) // null iff loop is called from RemoveFace
		{
			// Trigger removing other loops, and this one again with l.face == null
			RemoveFace(l.face);
			return;
		}

		// remove from radial linked list
		if (l.radial_next == l)
			l.edge.loop = null;
		else
		{
			l.radial_prev.radial_next = l.radial_next;
			l.radial_next.radial_prev = l.radial_prev;
			if (l.edge.loop == l)
				l.edge.loop = l.radial_next;
		}

		// forget other loops of the same face so thet they get released from memory
		l.next = null;
		l.prev = null;

		loops.Remove(l);
	}

	/**
     * Remove the provided face from the mesh.
     * If the face was not part of this mesh, the behavior is undefined.
     * (actually almost ensured to be a true mess, but do as it pleases you :D)
     */
	public void RemoveFace(Face f)
	{
		var l = f.loop;
		Loop nextL = null;
		while (nextL != f.loop)
		{
			nextL = l.next;
			l.face = null; // prevent infinite recursion, because otherwise RemoveLoop calls RemoveFace
			RemoveLoop(l);
			l = nextL;
		}
		faces.Remove(f);
	}
}