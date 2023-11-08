using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(builder => { /*builder.AddConsole();*/ });

        var context = new MyDbContext(loggerFactory);
        context.Database.EnsureCreated();
        InitializeData(context);

        Console.WriteLine("All posts:");
        var data = context.BlogPosts.Select(x => x.Title).ToList();
        Console.WriteLine(JsonSerializer.Serialize(data));
            
            
        Console.WriteLine("How many comments each user left:");
        var commentsPerUser = context.BlogComments
            .GroupBy(c => c.UserName)
            .Select(g => new { UserName = g.Key, CommentCount = g.Count() })
            .OrderBy(o => o.UserName)
            .ToList();

        foreach (var user in commentsPerUser)
        {
            Console.WriteLine($"{user.UserName}: {user.CommentCount}");
        }



        Console.WriteLine("Posts ordered by date of last comment. Result should include text of last comment:");
        var postsOrderedByLastComment = context.BlogPosts
            .Select(p => new
            {
                PostTitle = p.Title,
                LastCommentDate = p.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault().CreatedDate,
                LastCommentText = p.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault().Text
            })
            .OrderByDescending(p => p.LastCommentDate)
            .ToList();

        foreach (var post in postsOrderedByLastComment)
        {
            Console.WriteLine($"{post.PostTitle}: '{post.LastCommentDate}', '{post.LastCommentText}'");
        }


        Console.WriteLine("How many last comments each user left:");
        var lastCommentsPerUser = context.BlogPosts
            .Select(p => new
            {
                PostTitle = p.Title,
                LastComment = p.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault()
            })
            .GroupBy(p => p.LastComment.UserName)
            .Select(g => new { UserName = g.Key, LastCommentCount = g.Count() })
            .ToList();

        foreach (var user in lastCommentsPerUser)
        {
            Console.WriteLine($"{user.UserName}: {user.LastCommentCount}");
        }

    }

    private static void InitializeData(MyDbContext context)
    {
        context.BlogPosts.Add(new BlogPost("Post1")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("1", new DateTime(2020, 3, 2), "Petr"),
                new BlogComment("2", new DateTime(2020, 3, 4), "Elena"),
                new BlogComment("8", new DateTime(2020, 3, 5), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post2")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("3", new DateTime(2020, 3, 5), "Elena"),
                new BlogComment("4", new DateTime(2020, 3, 6), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post3")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("5", new DateTime(2020, 2, 7), "Ivan"),
                new BlogComment("6", new DateTime(2020, 2, 9), "Elena"),
                new BlogComment("7", new DateTime(2020, 2, 10), "Ivan"),
                new BlogComment("9", new DateTime(2020, 2, 14), "Petr"),
            }
        });
        context.SaveChanges();
    }
}