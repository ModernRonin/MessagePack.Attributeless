using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests
{
    [TestFixture]
    public class BidirectionalMapTests
    {
        [SetUp]
        public void Setup()
        {
            _underTest = new BidirectionalMap<string, int>();
        }

        BidirectionalMap<string, int> _underTest;

        [Test]
        public void Count_is_zero_after_construction() => _underTest.Count.Should().Be(0);

        [Test]
        public void LeftFor_returns_the_corresponding_left_element()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.LeftFor(17).Should().Be("b");
        }

        [Test]
        public void LeftToRightView_is_empty_after_construction() =>
            _underTest.LeftToRightView().Should().BeEmpty();

        [Test]
        public void RightFor_returns_the_corresponding_right_element()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.RightFor("b").Should().Be(17);
        }

        [Test]
        public void RightToLeftView_is_empty_after_construction() =>
            _underTest.RightToLeftView().Should().BeEmpty();

        [Test]
        public void SetLeftToRight_overwrites_preexisting_values()
        {
            _underTest.SetLeftToRight("a", 17);
            _underTest.SetLeftToRight("a", 13);

            _underTest.LeftToRightView().Should().BeEquivalentTo(new Dictionary<string, int> {["a"] = 13});
            _underTest.RightToLeftView().Should().BeEquivalentTo(new Dictionary<int, string> {[13] = "a"});
        }

        [Test]
        public void SetLeftToRight_sets_LeftToRightView()
        {
            _underTest.SetLeftToRight("a", 13);

            _underTest.LeftToRightView().Should().BeEquivalentTo(new Dictionary<string, int> {["a"] = 13});
        }

        [Test]
        public void SetLeftToRight_sets_RightToLeftView()
        {
            _underTest.SetLeftToRight("a", 13);

            _underTest.RightToLeftView().Should().BeEquivalentTo(new Dictionary<int, string> {[13] = "a"});
        }

        [Test]
        public void SetRightToLeft_overwrites_preexisting_values()
        {
            _underTest.SetRightToLeft(17, "a");
            _underTest.SetRightToLeft(13, "a");

            _underTest.LeftToRightView().Should().BeEquivalentTo(new Dictionary<string, int> {["a"] = 13});
            _underTest.RightToLeftView().Should().BeEquivalentTo(new Dictionary<int, string> {[13] = "a"});
        }

        [Test]
        public void SetRightToLeft_sets_LeftToRightView()
        {
            _underTest.SetRightToLeft(13, "a");

            _underTest.LeftToRightView().Should().BeEquivalentTo(new Dictionary<string, int> {["a"] = 13});
        }

        [Test]
        public void SetRightToLeft_sets_RightToLeftView()
        {
            _underTest.SetRightToLeft(13, "a");

            _underTest.RightToLeftView().Should().BeEquivalentTo(new Dictionary<int, string> {[13] = "a"});
        }
    }
}