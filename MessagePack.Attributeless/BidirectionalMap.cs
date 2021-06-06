﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MessagePack.Attributeless
{
    public class BidirectionalMap<TLeft, TRight> : IEnumerable<KeyValuePair<TLeft, TRight>>
    {
        readonly Dictionary<TLeft, TRight> _leftToRight = new Dictionary<TLeft, TRight>();
        readonly Dictionary<TRight, TLeft> _rightToLeft = new Dictionary<TRight, TLeft>();
        public IEnumerator<KeyValuePair<TLeft, TRight>> GetEnumerator() => _leftToRight.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TLeft this[TRight key]
        {
            get => _rightToLeft[key];
            set
            {
                _rightToLeft[key] = value;
                _leftToRight[value] = key;
            }
        }

        public TRight this[TLeft key]
        {
            get => _leftToRight[key];
            set
            {
                _rightToLeft[value] = key;
                _leftToRight[key] = value;
            }
        }

        public bool ContainsLeft(TLeft left) => _leftToRight.ContainsKey(left);
        public bool ContainsRight(TRight right) => _rightToLeft.ContainsKey(right);

        public IReadOnlyDictionary<TLeft, TRight> LeftToRightView() => _leftToRight;

        public void RemoveLeft(TLeft left)
        {
            var right = _leftToRight[left];
            _leftToRight.Remove(left);
            _rightToLeft.Remove(right);
        }

        public void RemoveWhereLeft(Func<TLeft, bool> predicate)
        {
            var leftKeysToRemove = _leftToRight.Keys.Where(predicate).ToArray();
            foreach (var left in leftKeysToRemove) RemoveLeft(left);
        }

        public IReadOnlyDictionary<TRight, TLeft> RightToLeftView() => _rightToLeft;

        public int Count => _leftToRight.Count;
    }
}