using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using EFSqlTranslator.Translation.Extensions;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [CategoryReadMe(
         Index = 7,
         Title = "Translating Manual Join",
         Description = @"
This libary supports more complicated join. You can define your own join condition rather than
have to be limited to column pairs."
     )]
    public class ManualTranslationTests
    {
        [Fact]
        [TranslationReadMe(
             Index = 0,
             Title = "Join on custom condition"
         )]
        public void Test_Translate_Join_Select_Columns()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Posts.Any(p => p.User.UserName != null));

                var query1 = db.Posts.
                    Join(
                        query,
                        (p, b) => p.BlogId == b.BlogId && p.User.UserName == "ethan",
                        (p, b) => new { PId = p.PostId, b.Name },
                        DbJoinType.LeftOuter);

                var script = QueryTranslator.Translate(query1.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.PostId as 'PId', sq0.Name
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
left outer join (
    select b0.Name, b0.BlogId as 'BlogId_jk0'
    from Blogs b0
    left outer join (
        select p0.BlogId as 'BlogId_jk0'
        from Posts p0
        inner join Users u0 on p0.UserId = u0.UserId
        where u0.UserName is not null
        group by p0.BlogId
    ) sq0 on b0.BlogId = sq0.BlogId_jk0
    where sq0.BlogId_jk0 is not null
) sq0 on (p0.BlogId = sq0.BlogId_jk0) and (u0.UserName = 'ethan')";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        [TranslationReadMe(
            Index = 0,
            Title = "Join on custom condition"
        )]
        public void Test_Translate_Join_Aggregate_Columns()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.GroupBy(x => x.BlogId).Select(x => new
                {
                    BlogId = x.Key,
                    MaxLikes = x.Max(z => z.LikeCount)
                });

                var query1 = db.Posts.
                    Join(
                        query,
                        (p, b) => p.BlogId == b.BlogId && p.LikeCount == b.MaxLikes,
                        (p, b) => new { PId = p.PostId, b.BlogId },
                        DbJoinType.RightOuter);

                var script = QueryTranslator.Translate(query1.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.PostId as 'PId', sq0.BlogId
from Posts p0
right outer join (
    select sq0.BlogId as 'BlogId_jk0', sq0.MaxLikes as 'MaxLikes_jk0'
    from (
        select p0.BlogId, max(p0.LikeCount) as 'MaxLikes'
        from Posts p0
        group by p0.BlogId

    ) sq0

) sq0 on (p0.BlogId = sq0.BlogId_jk0) and (p0.LikeCount = sq0.MaxLikes_jk0)";


                TestUtils.AssertStringEqual(expected, sql);
            }
        }


        [Fact]
        [TranslationReadMe(
            Index = 0,
            Title = "Join on custom condition"
        )]
        public void Test_Translate_Join_Aggregate_Columns3()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.GroupBy(x => x.BlogId).Select(x => new
                {
                    BlogId = x.Key,
                    MaxLikes = x.Max(z => z.LikeCount)
                });

                var query1 = db.Posts.
                    Join(
                        query,
                        (p, b) => p.BlogId == b.BlogId && p.LikeCount == b.MaxLikes,
                        (p, b) => new { BlogTitle = p.Blog.Name, Blog = p.Blog, PostWithMaxLikes = p.Title },
                        DbJoinType.RightOuter);

                var script = QueryTranslator.Translate(query1.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.Name as 'BlogTitle', b0.*, p0.Title as 'PostWithMaxLikes'
from Posts p0
inner join Blogs b0 on p0.BlogId = b0.BlogId
right outer join (
    select sq0.BlogId as 'BlogId_jk0', sq0.MaxLikes as 'MaxLikes_jk0'
    from (
        select p0.BlogId, max(p0.LikeCount) as 'MaxLikes'
        from Posts p0
        group by p0.BlogId

    ) sq0

) sq0 on (p0.BlogId = sq0.BlogId_jk0) and (p0.LikeCount = sq0.MaxLikes_jk0)";


                TestUtils.AssertStringEqual(expected, sql);
            }
        }


        [Fact]
        public void Test_Translate_Join_Aggregate_Columns2()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .GroupBy(x => x.BlogId)
                    .Select(x => new
                    {
                        BlogId = x.Key,
                        MaxLikes = x.Max(z => z.LikeCount)
                    });

                var query1 = db.Posts
                    .Join(
                        query,
                        (p, b) => p.BlogId == b.BlogId && p.LikeCount == b.MaxLikes,
                        (p, b) => new { BlogTitle = p.Blog.Name, p.Blog.BlogId, PostWithMaxLikes = p.Title },
                        DbJoinType.RightOuter);

                var script = QueryTranslator.Translate(query1.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.Name as 'BlogTitle', b0.BlogId, p0.Title as 'PostWithMaxLikes'
from Posts p0
inner join Blogs b0 on p0.BlogId = b0.BlogId
right outer join (
    select sq0.BlogId as 'BlogId_jk0', sq0.MaxLikes as 'MaxLikes_jk0'
    from (
        select p0.BlogId, max(p0.LikeCount) as 'MaxLikes'
        from Posts p0
        group by p0.BlogId
    ) sq0
) sq0 on (p0.BlogId = sq0.BlogId_jk0) and (p0.LikeCount = sq0.MaxLikes_jk0)
";


                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}
