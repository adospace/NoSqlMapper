using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlMapper.JsonNET;
using NoSqlMapper.SqlServer;
using NoSqlMapper.Test.Models;

namespace NoSqlMapper.Test
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public async Task DatabaseTest_InsertWithoutId()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(@"Data Source=.\sql2017;Integrated Security=true")
                .UseJsonNET())
            {
                var db = await nsClient.GetDatabaseAsync("Test_InsertWithoutId");

                Assert.AreEqual("Test_InsertWithoutId", db.Name);

                var posts = await db.GetCollectionAsync<PostWithoutId>("posts-no-id");

                Assert.AreEqual(posts.Name, "posts-no-id");

                var post = await posts.InsertAsync(new PostWithoutId()
                {
                    Title = "title",
                    Description = "desc",
                    Body = "body",
                });

                Assert.IsNotNull(post);
            }
        }

        [TestMethod]
        public async Task DatabaseTest_InsertWithId()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(@"Data Source=.\sql2017;Integrated Security=true")
                .UseJsonNET())
            {
                var db = await nsClient.GetDatabaseAsync("Test_InsertWithId");

                Assert.AreEqual("Test_InsertWithId", db.Name);

                var posts = await db.GetCollectionAsync<Post>("posts");

                Assert.AreEqual(posts.Name, "posts");

                var post = await posts.InsertAsync(new Post()
                {
                    Title = "title",
                    Description = "desc",
                    Body = "body",
                });

                Assert.IsNotNull(post);
                Assert.AreNotEqual(Guid.Empty, post.Id);
            }
        }

    }
}
