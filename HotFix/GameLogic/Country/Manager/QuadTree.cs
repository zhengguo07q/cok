using UnityEngine;
using System.Collections.Generic;

namespace GameLogic.Country.Manager
{
    public class QuadTree<T>
    {
        private class Node
        {
            public Rect Bounds;
            public List<(Vector2 position, T item)> objects;
            public Node[] Children;
            public bool IsLeaf => Children == null;

            public Node(Rect bounds)
            {
                Bounds = bounds;
                objects = new List<(Vector2, T)>();
            }
        }

        private readonly Node root;
        private readonly int maxObjectsPerNode;
        private readonly float minNodeSize;
        private readonly Dictionary<T, Vector2> objectsPositions;

        public QuadTree(Rect bounds, int maxObjectsPerNode = 8, float minNodeSize = 1f)
        {
            this.root = new Node(bounds);
            this.maxObjectsPerNode = maxObjectsPerNode;
            this.minNodeSize = minNodeSize;
            this.objectsPositions = new Dictionary<T, Vector2>();
        }

        public void Insert(Vector2 position, T item)
        {
            objectsPositions[item] = position;
            Insert(root, position, item);
        }

        public void Remove(T item)
        {
            if (objectsPositions.TryGetValue(item, out Vector2 position))
            {
                Remove(root, position, item);
                objectsPositions.Remove(item);
            }
        }

        public void Update(Vector2 newPosition, T item)
        {
            Remove(item);
            Insert(newPosition, item);
        }

        public List<T> QueryRange(Rect range)
        {
            var result = new List<T>();
            QueryRange(root, range, result);
            return result;
        }

        private void Insert(Node node, Vector2 position, T item)
        {
            if (!node.Bounds.Contains(position))
                return;

            if (node.IsLeaf)
            {
                node.objects.Add((position, item));
                
                if (node.objects.Count > maxObjectsPerNode && node.Bounds.size.x > minNodeSize)
                {
                    Split(node);
                }
            }
            else
            {
                for (int i = 0; i < node.Children.Length; i++)
                {
                    if (node.Children[i].Bounds.Contains(position))
                    {
                        Insert(node.Children[i], position, item);
                        break;
                    }
                }
            }
        }

        private void Split(Node node)
        {
            float halfWidth = node.Bounds.width * 0.5f;
            float halfHeight = node.Bounds.height * 0.5f;
            Vector2 center = node.Bounds.center;

            node.Children = new Node[4];
            node.Children[0] = new Node(new Rect(center.x - halfWidth, center.y - halfHeight, halfWidth, halfHeight));
            node.Children[1] = new Node(new Rect(center.x, center.y - halfHeight, halfWidth, halfHeight));
            node.Children[2] = new Node(new Rect(center.x - halfWidth, center.y, halfWidth, halfHeight));
            node.Children[3] = new Node(new Rect(center.x, center.y, halfWidth, halfHeight));

            var oldItems = node.objects;
            node.objects = new List<(Vector2, T)>();

            foreach (var item in oldItems)
            {
                for (int i = 0; i < node.Children.Length; i++)
                {
                    if (node.Children[i].Bounds.Contains(item.position))
                    {
                        Insert(node.Children[i], item.position, item.item);
                        break;
                    }
                }
            }
        }

        private bool Remove(Node node, Vector2 position, T item)
        {
            if (!node.Bounds.Contains(position))
                return false;

            if (node.IsLeaf)
            {
                for (int i = node.objects.Count - 1; i >= 0; i--)
                {
                    if (EqualityComparer<T>.Default.Equals(node.objects[i].item, item))
                    {
                        node.objects.RemoveAt(i);
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < node.Children.Length; i++)
                {
                    if (Remove(node.Children[i], position, item))
                        return true;
                }
            }

            return false;
        }

        private void QueryRange(Node node, Rect range, List<T> result)
        {
            if (!node.Bounds.Overlaps(range))
                return;

            if (node.IsLeaf)
            {
                foreach (var item in node.objects)
                {
                    if (range.Contains(item.position))
                    {
                        result.Add(item.item);
                    }
                }
            }
            else
            {
                for (int i = 0; i < node.Children.Length; i++)
                {
                    QueryRange(node.Children[i], range, result);
                }
            }
        }
    }
}