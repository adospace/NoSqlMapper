using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlMapper.SqlServer;
using NoSqlMapper.JsonNET;

namespace NoSqlMapper.Test
{
    [TestClass]
    public class IntegrationTestsSqlServer
    {
        private string ConnectionString => Environment.GetEnvironmentVariable("NS_MAPPER_TEST_SQL_SERVER_CONNECTION_STRING") ??
                                           throw new ArgumentException(
                                               "Set NS_MAPPER_TEST_SQL_SERVER_CONNECTION_STRING environmental variable containing connection string to Sql Server 2016+");

        [TestMethod]
        public async Task DatabaseTest_InsertWithoutId()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_InsertWithoutId(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_InsertWithId()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_InsertWithId(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_FindAll(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_FindByObjectId()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_FindByObjectId(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_ContainsArray()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_FindAll_ContainsArray(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_NotContainsArray()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_FindAll_NotContainsArray(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_Collection1()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_FindAll_Collection1(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_Collection2()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_FindAll_Collection2(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_Collection3_Sort()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_FindAll_Collection3_Sort(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_FindAll_Collection4()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_FindAll_Collection4(nsClient);
        }

        [TestMethod]
        public async Task DatabaseTest_Index()
        {
            using (var nsClient = new NsConnection()
                .UseSqlServer(ConnectionString)
                .UseJsonNET()
                .LogTo(Console.WriteLine))
                await IntegrationTests.DatabaseTest_Index(nsClient);
        }
    }
}
