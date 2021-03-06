﻿using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TableDependency.Enums;
using TableDependency.EventArgs;
using TableDependency.IntegrationTest.Base;
using TableDependency.SqlClient;

namespace TableDependency.IntegrationTest
{
    public class RowVersionTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Version { get; set; }

}

    [TestClass]
    public class RowVersionType : SqlTableDependencyBaseTest
    {
        private static readonly string TableName = typeof(RowVersionTypeModel).Name;
        private byte[] _rowVersionInsert = null;
        private byte[] _rowVersionUpdate = null;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            using (var sqlConnection = new SqlConnection(ConnectionStringForTestUser))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = $"IF OBJECT_ID('{TableName}', 'U') IS NOT NULL DROP TABLE [{TableName}];";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = $"CREATE TABLE {TableName}(Id INT, Name VARCHAR(50), Version ROWVERSION);";
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        [TestInitialize()]
        public void TestInitialize()
        {
        }
        
        [ClassCleanup()]
        public static void ClassCleanup()
        {
            using (var sqlConnection = new SqlConnection(ConnectionStringForTestUser))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = $"IF OBJECT_ID('{TableName}', 'U') IS NOT NULL DROP TABLE [{TableName}];";
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        [TestCategory("SqlServer")]
        [TestMethod]
        public void Test()
        {
            SqlTableDependency<RowVersionTypeModel> tableDependency = null;

            try
            {
                tableDependency = new SqlTableDependency<RowVersionTypeModel>(ConnectionStringForTestUser, TableName);
                tableDependency.OnChanged += this.TableDependency_Changed;
                tableDependency.Start();

                Thread.Sleep(5000);

                var t = new Task(ModifyTableContent);
                t.Start();
                Thread.Sleep(1000 * 10 * 1);
            }
            finally
            {
                tableDependency?.Dispose();
            }

            Assert.AreNotEqual(_rowVersionInsert, _rowVersionUpdate);
        }

        private void TableDependency_Changed(object sender, RecordChangedEventArgs<RowVersionTypeModel> e)
        {

            switch (e.ChangeType)
            {
                case ChangeType.Insert:
                    _rowVersionInsert = e.Entity.Version;
                    break;

                case ChangeType.Update:
                    _rowVersionUpdate = e.Entity.Version;
                    break;
            }
        }

        private static void ModifyTableContent()
        {
            using (var sqlConnection = new SqlConnection(ConnectionStringForTestUser))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = $"INSERT INTO [{TableName}] ([Id], [Name]) VALUES (1, 'AA')";                   
                    sqlCommand.ExecuteNonQuery();
                    Thread.Sleep(1000);
                }

                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = $"UPDATE [{TableName}] SET [Name] = 'BB' WHERE [Id] = 1";
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}