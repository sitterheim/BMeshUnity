// Copyright (C) 2021-2022 Steffen Itterheim
// Usage is bound to the Unity Asset Store Terms of Service and EULA: https://unity3d.com/legal/as_terms

using System.Collections.Generic;
using UnityEngine;

public partial class BMesh
{
	/**
     * Attributes are arbitrary data that can be attached to topologic entities.
     * There are identified by a name and their value is an array of either int
     * or float. This array has theoretically a fixed size but in practice you
     * can do whatever you want becase they are stored per entity, not in a
     * global buffer, so it is flexible. Maybe one day for better efficiency
     * they would use proper data buffers, but the API would change anyway at
     * that point.
     */
	public enum AttributeBaseType
	{
		Int,
		Float,
	}

	// Attribute definitions. The content of attributes is stored in the
	// topological objects (Vertex, Edge, etc.) in the 'attribute' field.
	// These lists are here to ensure consistency.

	public List<AttributeDefinition> vertexAttributes;
	public List<AttributeDefinition> edgeAttributes;
	public List<AttributeDefinition> loopAttributes;
	public List<AttributeDefinition> faceAttributes;

	/**
     * The same series of method repeats for Vertices, Edges, Loops and Faces.
     * Maybe there's a nice way to factorize, but in the meantime I'll at least
     * factorize the comments, so the following work for all types of
     * topological entities.
     */
	/**
	 * Check whether the mesh as an attribute enforced to any vertices with the
	 * given name. If this is true, one can safely use v.attributes[attribName]
	 * without checking v.attributes.ContainsKey() first.
	 */
	public bool HasVertexAttribute(string attribName)
	{
		foreach (var a in vertexAttributes)
		{
			if (a.name == attribName)
				return true;
		}
		return false;
	}

	public bool HasVertexAttribute(AttributeDefinition attrib) => HasVertexAttribute(attrib.name);

	/**
     * Add a new attribute and return it, so that one can write oneliners like
     *     AddVertexAttribute("foo", Float, 3).defaultValue = ...
     * NB: It does not return the attribute from the mesh definition if it
     * existed already. Maybe this can be considered as a bug, maybe not.
     */
	public AttributeDefinition AddVertexAttribute(AttributeDefinition attrib)
	{
		if (HasVertexAttribute(attrib)) return attrib; // !!

		vertexAttributes.Add(attrib);
		foreach (var v in vertices)
		{
			if (v.attributes == null) v.attributes = new Dictionary<string, AttributeValue>(); // move in Vertex ctor?
			v.attributes[attrib.name] = AttributeValue.Copy(attrib.defaultValue);
		}
		return attrib;
	}

	public AttributeDefinition AddVertexAttribute(string name, AttributeBaseType baseType, int dimensions) =>
		AddVertexAttribute(new AttributeDefinition(name, baseType, dimensions));

	/**
     * Called internally when adding a new vertex to ensure that the vertex has
     * all required attribute. If not, the default value is used to add it.
     */
	private void EnsureVertexAttributes(Vertex v)
	{
		if (v.attributes == null) v.attributes = new Dictionary<string, AttributeValue>();
		foreach (var attr in vertexAttributes)
		{
			if (!v.attributes.ContainsKey(attr.name))
				v.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
			else if (!attr.type.CheckValue(v.attributes[attr.name]))
			{
				Debug.LogWarning("Vertex attribute '" + attr.name + "' is not compatible with mesh attribute definition, ignoring.");
				// different type, overriding value with default
				v.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
			}
		}
	}

	private void EnsureVertexAttributes(Vertex[] verts)
	{
		foreach (var vertex in verts)
			EnsureVertexAttributes(vertex);
	}

	public bool HasEdgeAttribute(string attribName)
	{
		foreach (var a in edgeAttributes)
		{
			if (a.name == attribName)
				return true;
		}
		return false;
	}

	public bool HasEdgeAttribute(AttributeDefinition attrib) => HasEdgeAttribute(attrib.name);

	public AttributeDefinition AddEdgeAttribute(AttributeDefinition attrib)
	{
		if (HasEdgeAttribute(attrib)) return attrib;

		edgeAttributes.Add(attrib);
		foreach (var e in edges)
		{
			if (e.attributes == null) e.attributes = new Dictionary<string, AttributeValue>(); // move in Edge ctor?
			e.attributes[attrib.name] = AttributeValue.Copy(attrib.defaultValue);
		}
		return attrib;
	}

	public AttributeDefinition AddEdgeAttribute(string name, AttributeBaseType baseType, int dimensions) =>
		AddEdgeAttribute(new AttributeDefinition(name, baseType, dimensions));

	private void EnsureEdgeAttributes(Edge e)
	{
		if (e.attributes == null) e.attributes = new Dictionary<string, AttributeValue>();
		foreach (var attr in edgeAttributes)
		{
			if (!e.attributes.ContainsKey(attr.name))
				e.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
			else if (!attr.type.CheckValue(e.attributes[attr.name]))
			{
				Debug.LogWarning("Edge attribute '" + attr.name + "' is not compatible with mesh attribute definition, ignoring.");
				// different type, overriding value with default
				e.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
			}
		}
	}

	public bool HasLoopAttribute(string attribName)
	{
		foreach (var a in loopAttributes)
		{
			if (a.name == attribName)
				return true;
		}
		return false;
	}

	public bool HasLoopAttribute(AttributeDefinition attrib) => HasLoopAttribute(attrib.name);

	public AttributeDefinition AddLoopAttribute(AttributeDefinition attrib)
	{
		if (HasLoopAttribute(attrib)) return attrib;

		loopAttributes.Add(attrib);
		foreach (var l in loops)
		{
			if (l.attributes == null) l.attributes = new Dictionary<string, AttributeValue>(); // move in Loop ctor?
			l.attributes[attrib.name] = AttributeValue.Copy(attrib.defaultValue);
		}
		return attrib;
	}

	public AttributeDefinition AddLoopAttribute(string name, AttributeBaseType baseType, int dimensions) =>
		AddLoopAttribute(new AttributeDefinition(name, baseType, dimensions));

	private void EnsureLoopAttributes(Loop l)
	{
		if (l.attributes == null) l.attributes = new Dictionary<string, AttributeValue>();
		foreach (var attr in loopAttributes)
		{
			if (!l.attributes.ContainsKey(attr.name))
				l.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
			else if (!attr.type.CheckValue(l.attributes[attr.name]))
			{
				Debug.LogWarning("Loop attribute '" + attr.name + "' is not compatible with mesh attribute definition, ignoring.");
				// different type, overriding value with default
				l.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
			}
		}
	}

	public bool HasFaceAttribute(string attribName)
	{
		foreach (var a in faceAttributes)
		{
			if (a.name == attribName)
				return true;
		}
		return false;
	}

	public bool HasFaceAttribute(AttributeDefinition attrib) => HasFaceAttribute(attrib.name);

	public AttributeDefinition AddFaceAttribute(AttributeDefinition attrib)
	{
		if (HasFaceAttribute(attrib)) return attrib;

		faceAttributes.Add(attrib);
		foreach (var f in faces)
		{
			if (f.attributes == null) f.attributes = new Dictionary<string, AttributeValue>(); // move in Face ctor?
			f.attributes[attrib.name] = AttributeValue.Copy(attrib.defaultValue);
		}
		return attrib;
	}

	public AttributeDefinition AddFaceAttribute(string name, AttributeBaseType baseType, int dimensions) =>
		AddFaceAttribute(new AttributeDefinition(name, baseType, dimensions));

	private void EnsureFaceAttributes(Face f)
	{
		if (f.attributes == null) f.attributes = new Dictionary<string, AttributeValue>();
		foreach (var attr in faceAttributes)
		{
			if (!f.attributes.ContainsKey(attr.name))
				f.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
			else if (!attr.type.CheckValue(f.attributes[attr.name]))
			{
				Debug.LogWarning("Face attribute '" + attr.name + "' is not compatible with mesh attribute definition, ignoring.");
				// different type, overriding value with default
				f.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
			}
		}
	}
}