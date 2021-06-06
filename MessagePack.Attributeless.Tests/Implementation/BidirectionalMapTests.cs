using System.Collections.Generic;
using FluentAssertions;
using MessagePack.Attributeless.Implementation;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests.Implementation
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
        public void ContainsLeft_returns_false_if_element_is_contained()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.ContainsLeft("z").Should().BeFalse();
        }

        [Test]
        public void ContainsLeft_returns_true_if_element_is_contained()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.ContainsLeft("b").Should().BeTrue();
        }

        [Test]
        public void ContainsRight_returns_false_if_element_is_contained()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.ContainsRight(23).Should().BeFalse();
        }

        [Test]
        public void ContainsRight_returns_true_if_element_is_contained()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.ContainsRight(17).Should().BeTrue();
        }

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
        public void RemoveLeft_with_element_doesnt_do_anything_if_element_is_not_contained()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.RemoveLeft("z");

            _underTest.LeftToRightView()
                .Should()
                .BeEquivalentTo(new Dictionary<string, int>
                {
                    ["a"] = 13,
                    ["b"] = 17,
                    ["c"] = 19
                });
        }

        [Test]
        public void RemoveLeft_with_element_removes_the_element()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.RemoveLeft("b");

            _underTest.LeftToRightView()
                .Should()
                .BeEquivalentTo(new Dictionary<string, int>
                {
                    ["a"] = 13,
                    ["c"] = 19
                });
        }

        [Test]
        public void RemoveLeft_with_predicate_removes_all_matching_elements()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.RemoveLeft(s => s is "b" or "c");

            _underTest.LeftToRightView().Should().BeEquivalentTo(new Dictionary<string, int> {["a"] = 13});
        }

        [Test]
        public void RemoveRight_with_element_doesnt_do_anything_if_element_is_not_contained()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.RemoveRight(23);

            _underTest.LeftToRightView()
                .Should()
                .BeEquivalentTo(new Dictionary<string, int>
                {
                    ["a"] = 13,
                    ["b"] = 17,
                    ["c"] = 19
                });
        }

        [Test]
        public void RemoveRight_with_element_removes_the_element()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.RemoveRight(17);

            _underTest.LeftToRightView()
                .Should()
                .BeEquivalentTo(new Dictionary<string, int>
                {
                    ["a"] = 13,
                    ["c"] = 19
                });
        }

        [Test]
        public void RemoveRight_with_predicate_removes_all_matching_elements()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.RemoveRight(i => i > 16);

            _underTest.LeftToRightView().Should().BeEquivalentTo(new Dictionary<string, int> {["a"] = 13});
        }

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
        public void SetLeftToRight_increments_count()
        {
            _underTest.SetLeftToRight("a", 13);
            _underTest.SetLeftToRight("b", 17);
            _underTest.SetLeftToRight("c", 19);

            _underTest.Count.Should().Be(3);
        }

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
        public void SetRightToLeft_increments_count()
        {
            _underTest.SetRightToLeft(13, "a");
            _underTest.SetRightToLeft(17, "b");
            _underTest.SetRightToLeft(19, "c");

            _underTest.Count.Should().Be(3);
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