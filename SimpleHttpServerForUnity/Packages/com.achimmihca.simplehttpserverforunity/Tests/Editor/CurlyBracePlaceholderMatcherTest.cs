using System.Collections.Generic;
using NUnit.Framework;

namespace SimpleHttpServerForUnity
{
	public class CurlyBracePlaceholderMatcherTest
	{
		[Test]
		public void CurlyBracePlaceholderTest()
		{
			CurlyBracePlaceholderMatcher matcher = new CurlyBracePlaceholderMatcher("http://api/{foo}/{bar}");
			
			bool isMatch = matcher.TryMatch("http://api/fooValue", out Dictionary<string, string> placeholderValues1);
			Assert.IsFalse(isMatch);
			Assert.IsNull(placeholderValues1);
			
			isMatch = matcher.TryMatch("http://api/fooValue/barValue", out Dictionary<string, string> placeholderValues2);
			Assert.IsTrue(isMatch);
			Assert.AreEqual("fooValue", placeholderValues2["foo"]);
			Assert.AreEqual("barValue", placeholderValues2["bar"]);
		}

        [Test]
        public void SegmentCountTest()
        {
            CurlyBracePlaceholderMatcher matcher1Segments = new CurlyBracePlaceholderMatcher("http://api/");
            CurlyBracePlaceholderMatcher matcher2Segments = new CurlyBracePlaceholderMatcher("api/entry");
            CurlyBracePlaceholderMatcher matcher3Segments = new CurlyBracePlaceholderMatcher("http://api/entry/{foo}/");
            CurlyBracePlaceholderMatcher matcher4Segments = new CurlyBracePlaceholderMatcher("api/entry/{foo}/bar");
            CurlyBracePlaceholderMatcher matcher5Segments = new CurlyBracePlaceholderMatcher("https://api/entry/{foo}/something/{bar}/");
            
            Assert.AreEqual(1, matcher1Segments.SegmentCount);
            Assert.AreEqual(2, matcher2Segments.SegmentCount);
            Assert.AreEqual(3, matcher3Segments.SegmentCount);
            Assert.AreEqual(4, matcher4Segments.SegmentCount);
            Assert.AreEqual(5, matcher5Segments.SegmentCount);
        }
        
		[Test]
		public void WildcardTest()
		{
			CurlyBracePlaceholderMatcher matcher = new CurlyBracePlaceholderMatcher("http://api/{foo}/*");
			
			bool isMatch = matcher.TryMatch("http://fooValue/something/else", out Dictionary<string, string> placeholderValues1);
			Assert.IsFalse(isMatch);
			Assert.IsNull(placeholderValues1);
			
			isMatch = matcher.TryMatch("http://api/fooValue/something/else", out Dictionary<string, string> placeholderValues2);
			Assert.IsTrue(isMatch);
			Assert.AreEqual("fooValue", placeholderValues2["foo"]);
			Assert.AreEqual("something/else", placeholderValues2["*"]);
		}
	}
}
