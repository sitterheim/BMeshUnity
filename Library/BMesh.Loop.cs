﻿// Copyright (C) 2021-2022 Steffen Itterheim
// Usage is bound to the Unity Asset Store Terms of Service and EULA: https://unity3d.com/legal/as_terms

using System.Collections.Generic;
using UnityEngine;

public partial class BMesh
{
	/**
     * Since a face is basically a list of edges, and the Loop object is a node
     * of this list, called so because the list must loop.
     * A loop is associated to one and only one face.
     * 
     * A loop can be seen as a list of edges, it also stores a reference to a
     * vertex for commodity but technically it could be found through the edge.
     * It may also be interpreted as a "face corner", and is hence where one
     * typically stores UVs, because one vertex may have different UV
     * coordinates depending on the face.
     * 
     * On top of this, the loop is also used as a node of another linked list,
     * namely the radial list, that enables iterating over all the faces using
     * the same edge.
     */
	public class Loop
	{
		public Dictionary<string, AttributeValue> attributes; // [attribute] (extra attributes)

		public Vertex vert;
		public Edge edge;
		public Face face; // there is exactly one face using a loop

		public Loop radial_prev; // around edge
		public Loop radial_next;
		public Loop prev; // around face
		public Loop next;

		public Loop(Vertex v, Edge e, Face f)
		{
			vert = v;
			SetEdge(e);
			SetFace(f);
		}

		/**
         * Insert the loop in the linked list of the face.
         * (Used in constructor)
         */
		public void SetFace(Face f)
		{
			Debug.Assert(face == null);
			if (f.loop == null)
			{
				f.loop = this;
				next = prev = this;
			}
			else
			{
				prev = f.loop;
				next = f.loop.next;

				f.loop.next.prev = this;
				f.loop.next = this;

				f.loop = this;
			}
			face = f;
		}

		/**
         * Insert the loop in the radial linked list.
         * (Used in constructor)
         */
		public void SetEdge(Edge e)
		{
			Debug.Assert(edge == null);
			if (e.loop == null)
			{
				e.loop = this;
				radial_next = radial_prev = this;
			}
			else
			{
				radial_prev = e.loop;
				radial_next = e.loop.radial_next;

				e.loop.radial_next.radial_prev = this;
				e.loop.radial_next = this;

				e.loop = this;
			}
			edge = e;
		}
	}
}