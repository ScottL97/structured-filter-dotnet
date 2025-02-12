namespace StructuredFilter.Utils;

using System.Collections.Generic;

public sealed class TreeNode<T>(T value)
{
    public T Value { get; } = value;
    public List<TreeNode<T>> Children { get; } = [];

    public void AddChild(TreeNode<T> child)
    {
        Children.Add(child);
    }
}

public sealed class Tree<T>(T? rootValue)
{
    public TreeNode<T>? Root { get; set; } = rootValue is null ? null : new TreeNode<T>(rootValue);

    public IEnumerable<T> Traverse()
    {
        if (Root == null)
            yield break;

        var stack = new Stack<TreeNode<T>>();
        stack.Push(Root);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();
            yield return currentNode.Value;

            for (int i = currentNode.Children.Count - 1; i >= 0; i--)
            {
                stack.Push(currentNode.Children[i]);
            }
        }
    }

    public Tree<T> AddLevelAboveRoot(T newRootValue)
    {
        var newRoot = new TreeNode<T>(newRootValue);
        if (Root != null)
        {
            newRoot.AddChild(Root);
        }
        Root = newRoot;
        return this;
    }
}
