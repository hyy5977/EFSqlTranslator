﻿using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    public class AnyTranslationTests
    {
        [Test]
        public void Test_Any_With_Predicate()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Posts.Any(p => p.Content != null));

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0'
    from Posts p0
    where p0.'Content' is not null
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
where sq0.'BlogId_jk0' is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_Any_Without_Predicate()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Posts.Any());

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0'
    from Posts p0
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
where sq0.'BlogId_jk0' is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}