using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlMapper.Test.Models;

namespace NoSqlMapper.Test
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TypeReflector_Test()
        {
            var typePost = TypeReflector.Create<Post>();

            Assert.IsNotNull(typePost);
            Assert.AreEqual(9, typePost.Properties.Count);

            Assert.AreEqual(typeof(string), typePost.Properties["Title"].PropertyType);
            Assert.AreEqual(typeof(List<Comment>), typePost.Properties["Comments"].PropertyType);
            Assert.AreEqual(typeof(string[]), typePost.Properties["Tags"].PropertyType);

            var userType = typePost.Navigate("Author");
            Assert.IsNotNull(userType);
            Assert.AreEqual(typeof(string), userType.Properties["Username"].PropertyType);


            var commentType = typePost.Navigate("Comments");
            Assert.IsNotNull(commentType);
            Assert.AreEqual(typeof(string), commentType.Properties["Content"].PropertyType);

            userType = typePost.Navigate("Comments.Author");
            Assert.IsNotNull(userType);
            Assert.AreEqual(typeof(string), userType.Properties["Username"].PropertyType);

            userType = typePost.Navigate("Comments[0].Author");
            Assert.IsNotNull(userType);
            Assert.AreEqual(typeof(string), userType.Properties["Username"].PropertyType);

            commentType = typePost.Navigate("Comments.Replies");
            Assert.IsNotNull(commentType);
            Assert.AreEqual(typeof(string), commentType.Properties["Content"].PropertyType);

            userType = typePost.Navigate("Comments.Replies.Author");
            Assert.IsNotNull(userType);
            Assert.AreEqual(typeof(string), userType.Properties["Username"].PropertyType);

        }
    }
}
