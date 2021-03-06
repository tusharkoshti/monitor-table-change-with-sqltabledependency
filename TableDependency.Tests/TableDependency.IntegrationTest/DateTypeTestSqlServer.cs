﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TableDependency.Enums;
using TableDependency.EventArgs;
using TableDependency.IntegrationTest.Base;
using TableDependency.SqlClient;

namespace TableDependency.IntegrationTest
{
    public class DateTypeTestModel
    {
        public DateTime? DateColumn { get; set; }
        public DateTime? DatetimeColumn { get; set; }
        public DateTime? Datetime2Column { get; set; }
        public DateTimeOffset? DatetimeoffsetColumn { get; set; }
    }

    [TestClass]
    public class DateTypeTest : SqlTableDependencyBaseTest
    {
        private static readonly string TableName = typeof(DateTypeTestModel).Name;
        private static readonly Dictionary<string, Tuple<DateTypeTestModel, DateTypeTestModel>> CheckValues = new Dictionary<string, Tuple<DateTypeTestModel, DateTypeTestModel>>();

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

                    sqlCommand.CommandText = $"CREATE TABLE [{TableName}] (" +
                        "dateColumn date NULL, " +
                        "datetimeColumn DATETIME NULL, " +
                        "datetime2Column datetime2(7) NULL, " +
                        "datetimeoffsetColumn DATETIMEOFFSET(7) NULL)";
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
        public void CheckDateTypeTest()
        {
            SqlTableDependency<DateTypeTestModel> tableDependency = null;

            try
            {
                tableDependency = new SqlTableDependency<DateTypeTestModel>(ConnectionStringForTestUser, TableName);
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

            Assert.AreEqual(CheckValues[ChangeType.Insert.ToString()].Item2.DateColumn, CheckValues[ChangeType.Insert.ToString()].Item1.DateColumn);
            Assert.IsNull(CheckValues[ChangeType.Insert.ToString()].Item2.DatetimeColumn);
            Assert.AreEqual(CheckValues[ChangeType.Insert.ToString()].Item2.Datetime2Column, CheckValues[ChangeType.Insert.ToString()].Item1.Datetime2Column);
            Assert.AreEqual(CheckValues[ChangeType.Insert.ToString()].Item2.DatetimeoffsetColumn, CheckValues[ChangeType.Insert.ToString()].Item1.DatetimeoffsetColumn);

            Assert.IsNull(CheckValues[ChangeType.Update.ToString()].Item2.DateColumn);
            var date1 = CheckValues[ChangeType.Update.ToString()].Item1.DatetimeColumn.GetValueOrDefault().AddMilliseconds(-CheckValues[ChangeType.Update.ToString()].Item1.DatetimeColumn.GetValueOrDefault().Millisecond);
            var date2 = CheckValues[ChangeType.Update.ToString()].Item2.DatetimeColumn.GetValueOrDefault().AddMilliseconds(-CheckValues[ChangeType.Update.ToString()].Item2.DatetimeColumn.GetValueOrDefault().Millisecond);
            Assert.AreEqual(date1.ToString("yyyyMMddhhmm"), date2.ToString("yyyyMMddhhmm"));
            Assert.IsNull(CheckValues[ChangeType.Update.ToString()].Item2.Datetime2Column);
            Assert.AreEqual(CheckValues[ChangeType.Update.ToString()].Item2.DatetimeoffsetColumn, CheckValues[ChangeType.Update.ToString()].Item1.DatetimeoffsetColumn);

            Assert.IsNull(CheckValues[ChangeType.Delete.ToString()].Item2.DateColumn);
            date1 = CheckValues[ChangeType.Update.ToString()].Item1.DatetimeColumn.GetValueOrDefault().AddMilliseconds(-CheckValues[ChangeType.Update.ToString()].Item1.DatetimeColumn.GetValueOrDefault().Millisecond);
            date2 = CheckValues[ChangeType.Update.ToString()].Item2.DatetimeColumn.GetValueOrDefault().AddMilliseconds(-CheckValues[ChangeType.Update.ToString()].Item2.DatetimeColumn.GetValueOrDefault().Millisecond);
            Assert.AreEqual(date1.ToString("yyyyMMddhhmm"), date2.ToString("yyyyMMddhhmm")); Assert.IsNull(CheckValues[ChangeType.Delete.ToString()].Item2.Datetime2Column);
            Assert.AreEqual(CheckValues[ChangeType.Delete.ToString()].Item2.DatetimeoffsetColumn.GetValueOrDefault().ToString("yyyyMMddhhmm"), CheckValues[ChangeType.Delete.ToString()].Item1.DatetimeoffsetColumn.GetValueOrDefault().ToString("yyyyMMddhhmm"));

            Assert.IsTrue(base.AreAllDbObjectDisposed(tableDependency.DataBaseObjectsNamingConvention));
            Assert.IsTrue(base.CountConversationEndpoints(tableDependency.DataBaseObjectsNamingConvention) == 0);
        }

        private void TableDependency_Changed(object sender, RecordChangedEventArgs<DateTypeTestModel> e)
        {
            switch (e.ChangeType)
            {
                case ChangeType.Insert:
                    CheckValues[ChangeType.Insert.ToString()].Item2.DateColumn = e.Entity.DateColumn;
                    CheckValues[ChangeType.Insert.ToString()].Item2.DatetimeColumn = e.Entity.DatetimeColumn;
                    CheckValues[ChangeType.Insert.ToString()].Item2.Datetime2Column = e.Entity.Datetime2Column;
                    CheckValues[ChangeType.Insert.ToString()].Item2.DatetimeoffsetColumn = e.Entity.DatetimeoffsetColumn;
                    break;
                case ChangeType.Update:
                    CheckValues[ChangeType.Update.ToString()].Item2.DateColumn = e.Entity.DateColumn;
                    CheckValues[ChangeType.Update.ToString()].Item2.DatetimeColumn = e.Entity.DatetimeColumn;
                    CheckValues[ChangeType.Update.ToString()].Item2.Datetime2Column = e.Entity.Datetime2Column;
                    CheckValues[ChangeType.Update.ToString()].Item2.DatetimeoffsetColumn = e.Entity.DatetimeoffsetColumn;
                    break;
                case ChangeType.Delete:
                    CheckValues[ChangeType.Delete.ToString()].Item2.DateColumn = e.Entity.DateColumn;
                    CheckValues[ChangeType.Delete.ToString()].Item2.DatetimeColumn = e.Entity.DatetimeColumn;
                    CheckValues[ChangeType.Delete.ToString()].Item2.Datetime2Column = e.Entity.Datetime2Column;
                    CheckValues[ChangeType.Delete.ToString()].Item2.DatetimeoffsetColumn = e.Entity.DatetimeoffsetColumn;
                    break;
            }
        }

        private static void ModifyTableContent()
        {
            CheckValues.Add(ChangeType.Insert.ToString(), new Tuple<DateTypeTestModel, DateTypeTestModel>(new DateTypeTestModel { DateColumn =  DateTime.Now.AddDays(-1).Date, DatetimeColumn = null, Datetime2Column = DateTime.Now.AddDays(-3), DatetimeoffsetColumn = DateTimeOffset.Now.AddDays(-4) }, new DateTypeTestModel()));
            CheckValues.Add(ChangeType.Update.ToString(), new Tuple<DateTypeTestModel, DateTypeTestModel>(new DateTypeTestModel { DateColumn = null, DatetimeColumn = DateTime.Now, Datetime2Column = null, DatetimeoffsetColumn = DateTime.Now }, new DateTypeTestModel()));
            CheckValues.Add(ChangeType.Delete.ToString(), new Tuple<DateTypeTestModel, DateTypeTestModel>(new DateTypeTestModel { DateColumn = null, DatetimeColumn = DateTime.Now, Datetime2Column = null, DatetimeoffsetColumn = DateTime.Now }, new DateTypeTestModel()));

            using (var sqlConnection = new SqlConnection(ConnectionStringForTestUser))
            {
                sqlConnection.Open();

                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = $"INSERT INTO [{TableName}] ([dateColumn], [datetimeColumn], [datetime2Column], [datetimeoffsetColumn]) VALUES(@dateColumn, NULL, @datetime2Column, @datetimeoffsetColumn)";
                    sqlCommand.Parameters.Add(new SqlParameter("@dateColumn", SqlDbType.Date) { Value = CheckValues[ChangeType.Insert.ToString()].Item1.DateColumn });
                    sqlCommand.Parameters.Add(new SqlParameter("@datetime2Column", SqlDbType.DateTime2) { Value = CheckValues[ChangeType.Insert.ToString()].Item1.Datetime2Column });
                    sqlCommand.Parameters.Add(new SqlParameter("@datetimeoffsetColumn", SqlDbType.DateTimeOffset) { Value = CheckValues[ChangeType.Insert.ToString()].Item1.DatetimeoffsetColumn });
                    sqlCommand.ExecuteNonQuery();
                }

                Thread.Sleep(1000);

                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = $"UPDATE [{TableName}] SET [dateColumn] = NULL, [datetimeColumn] = @datetimeColumn, [datetime2Column] = NULL, [datetimeoffsetColumn] = @datetimeoffsetColumn";
                    sqlCommand.Parameters.Add(new SqlParameter("@datetimeColumn", SqlDbType.DateTime) { Value = CheckValues[ChangeType.Update.ToString()].Item1.DatetimeColumn });
                    sqlCommand.Parameters.Add(new SqlParameter("@datetimeoffsetColumn", SqlDbType.DateTimeOffset) { Value = CheckValues[ChangeType.Update.ToString()].Item1.DatetimeoffsetColumn });
                    sqlCommand.ExecuteNonQuery();
                }

                Thread.Sleep(1000);

                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = $"DELETE FROM [{TableName}]";
                    sqlCommand.ExecuteNonQuery();
                }

                Thread.Sleep(1000);
            }
        }
    }
}