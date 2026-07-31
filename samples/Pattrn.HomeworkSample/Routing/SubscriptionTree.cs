// Copyright (C) Abc Arbitrage Asset Management - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Written by  <Simon Gelbart@abc-arbitrage.com>, 2023-11-03

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Homework.Routing
{
    public class TreeNode<TKey, TValue> where TKey : IEquatable<TKey>
    {
        public HashSet<TValue> Data { get; }
        public ConcurrentDictionary<TKey, TreeNode<TKey, TValue>> Children { get; }

        public TreeNode()
        {
            Data = new HashSet<TValue>();
            Children = new ConcurrentDictionary<TKey, TreeNode<TKey, TValue>>();
        }
    }


    public class SubscriptionTree
    {
        private readonly TreeNode<string, Subscription> _children;
        private readonly object _lock = new object();

        public SubscriptionTree() => _children = new TreeNode<string, Subscription>();

        public void Add(Subscription subscription)
        {
            lock (_lock)
            {
                AddPathToTree(_children, subscription.ContentPattern.Parts.ToList(), subscription);
            }
        }

        public void Remove(Subscription subscription)
        {
            lock (_lock)
            {
                RemoveFromTree(_children, subscription.ContentPattern.Parts.ToList(), subscription);
            }
        }

        public void Remove(ClientId consumer)
        {
            lock (_lock)
            {
                RemoveFromChildren(_children, consumer);
            }
        }

        private void RemoveFromChildren(TreeNode<string, Subscription> children, ClientId consumer)
        {
            children.Data.RemoveWhere(x => x.ConsumerId.Equals(consumer));

            foreach (var child in children.Children)
            {
                RemoveFromChildren(child.Value, consumer);
            }
        }

        private void RemoveFromTree(TreeNode<string, Subscription> node, IList<string> path, Subscription subscription)
        {
            if (path.Count == 0)
            {
                node.Data.Remove(subscription);
                return;
            }

            string nextData = path[0];
            TreeNode<string, Subscription> nextNode;
            if (!node.Children.TryGetValue(nextData, out nextNode!))
            {
                node.Data.Remove(subscription);
                return;
            }

            path.RemoveAt(0);
            RemoveFromTree(nextNode, path, subscription);
            if (!IsNodeEmpty(nextNode)) return;

            TreeNode<string, Subscription> oldChild;
            node.Children.TryRemove(nextData, out oldChild!);
        }

        private bool IsNodeEmpty(TreeNode<string, Subscription> treeNode)
        {
            return treeNode.Data.Count == 0 && treeNode.Children.IsEmpty;
        }

        private void AddPathToTree(TreeNode<string, Subscription> node, IList<string> path, Subscription subscription)
        {
            if (path.Count == 0)
            {
                node.Data.Add(subscription);
                return;
            }
            var nextData = path[0];
            TreeNode<string, Subscription> nextNode;
            if (!node.Children.TryGetValue(nextData, out nextNode!))
            {
                nextNode = new TreeNode<string, Subscription>();
                node.Children.TryAdd(nextData, nextNode);
            }
            path.RemoveAt(0);
            AddPathToTree(nextNode, path, subscription);
        }

        private void GetSubscriptionFromPathTree(
            TreeNode<string, Subscription> node,
            IList<string> path,
            int pathIndex,
            List<Subscription> subscriptions)
        {
            subscriptions.AddRange(node.Data);
            if (pathIndex == path.Count)
                return;

            var nextData = path[pathIndex];
            TreeNode<string, Subscription> childNode;
            bool isChildNodeSearchNeeded = node.Children.TryGetValue(nextData, out childNode!);

            TreeNode<string, Subscription> starNode;
            bool isStarNodeSearchNeeded =node.Children.TryGetValue("*", out starNode!);
            if (isChildNodeSearchNeeded)
                GetSubscriptionFromPathTree(childNode, path, pathIndex + 1, subscriptions);
            if (isStarNodeSearchNeeded)
                GetSubscriptionFromPathTree(starNode, path, pathIndex + 1, subscriptions);
        }

        public List<Subscription> GetSubscriptions(IList<string> path)
        {
            List<Subscription> result = new List<Subscription>();
            GetSubscriptionFromPathTree(_children, path, 0, result);
            return result;
        }
    }
}
