using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            var userTypes = TypeReflector.Navigate<Post>("Author.Username").ToList();
            Assert.IsNotNull(userTypes);
            Assert.AreEqual(typeof(User), userTypes[0].Type);
            Assert.AreEqual(typeof(string), userTypes[1].Type);

            var tagsType = TypeReflector.Navigate<Post>("Tags").ToList();
            Assert.IsNotNull(tagsType);
            Assert.IsTrue(tagsType[0].IsValueArray);
            Assert.AreEqual(typeof(string), tagsType[0].Type);

            var commentsType = TypeReflector.Navigate<Post>("Comments.Content").ToList();
            Assert.IsNotNull(commentsType);
            Assert.IsTrue(commentsType[0].IsObjectArray);
            Assert.AreEqual(typeof(Comment), commentsType[0].Type);
            Assert.AreEqual(typeof(string), commentsType[1].Type);

            userTypes = TypeReflector.Navigate<Post>("Comments.Author.Username").ToList();
            Assert.IsNotNull(userTypes);
            Assert.IsTrue(userTypes[0].IsObjectArray);
            Assert.AreEqual(typeof(Comment), userTypes[0].Type);
            Assert.AreEqual(typeof(User), userTypes[1].Type);
            Assert.AreEqual(typeof(string), userTypes[2].Type);

            var replieTypes = TypeReflector.Navigate<Post>("Comments.Replies.Author.Username").ToList();
            Assert.IsNotNull(replieTypes);
            Assert.IsTrue(replieTypes[0].IsObjectArray);
            Assert.AreEqual(typeof(Comment), replieTypes[0].Type);
            Assert.IsTrue(replieTypes[1].IsObjectArray);
            Assert.AreEqual(typeof(Comment), replieTypes[1].Type);
            Assert.AreEqual(typeof(User), replieTypes[2].Type);
            Assert.AreEqual(typeof(string), replieTypes[3].Type);
        }
    }
}
