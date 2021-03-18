using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.DataStructures
{
    public class Octree<T> where T : System.IEquatable<T>
    {
        public OctreeNode Root { get; private set; }

        public int NumNodes { get; private set; }

        public int NumItems { get; private set; }

        public float MinHalfSize { get; private set; }

        public Octree(float minHalfSize, Vector3 center, float halfSize)
		{
            MinHalfSize = minHalfSize;
            Root = new OctreeNode(null, center, halfSize);
            NumNodes = 1;
            NumItems = 0;
		}

        public OctreeNode Add(T item, Vector3 position)
		{
            return Root.Add(this, item, position);
		}

        public OctreeNode Remove(T item, Vector3 position)
		{
            return Root.Remove(this, item, position);
        }

        public void DrawNodeGizmos()
		{
            Root.DrawNodeGizmos(0);
		}

        public void DrawItemGizmos()
		{
            Root.DrawItemGizmos(0);
		}

        public T GetClosestItem(Vector3 position)
		{
            OctreeNode.SearchItem? n_closestSearchItem = Root.GetClosestItem(position, new List<OctreeNode.SearchItem>());
            if(n_closestSearchItem != null)
			{
                OctreeNode.SearchItem closestSearchItem = (OctreeNode.SearchItem)n_closestSearchItem;
                if(closestSearchItem.Type == OctreeNode.SearchItem.Types.Item) return closestSearchItem.Item.Item;

            }
            return default(T);
		}

        public class OctreeNode
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

            public struct SearchItem
			{
                public enum Types { Item, Node }
                public Types Type;
                public OctreeItem Item;
                public OctreeNode Node;
                public float Distance;

                public SearchItem(OctreeItem item, Vector3 position)
                {
                    Type = Types.Item;
                    Item = item;
                    Node = null;
                    Distance = Vector3.Distance(position, Item.Position);
                }

                public SearchItem(OctreeNode node, Vector3 position)
				{
                    Type = Types.Node;
                    Item = null;
                    Node = node;
                    Distance = Node.MinDistance(position);
                }
			}

            OctreeNode Parent;

            public Vector3 Center { get; private set; }

            public float HalfSize { get; private set; }

            public float Size => HalfSize * 2;

            public Vector3 Min => new Vector3(Center.x - HalfSize, Center.y - HalfSize, Center.z - HalfSize);

            public Vector3 Max => new Vector3(Center.x + HalfSize, Center.y + HalfSize, Center.z + HalfSize);

            public int Depth => GetDepth();

            public bool IsLeaf => Contents.Count != 0;

            public List<OctreeItem> Contents;

            public OctreeNode[] Children;

            public OctreeNode(OctreeNode parent, Vector3 center, float halfSize)
            {
                Parent = parent;
                Center = center;
                HalfSize = halfSize;
                Contents = new List<OctreeItem>();
                Children = new OctreeNode[8];
                for(int i = 0; i < Children.Length; i++)
                {
                    Children[i] = null;
                }
            }

            public float MinDistance(Vector3 position)
			{
                Vector3 min = Min;
                Vector3 max = Max;
                float dX = Mathf.Min(Mathf.Abs(position.x - min.x), Mathf.Abs(position.x - max.x));
                float dY = Mathf.Min(Mathf.Abs(position.y - min.y), Mathf.Abs(position.y - max.y));
                float dZ = Mathf.Min(Mathf.Abs(position.z - min.z), Mathf.Abs(position.z - max.z));
                return Mathf.Min(dX, dY, dZ);
            }

            public SearchItem? GetClosestItem(Vector3 position, List<SearchItem> searchList)
			{
				if(IsLeaf)
				{
                    //add contents to search list
                    for(int i = 0; i < Contents.Count; i++)
					{
                        searchList.Add(new SearchItem(Contents[i], position));
					}
				}
				else
				{
                    //add child nodes to search list
                    for(int i = 0; i < Children.Length; i++)
					{
                        if(Children[i] != null) searchList.Add(new SearchItem(Children[i], position));
					}
				}

                //sort list (farthest at start, closest at end)
                searchList.Sort((x, y) => y.Distance.CompareTo(x.Distance));

                //check items in the list
                while(searchList.Count > 0)
				{
                    //remove and check the last (closest) item in the list
                    SearchItem searchItem = searchList[searchList.Count - 1];
                    searchList.RemoveAt(searchList.Count - 1);

					switch(searchItem.Type)
					{
                        case SearchItem.Types.Item:
                            //if this is an item, search is done
                            return searchItem;
                        default:
                        case SearchItem.Types.Node:
                            //if this is a node, search its children
                            return searchItem.Node.GetClosestItem(position, searchList);
					}
				}

                //if search reaches here, nothing was found
                return null;
			}

            public bool Contains(Vector3 position)
            {
                //return true;
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
                bool y = (index & 2) == 2;
                bool z = (index & 4) == 4;
                float qs = HalfSize / 2;
                return Center + new Vector3(x ? qs : -qs, y ? qs : -qs, z ? qs : -qs);
            }

            Color IndexToColor(int index)
			{
                bool x = (index & 1) == 1;
                bool y = (index & 2) == 2;
                bool z = (index & 4) == 4;
                return new Color(x ? .9f : .1f, y ? .9f : .1f, z ? .9f : .1f);
            }

            OctreeNode AddChild(Octree<T> tree, int index)
            {
                Vector3 childCenter = IndexToChildCenter(index);
                float childHalfSize = HalfSize / 2;
                OctreeNode child = new OctreeNode(this, childCenter, childHalfSize);
                Children[index] = child;
                tree.NumNodes++;
                return child;
            }

            public OctreeNode Add(Octree<T> tree, T item, Vector3 position)
            {
                if(HalfSize > tree.MinHalfSize)
                {
                    //recursively find a leaf node
                    int index = PositionToIndex(position);
                    if(Children[index] == null) AddChild(tree, index);
                    return Children[index].Add(tree, item, position);
                }
                else
                {
                    //this is a leaf node; add the item
                    Contents.Add(new OctreeItem(item, position));
                    tree.NumItems++;
                    return this;
                }
            }

            public OctreeNode Remove(Octree<T> tree, T item, Vector3 position)
            {
                if(HalfSize > tree.MinHalfSize)
                {
                    //recursively find a leaf node
                    int index = PositionToIndex(position);
                    if(Children[index] != null)
                    {
                        return Children[index].Remove(tree, item, position);
					}
					else
					{
                        return null;
					}
                }
                else
                {
                    //this is a leaf node; remove the item
                    for(int i = 0; i < Contents.Count; i++)
                    {
                        if(Contents[i].Item.Equals(item))
                        {
                            Contents.RemoveAt(i);
                            tree.NumItems--;
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

            public void DrawNodeGizmos(int index)
            {
                Gizmos.color = IndexToColor(index);
                Gizmos.DrawWireCube(Center, Vector3.one * HalfSize * 2);

                for(int i = 0; i < Children.Length; i++)
				{
                    if(Children[i] != null) Children[i].DrawNodeGizmos(i);
				}
            }

            public void DrawItemGizmos(int index)
			{
                Gizmos.color = Color.Lerp(Color.gray, IndexToColor(index), .75f);
                float pointSize = HalfSize / 4;

                for(int i = 0; i < Contents.Count; i++)
				{
                    Gizmos.DrawSphere(Contents[i].Position, pointSize);
				}

                for(int i = 0; i < Children.Length; i++)
                {
                    if(Children[i] != null) Children[i].DrawItemGizmos(i);
                }
            }
        } 
    }
}