﻿using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Pomelo.EntityFrameworkCore.MySql.PerfTests.Models;
using Xunit;

namespace Pomelo.EntityFrameworkCore.MySql.PerfTests.Tests.Connection
{
    public class ConnectionTest
    {
        private static readonly MySqlConnection Connection = new MySqlConnection(AppConfig.Config["Data:ConnectionString"]);

        private static AppDb NewDbContext(bool reuseConnection)
        {
            return reuseConnection ? new AppDb(Connection) : new AppDb();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AffectedRowsFalse(bool reuseConnection)
        {
            var title = "test";
            var blog = new Blog {Title = title};
            using (var db = NewDbContext(reuseConnection))
            {
                db.Blogs.Add(blog);
                await db.SaveChangesAsync();
            }
            Assert.True(blog.Id > 0);

            // this will throw a DbUpdateConcurrencyException if UseAffectedRows=true
            var sameBlog = new Blog {Id = blog.Id, Title = title};
            using (var db = NewDbContext(reuseConnection))
            {
                db.Blogs.Update(sameBlog);
                await db.SaveChangesAsync();
            }
            Assert.Equal(blog.Id, sameBlog.Id);
        }
    }
}