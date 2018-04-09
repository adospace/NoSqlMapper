using System;
using System.Collections.Generic;
using System.Linq;
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

        [TestMethod]
        public async Task DatabaseTest_FindAll()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(@"Data Source=.\sql2017;Integrated Security=true")
                .UseJsonNET())
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_FindAll");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");

                await posts.InsertAsync(new Post()
                {
                    Title = "title",
                    Description = "desc",
                    Body = "body",
                });

                var postFound = await posts.FindAllAsync(Query.Query.Eq("Title", "title"));

                Assert.IsNotNull(postFound);
                Assert.AreEqual(1, postFound.Count());
                var post = postFound.Single();
                Assert.AreEqual("title", post.Title);
                Assert.AreEqual("desc", post.Description);
                Assert.AreEqual("body", post.Body);
            }
        }

        [TestMethod]
        public async Task DatabaseTest_FindByObjectId()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(@"Data Source=.\sql2017;Integrated Security=true")
                .UseJsonNET())
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_FindByObjectId");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");
                var id = Guid.NewGuid();
                await posts.InsertAsync(new Post()
                {
                    Id = id,
                    Title = "title",
                    Description = "desc",
                    Body = "body",
                });

                var post = await posts.FindAsync(id);

                Assert.IsNotNull(post);
                Assert.AreEqual("title", post.Title);
                Assert.AreEqual("desc", post.Description);
                Assert.AreEqual("body", post.Body);
            }
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_ContainsArray()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(@"Data Source=.\sql2017;Integrated Security=true")
                .UseJsonNET())
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_FindAll_ContainsArray");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");

                await posts.InsertAsync(new Post()
                {
                    Title = "title",
                    Tags = new[] {"tag1", "tag2"},
                    Comments = new List<Comment>(new[]
                        {new Comment() {Author = new User() {Username = "admin"}, Content = "comment content"}})
                });

                var postFound = await posts.FindAllAsync(Query.Query.Contains("Tags", "tag1"));

                Assert.IsNotNull(postFound);
                Assert.AreEqual(1, postFound.Count());
                var post = postFound.Single();
                Assert.AreEqual("title", post.Title);
                Assert.AreEqual("desc", post.Description);
                Assert.AreEqual("body", post.Body);
            }
        }

    }
}
