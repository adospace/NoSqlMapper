using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlMapper.JsonNET;
using NoSqlMapper.Query;
using NoSqlMapper.SqlServer;

namespace NoSqlMapper.Test
{
    [TestClass]
    public class IntegrationTests
    {
        public TestContext TestContext { get; set; }

        private string ConnectionString => Environment.GetEnvironmentVariable("NO_SQL_MAPPER_TEST_CONNECTION_STRING") ?? 
                                           throw new ArgumentException("Set NO_SQL_MAPPER_TEST_CONNECTION_STRING environmental variable containing connection string to Sql Server 2016+");

        [TestMethod]
        public async Task DatabaseTest_InsertWithoutId()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
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
                .UseSqlServer(ConnectionString)
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
                .UseSqlServer(ConnectionString)
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

                var postFound = await posts.FindAsync(Query.Query.Eq("Title", "title"));

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
                .UseSqlServer(ConnectionString)
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
                .UseSqlServer(ConnectionString)
                .UseJsonNET())
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_FindAll_ContainsArray");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");

                await posts.InsertAsync(new Post()
                {
                    Title = "title",
                    Tags = new[] {"tag1", "tag2"},
                });

                var postFound = await posts.FindAsync(Query.Query.Contains("Tags", "tag1"));

                Assert.IsNotNull(postFound);
                Assert.AreEqual(1, postFound.Count());
                var post = postFound.Single();
                Assert.AreEqual("title", post.Title);
                Assert.IsNull(post.Description);
            }
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_NotContainsArray()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET())
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_FindAll_ContainsArray");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");

                await posts.InsertAsync(new Post()
                {
                    Title = "title",
                    Tags = new[] { "tag1", "tag2" },
                });

                var postFound = await posts.FindAsync(Query.Query.NotContains("Tags", "tag1"));

                Assert.IsNotNull(postFound);
                Assert.AreEqual(0, postFound.Count());
            }
        }


        [TestMethod]
        public async Task DatabaseTest_FindAll_Collection1()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET())
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_FindAll_Collection1");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");

                await posts.InsertAsync(new Post()
                {
                    Title = "title",
                    Tags = new[] { "tag1", "tag2" },
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content",
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "user"}, Content = "reply to comment"}}
                        }
                    })
                });

                await posts.InsertAsync(new Post()
                {
                    Title = "title of post 2",
                    Tags = new[] { "tag2" },
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content to post 2",
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "user"}, Content = "reply to comment of post 2"}}
                        },
                        new Comment()
                        {
                            Author = new User() {Username = "user 2"},
                            Content = "second comment content to post 2",
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "admin"}, Content = "admin reply to user 2 comment of post 2"}}
                        }
                    })
                });

                var postFound = await posts.FindAsync(Query.Query.Eq("Comments.Author.Username", "admin"));

                Assert.IsNotNull(postFound);
                Assert.AreEqual(2, postFound.Count());
            }
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_Collection2()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine)
            )
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_FindAll_Collection2");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");

                await posts.InsertAsync(new Post()
                {
                    Title = "title of post 2",
                    Tags = new[] { "tag2" },
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content to post 2",
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "user"}, Content = "reply to comment of post 2"}}
                        },
                        new Comment()
                        {
                            Author = new User() {Username = "user2"},
                            Content = "second comment content to post 2",
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "admin"}, Content = "admin reply to user 2 comment of post 2"}}
                        }
                    })
                });

                var query = Query.Query.Or(
                    Query.Query.Eq("Comments.Author.Username", "admin"),
                    Query.Query.Eq("Comments.Replies.Author.Username", "admin"));

                var postFoundCount = await posts.CountAsync(query);
                Assert.AreEqual(1, postFoundCount);

                var postsFound = await posts.FindAsync(query);

                Assert.IsNotNull(postsFound);
                Assert.AreEqual(1, postsFound.Count());
                var post = postsFound.Single();
                Assert.AreEqual("title of post 2", post.Title);
                Assert.IsNull(post.Description);
            }
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_Collection3_Sort()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine)
            )
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_FindAll_Collection3_Sort");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");

                await posts.InsertAsync(new Post()
                {
                    Title = "title of post 1",
                    Tags = new[] { "tag1", "tag2" },
                    Updated = DateTime.Now.AddDays(-1),
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content",
                            Updated = DateTime.Now.AddDays(-1),
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "user"}, Content = "reply to comment"}}
                        }
                    })
                });

                await posts.InsertAsync(new Post()
                {
                    Title = "title of post 2",
                    Tags = new[] { "tag2" },
                    Updated = DateTime.Now,
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content to post 2",
                            Updated = DateTime.Now,
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "user"}, Content = "reply to comment of post 2"}}
                        },
                        new Comment()
                        {
                            Author = new User() {Username = "user2"},
                            Content = "second comment content to post 2",
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "admin"}, Content = "admin reply to user 2 comment of post 2"}}
                        }
                    })
                });

                var query = Query.Query.Eq("Comments.Author.Username", "admin");

                var postFoundCount = await posts.CountAsync(query);
                Assert.AreEqual(2, postFoundCount);

                var postsFound = await posts.FindAsync(query, new[] {new SortDescription("Updated", SortOrder.Descending)});

                Assert.IsNotNull(postsFound);
                Assert.AreEqual(2, postsFound.Count());
                var post = postsFound.First();
                Assert.AreEqual("title of post 2", post.Title);
                Assert.IsNull(post.Description);

                postsFound = await posts.FindAsync(query, new[] { new SortDescription("Comments.Updated", SortOrder.Ascending) });

                Assert.IsNotNull(postsFound);
                Assert.AreEqual(2, postsFound.Count());
                post = postsFound.First();
                Assert.AreEqual("title of post 1", post.Title);
                Assert.IsNull(post.Description);

                postsFound = await posts.FindAllAsync(SortDescription.OrderById());
                Assert.IsNotNull(postsFound);
                Assert.AreEqual(2, postsFound.Count());

            }
        }


        [TestMethod]
        public async Task DatabaseTest_FindAll_Collection4()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine)
            )
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_FindAll_Collection4");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");

                await posts.InsertAsync(new Post()
                {
                    Title = "title of post 1",
                    Tags = new[] { "tag1", "tag2" },
                    Updated = DateTime.Now.AddDays(-1),
                    Author = new User() { Username = "admin" },
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content",
                            Updated = DateTime.Now.AddDays(-1),
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "user"}, Content = "reply to comment"}}
                        }
                    })
                });

                await posts.InsertAsync(new Post()
                {
                    Title = "title of post 2",
                    Tags = new[] { "tag2" },
                    Author = new User() { Username = "admin" },
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content to post 2",
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "user"}, Content = "reply to comment of post 2"}}
                        },
                        new Comment()
                        {
                            Author = new User() {Username = "user2"},
                            Content = "second comment content to post 2",
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "admin"}, Content = "admin reply to user 2 comment of post 2"}}
                        }
                    })
                });

                await posts.InsertAsync(new Post()
                {
                    Title = "title of post 3",
                    Tags = new[] { "tag3", "tag5" },
                    Updated = DateTime.Now.AddDays(-3),
                    Author = new User() { Username = "user" },
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content to post3",
                            Updated = DateTime.Now.AddDays(-3),
                        }
                    })
                });

                var adminPosts = await posts.FindAsync(_ => _.Author.Username == "admin");

                Assert.IsNotNull(adminPosts);
                Assert.AreEqual(2, adminPosts.Length);

                var userPostsAndReplies = await posts.FindAsync(_ => _.Author.Username == "user" || _.Comments[0].Replies[0].Author.Username == "user");

                Assert.IsNotNull(userPostsAndReplies);
                Assert.AreEqual(2, userPostsAndReplies.Length);

                var now = DateTime.Now;
                var oldPosts = await posts.FindAsync(_ => _.Updated < now);

                Assert.IsNotNull(oldPosts);
                Assert.AreEqual(2, oldPosts.Length);

            }
        }

        [TestMethod]
        public async Task DatabaseTest_Index()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine)
            )
            {
                var db = await nsClient.GetDatabaseAsync("DatabaseTest_Index");

                await db.DeleteCollectionAsync("posts");

                var posts = await db.GetCollectionAsync<Post>("posts");

                await posts.InsertAsync(new Post()
                {
                    Title = "title of post 1",
                    Tags = new[] { "tag1", "tag2" },
                    Updated = DateTime.Now.AddDays(-1),
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content",
                            Updated = DateTime.Now.AddDays(-1),
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "user"}, Content = "reply to comment"}}
                        }
                    })
                });

                await posts.InsertAsync(new Post()
                {
                    Title = "title of post 2",
                    Tags = new[] { "tag2" },
                    Updated = DateTime.Now,
                    FavoriteCount = 23,
                    Comments = new List<Comment>(new[]
                    {
                        new Comment()
                        {
                            Author = new User() {Username = "admin"},
                            Content = "comment content to post 2",
                            Updated = DateTime.Now,
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "user"}, Content = "reply to comment of post 2"}}
                        },
                        new Comment()
                        {
                            Author = new User() {Username = "user2"},
                            Content = "second comment content to post 2",
                            Replies = new[]
                                {new Reply() {Author = new User() {Username = "admin"}, Content = "admin reply to user 2 comment of post 2"}}
                        }
                    })
                });

                await posts.EnsureIndexAsync("Updated");

                //delete even if not exists
                await posts.DeleteIndexAsync("FavoriteCount");

                await posts.EnsureIndexAsync("FavoriteCount");

                //inspect query plan to find index usage
                await posts.FindAllAsync(SortDescription.OrderByDescending("FavoriteCount"));

                await posts.DeleteIndexAsync("FavoriteCount");

                //inspect query plan to not find index usage
                await posts.FindAllAsync(SortDescription.OrderByDescending("FavoriteCount"));
            }
        }

    }
}
