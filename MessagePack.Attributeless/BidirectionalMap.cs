using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MessagePack.Attributeless
{
    public sealed class BidirectionalMap<TLeft, TRight> : IEnumerable<KeyValuePair<TLeft, TRight>>
    {
        readonly Dictionary<TLeft, TRight> _leftToRight = new Dictionary<TLeft, TRight>();
        readonly Dictionary<TRight, TLeft> _rightToLeft = new Dictionary<TRight, TLeft>();
        public IEnumerator<KeyValuePair<TLeft, TRight>> GetEnumerator() => _leftToRight.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool ContainsLeft(TLeft left) => _leftToRight.ContainsKey(left);
        public bool ContainsRight(TRight right) => _rightToLeft.ContainsKey(right);

        public TLeft LeftFor(TRight right) => _rightToLeft[right];

        public IReadOnlyDictionary<TLeft, TRight> LeftToRightView() => _leftToRight;

        public void RemoveLeft(TLeft left)
        {
            if (!ContainsLeft(left)) return;

            var right = _leftToRight[left];
            _leftToRight.Remove(left);
            _rightToLeft.Remove(right);
        }

        public void RemoveLeft(Func<TLeft, bool> predicate)
        {
            var leftKeysToRemove = _leftToRight.Keys.Where(predicate).ToArray();
            foreach (var left in leftKeysToRemove) RemoveLeft(left);
        }

        public void RemoveRight(TRight right)
        {
            if (!ContainsRight(right)) return;

            var left = _rightToLeft[right];
            _leftToRight.Remove(left);
            _rightToLeft.Remove(right);
        }

        public void RemoveRight(Func<TRight, bool> predicate)
        {
            var rightKeysToRemove = _rightToLeft.Keys.Where(predicate).ToArray();
            foreach (var right in rightKeysToRemove) RemoveRight(right);
        }

        public TRight RightFor(TLeft left) => _leftToRight[left];

        public IReadOnlyDictionary<TRight, TLeft> RightToLeftView() => _rightToLeft;

        public void SetLeftToRight(TLeft left, TRight right)
        {
            if (_leftToRight.ContainsKey(left)) RemoveLeft(left);
            _rightToLeft[right] = left;
            _leftToRight[left] = right;
        }

        public void SetRightToLeft(TRight right, TLeft left)
        {
            if (_leftToRight.ContainsKey(left)) RemoveLeft(left);
            _rightToLeft[right] = left;
            _leftToRight[left] = right;
        }

        public int Count => _leftToRight.Count;
    }
}