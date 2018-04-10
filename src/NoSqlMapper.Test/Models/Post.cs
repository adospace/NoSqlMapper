using System;
using System.Collections.Generic;
using System.Text;

namespace NoSqlMapper.Test.Models
{
    public class PostWithoutId
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public List<Comment> Comments { get; set; }

        public User Author { get; set; }

        public DateTime Updated { get; set; }

        public int FavoriteCount { get; set; }

        public string[] Tags { get; set; }

    }

    public class Post : PostWithoutId
    {
        public Guid Id { get; set; }
    }

    public class User
    {
        public string Username { get; set; }

        public string Email { get; set; }
    }

    public class Comment
    {
        public string Content { get; set; }

        public User Author { get; set; }

        public Comment[] Replies { get; set; }
    }
}
