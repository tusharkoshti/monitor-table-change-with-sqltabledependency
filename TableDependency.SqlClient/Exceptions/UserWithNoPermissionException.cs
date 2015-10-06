﻿////////////////////////////////////////////////////////////////////////////////
//   TableDependency, SqlTableDependency, OracleTableDependency
//   Copyright (c) Christian Del Bianco.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////
using System;
using TableDependency.Exceptions;

namespace TableDependency.SqlClient.Exceptions
{
    public class UserWithNoPermissionException : TableDependencyException
    {
        protected internal UserWithNoPermissionException(Exception innerException = null)
            : base("User with no permission.", innerException)
        { }
    }
}