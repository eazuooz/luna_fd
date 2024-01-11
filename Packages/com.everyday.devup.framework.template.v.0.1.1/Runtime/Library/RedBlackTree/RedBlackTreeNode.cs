namespace OpinionatedCode.Collections
{
    internal sealed class RedBlackTreeNode<TKey, TValue>
    {
        public enum ColorEnum
        {
            Red,
            Black
        };

        public readonly TValue Value;

        public readonly TKey Key;

        public readonly bool IsLeaf;

        public readonly int HashedKey;

        public ColorEnum Color;

        public RedBlackTreeNode<TKey, TValue> Parent;

        public RedBlackTreeNode<TKey, TValue> Left;

        public RedBlackTreeNode<TKey, TValue> Right;

        public static RedBlackTreeNode<TKey, TValue> CreateLeaf()
        {
            return new RedBlackTreeNode<TKey, TValue>();
        }

        public static RedBlackTreeNode<TKey, TValue> CreateNode(TKey key, TValue value, ColorEnum color, int hashedKey)
        {
            return new RedBlackTreeNode<TKey, TValue>(key, value, color, hashedKey);
        }

        internal RedBlackTreeNode(TKey key, TValue value, ColorEnum color, int hashedKey)
        {
            Value = value;
            HashedKey = hashedKey;
            Color = color;
            Key = key;
        }

        internal RedBlackTreeNode()
        {
            IsLeaf = true;
            Color = ColorEnum.Black;
            HashedKey = 0;
        }

        public RedBlackTreeNode<TKey, TValue> Grandparent => Parent?.Parent;

        public RedBlackTreeNode<TKey, TValue> Sibling =>
            Parent == null ? null : Parent.Left == this ? Parent.Right : Parent.Left;

        public RedBlackTreeNode<TKey, TValue> Uncle => Parent?.Sibling;
    }
}