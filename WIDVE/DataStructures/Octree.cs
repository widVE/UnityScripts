using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.DataStructures
{
    public class Octree<T> where T : System.IEquatable<T>
    {
        public Node Root { get; private set; }

        public int NumNodes { get; private set; }

        public int NumItems { get; private set; }

        public float MinHalfSize { get; private set; }

        public Octree(float minHalfSize, Vector3 center, float halfSize)
		{
            MinHalfSize = minHalfSize;
            Root = new Node(null, center, halfSize);
            NumNodes = 1;
            NumItems = 0;
		}

        public Node Add(T item, Vector3 position)
		{
            return Root.Add(this, item, position);
		}

        public Node Remove(T item, Vector3 position)
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

        public T GetClosestItem(Vector3 position, bool drawGizmos = false)
		{
            Node.SearchItem? n_closestSearchItem = Root.GetClosestItem(position, new List<Node.SearchItem>(), drawGizmos);
            if(n_closestSearchItem != null)
			{
                Node.SearchItem closestSearchItem = (Node.SearchItem)n_closestSearchItem;
                if(closestSearchItem.Type == Node.SearchItem.Types.Item) return closestSearchItem.Item.Value;

            }
            return default(T);
		}

        public class Node
        {
            public class Item
            {
                public T Value;
                public Vector3 Position;

                public Item(T value, Vector3 position)
                {
                    Value = value;
                    Position = position;
                }
            }

            public struct SearchItem
			{
                public enum Types { Item, Node }
                public Types Type;
                public Item Item;
                public Node Node;
                public float Distance;

                public SearchItem(Item item, Vector3 position)
                {
                    Type = Types.Item;
                    Item = item;
                    Node = null;
                    //Distance = Vector3.Distance(position, Item.Position);
                    Distance = (position - Item.Position).sqrMagnitude;
                }

                public SearchItem(Node node, Vector3 position)
				{
                    Type = Types.Node;
                    Item = null;
                    Node = node;
                    //Distance = Node.MinDistance(position);
                    Distance = Node.MinDistanceSquared(position);
                }
			}

            Node Parent;

            public Vector3 Center { get; private set; }

            public float HalfSize { get; private set; }

            public float Size => HalfSize * 2;

            public Vector3 Min => new Vector3(Center.x - HalfSize, Center.y - HalfSize, Center.z - HalfSize);

            public Vector3 Max => new Vector3(Center.x + HalfSize, Center.y + HalfSize, Center.z + HalfSize);

            int _depth = -1;
            public int Depth => _depth < 0 ? _depth = GetDepth() : _depth;

            public bool IsLeaf => Contents.Count != 0;

            public List<Item> Contents;

            public Node[] Children;

            public Node(Node parent, Vector3 center, float halfSize)
            {
                Parent = parent;
                Center = center;
                HalfSize = halfSize;
                Contents = new List<Item>();
                Children = new Node[8];
                for(int i = 0; i < Children.Length; i++)
                {
                    Children[i] = null;
                }
            }

            public Vector3 ClosestPoint(Vector3 position)
			{
                //returns the closest position either within the node or on its surface   
                Vector3 min = Min;
                Vector3 max = Max;
                float pX = Mathf.Clamp(position.x, min.x, max.x);
                float pY = Mathf.Clamp(position.y, min.y, max.y);
                float pZ = Mathf.Clamp(position.z, min.z, max.z);
                return new Vector3(pX, pY, pZ);
            }

            public float MinDistanceSquared(Vector3 position)
			{
                Vector3 pNode = ClosestPoint(position);
                return (position - pNode).sqrMagnitude;
            }

            public SearchItem? GetClosestItem(Vector3 position, List<SearchItem> searchList, bool drawGizmos)
			{
                if(drawGizmos) DrawNodeGizmos(-1, false);

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
                searchList.Sort((x, y) => -x.Distance.CompareTo(y.Distance));

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
                            //Debug.Log($"Found closest point to {position}: {searchItem.Item.Position} [d: {searchItem.Distance}]");
                            if(drawGizmos) Gizmos.DrawWireSphere(position, .3f);
                            return searchItem;
                        default:
                        case SearchItem.Types.Node:
                            //if this is a node, search its children
                            //Debug.Log($"Getting closest point to {position} (checking node at depth {searchItem.Node.GetDepth()}) ({searchList.Count} items in searchList)");
                            //if(drawGizmos) searchItem.Node.DrawNodeGizmos(-1, false);
                            return searchItem.Node.GetClosestItem(position, searchList, drawGizmos);
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

            Node AddChild(Octree<T> tree, int index)
            {
                Vector3 childCenter = IndexToChildCenter(index);
                float childHalfSize = HalfSize / 2;
                Node child = new Node(this, childCenter, childHalfSize);
                Children[index] = child;
                tree.NumNodes++;
                return child;
            }

            public Node Add(Octree<T> tree, T item, Vector3 position)
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
                    Contents.Add(new Item(item, position));
                    tree.NumItems++;
                    return this;
                }
            }

            public Node Remove(Octree<T> tree, T item, Vector3 position)
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
                        if(Contents[i].Value.Equals(item))
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

            public void DrawNodeGizmos(int index, bool drawChildren = true)
            {
                if(index >= 0) Gizmos.color = IndexToColor(index);
                Gizmos.DrawWireCube(Center, Vector3.one * HalfSize * 2);

                if(drawChildren)
                {
                    for(int i = 0; i < Children.Length; i++)
                    {
                        if(Children[i] != null) Children[i].DrawNodeGizmos(i);
                    }
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