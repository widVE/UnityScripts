using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.DataStructures
{
    public class Octree<T> where T : System.IEquatable<T>
    {
        public OctreeNode<T> Root { get; private set; }

        public float MinHalfSize { get; private set; }

        public Octree(float minHalfSize, Vector3 center, float halfSize)
		{
            MinHalfSize = minHalfSize;
            Root = new OctreeNode<T>(null, center, halfSize);
		}

        public OctreeNode<T> Add(T item, Vector3 position)
		{
            return Root.Add(item, position, MinHalfSize);
		}

        public OctreeNode<T> Remove(T item, Vector3 position)
		{
            return Root.Remove(item, position, MinHalfSize);
		}
    }

    public class OctreeNode<T> where T : System.IEquatable<T>
    {
        public class OctreeItem
		{
            public T Item;
            public Vector3 Position;

            public OctreeItem(T item, Vector3 position)
			{
                Item = item;
                Position = position;
			}
		}

        OctreeNode<T> Parent;

        public Vector3 Center { get; private set; }

        public float HalfSize { get; private set; }

        public float Size => HalfSize * 2;

        public Vector3 Min => new Vector3(Center.x - HalfSize, Center.y - HalfSize, Center.z - HalfSize);

        public Vector3 Max => new Vector3(Center.x + HalfSize, Center.y + HalfSize, Center.z + HalfSize);

        public int Depth => GetDepth();

        public List<OctreeItem> Contents;

        public OctreeNode<T>[] Children;

        public OctreeNode(OctreeNode<T> parent, Vector3 center, float halfSize)
        {
            Parent = parent;
            Center = center;
            HalfSize = halfSize;
            Contents = new List<OctreeItem>();
            Children = new OctreeNode<T>[8];
            for(int i = 0; i < Children.Length; i++)
			{
                Children[i] = null;
			}
        }

        public bool Contains(Vector3 position)
		{
            if(position.x < Min.x) return false;
            if(position.y < Min.y) return false;
            if(position.z < Min.z) return false;
            if(position.x > Max.x) return false;
            if(position.y > Max.y) return false;
            if(position.z > Max.z) return false;
            return true;
        }

        int PositionToIndex(Vector3 position)
		{
            //index is in zyx order (most -> least significant bits)
            int x = position.x > Center.x ? 1 : 0;
            int y = position.y > Center.y ? 1 : 0;
            int z = position.z > Center.z ? 1 : 0;
            return (z << 2) | (y << 1) | x;
        }

        Vector3 IndexToChildCenter(int index)
		{
            bool x = (index & 1) == 1;
            bool y = (index & 3) == 1;
            bool z = (index & 5) == 1;
            float qs = HalfSize / 2;
            return Center + new Vector3(x ? qs : -qs, y ? qs : -qs, z ? qs : -qs);
		}

        OctreeNode<T> AddChild(int index)
        {
            Vector3 childCenter = IndexToChildCenter(index);
            float childHalfSize = HalfSize / 2;
            OctreeNode<T> child = new OctreeNode<T>(this, childCenter, childHalfSize);
            Children[index] = child;
            return child;
        }

        public OctreeNode<T> Add(T item, Vector3 position, float minHalfSize)
		{
            if(!Contains(position))
            {
                return null;
            }
            else
            {
                if(HalfSize > minHalfSize)
				{
                    int index = PositionToIndex(position);
                    if(Children[index] == null) AddChild(index);
                    return Children[index].Add(item, position, minHalfSize);
				}
				else
				{
                    Contents.Add(new OctreeItem(item, position));
                    return this;
				}
            }
		}

        public OctreeNode<T> Remove(T item, Vector3 position, float minHalfSize)
		{
            if(HalfSize > minHalfSize)
            {
                int index = PositionToIndex(position);
                if(Children[index] == null) AddChild(index);
                return Children[index].Remove(item, position, minHalfSize);
			}
			else
			{
                for(int i = 0; i< Contents.Count; i++)
			    {
                    if(Contents[i].Item.Equals(item))
                    {
                        Contents.RemoveAt(i);
                        return this;
                    }
			    }
                return null;
			}
		}

        int GetDepth()
		{
            int depth = 1;
            if(Parent != null) depth += Parent.GetDepth();
            return depth;
		}

        public void DrawGizmo(Gradient gradient = null)
		{
            if(gradient != null)
			{

			}

            Gizmos.DrawWireCube(Center, Vector3.one * HalfSize * 2);
		}
    }
}