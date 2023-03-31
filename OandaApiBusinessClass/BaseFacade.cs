using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Reflection;
using MySql.Data.MySqlClient;
using System.Linq.Expressions;

namespace OandaApiBusinessClass
{
    public class BaseFacade
    {
        public MySqlConnection Connection;
        public MySqlTransaction Transaction;

        public BaseFacade(string tableName, MySqlConnection connection, MySqlTransaction transaction)
        {
            TableName = tableName;
            Connection = connection;
            Transaction = transaction;
        }

        public BaseFacade()
        {
            Connection = null;
            Transaction = null;
        }

        public BaseFacade(string tableName) : this()
        {
            TableName = tableName;
        }

        public class SortSetting
        {
            public string ColumnName;
            public bool Asc;
        }
        public class SortSettings : List<BaseFacade.SortSetting>
        {
        }

        public string TableName 
        {
            get
            {
                return "`" + tableName + "`";
            }
            set
            {
                tableName = value;
                if (tableName.Length > 0 && tableName[0] == '`')
                {
                    tableName = tableName.Remove(0, 1);
                }
                if (tableName.Length > 0 && tableName[tableName.Length - 1] == '`')
                {
                    tableName = tableName.Remove(tableName.Length - 1, 1);
                }
            }
        }
        public string tableName;

        public MySqlConnection OpenConnection()
        {
            return GetConnection();
        }

        public MySqlConnection GetConnection()
        {
            if (Connection == null || Connection.State != System.Data.ConnectionState.Open)
            {
                string s = ConfigurationManager.ConnectionStrings["SQLConnectionString"].ConnectionString;
                Connection = new MySqlConnection(s);
                Connection.Open();
            }
            return Connection;
        }

        public MySqlConnection GetConnectionAndOpenTransaction()
        {
            GetConnection();
            Transaction = Connection.BeginTransaction();
            return Connection;
        }

        public void CloseConnectionTransactionCommit()
        {
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
            CloseConnection();
        }

        public void CloseConnectionTransactionRollback()
        {
            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
            CloseConnection();
        }

        public void CloseConnection()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }

        public void InitializeConnection(ref MySqlConnection conn, ref MySqlTransaction transaction)
        {
            if (conn == null) conn = Connection;
            if (transaction == null) transaction = Transaction;
            if (conn == null) conn = GetConnection();
        }

        public MySqlCommand GetMySqlCommand(string sql, MySqlConnection conn = null, MySqlTransaction transaction = null)
        {
            InitializeConnection(ref conn, ref transaction);
            MySqlCommand MySqlCommand = new MySqlCommand();
            MySqlCommand.Connection = conn;
            if (transaction != null)
                MySqlCommand.Transaction = transaction;
            MySqlCommand.CommandText = sql;
            //MySqlCommand.CommandTimeout = int.MaxValue;
            return MySqlCommand;
        }
    }
    
    public class BaseFacade<T> : BaseFacade
    {
        public BaseFacade(string tableName, MySqlConnection connection, MySqlTransaction transaction)
            : base(tableName, connection, transaction)
        {
        }

        public BaseFacade(string tableName)
            : base(tableName)
        {
        }

        protected int GetLastId(string tableName = null)
        {
            if (string.IsNullOrEmpty(tableName))
                tableName = TableName;
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"SELECT MAX(Id) FROM {0}", tableName));
            MySqlDataReader r = cmd.ExecuteReader();
            if (r.Read())
            {
                int maxId = Convert.ToInt32(r["MAX(Id)"]);
                r.Close();
                CloseConnection();
                return maxId;
            }
            else
            {
                r.Close();
                CloseConnection();
                return BaseEntity.NEW_OBJECT_ID;
            }
        }

        public virtual T GetParameters(MySqlDataReader r)
        {
            T item = (T)Activator.CreateInstance(typeof(T));
            foreach (PropertyInfo propertyInfo in item.GetType().GetProperties())
            {
                object[] attrs = propertyInfo.GetCustomAttributes(typeof(BaseAttribute), true);
                foreach (BaseAttribute BaseAttribute in attrs)
                {
                    if (BaseAttribute.UsedInSql)
                    {
                        if (r[propertyInfo.Name] == DBNull.Value)
                            propertyInfo.SetValue(item, null, null);
                        else
                        {
                            if (propertyInfo.PropertyType.IsEnum)
                            {
                                //string data = Convert.ToString(r[propertyInfo.Name]).Replace("-", "_");
                                string data = Convert.ToString(r[propertyInfo.Name]);
                                propertyInfo.SetValue(item, Enum.Parse(propertyInfo.PropertyType, data, true), null);
                            }
                            else if (propertyInfo.PropertyType.IsGenericType
                                    && Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null
                                    && Nullable.GetUnderlyingType(propertyInfo.PropertyType).IsEnum)
                            {
                                //string data = Convert.ToString(r[propertyInfo.Name]).Replace("-", "_");
                                string data = Convert.ToString(r[propertyInfo.Name]);
                                propertyInfo.SetValue(item, Enum.Parse(Nullable.GetUnderlyingType(propertyInfo.PropertyType), data, true), null);
                            }
                            else if (propertyInfo.PropertyType == typeof(Nullable<decimal>) || propertyInfo.PropertyType == typeof(decimal))
                            {
                                propertyInfo.SetValue(item, BaseModelTools.ToDecimal(r[propertyInfo.Name]), null);
                            }
                            else if (propertyInfo.PropertyType == typeof(Nullable<DateTime>) || propertyInfo.PropertyType == typeof(DateTime))
                            {
                                propertyInfo.SetValue(item, BaseModelTools.ToDateTime(r[propertyInfo.Name]), null);
                            }
                            else if (propertyInfo.PropertyType == typeof(Nullable<int>) || propertyInfo.PropertyType == typeof(int))
                            {
                                propertyInfo.SetValue(item, Convert.ToInt32(r[propertyInfo.Name]), null);
                            }
                            else if (propertyInfo.PropertyType == typeof(Nullable<long>) || propertyInfo.PropertyType == typeof(long))
                            {
                                propertyInfo.SetValue(item, Convert.ToInt64(r[propertyInfo.Name]), null);
                            }
                            else if (propertyInfo.PropertyType == typeof(Nullable<bool>) || propertyInfo.PropertyType == typeof(bool))
                            {
                                propertyInfo.SetValue(item, Convert.ToBoolean(r[propertyInfo.Name]), null);
                            }
                            else if (propertyInfo.PropertyType == typeof(string))
                            {
                                propertyInfo.SetValue(item, Convert.ToString(r[propertyInfo.Name]), null);
                            }
                            else
                            {
                                propertyInfo.SetValue(item, r[propertyInfo.Name], null);
                            }
                        }
                    }
                }
            }
            BaseEntity baseEntityItem = (BaseEntity)(object)item;
            if (BaseModelTools.ColumnExists(r, "Id"))
                baseEntityItem.Id = Convert.ToInt32(r["Id"]);
            return item;
        }

        public virtual void SetParameters(MySqlCommand cmd, T item)
        {
            foreach (PropertyInfo propertyInfo in item.GetType().GetProperties())
            {
                object[] attrs = propertyInfo.GetCustomAttributes(typeof(BaseAttribute), true);
                foreach (BaseAttribute BaseAttribute in attrs)
                {
                    if (BaseAttribute.UsedInSql)
                    {
                        if (propertyInfo.GetValue(item, null) == null)
                        {
                            cmd.Parameters.Add(new MySqlParameter("@" + propertyInfo.Name, DBNull.Value));
                        }
                        else if (propertyInfo.PropertyType.IsEnum
                            || (
                                propertyInfo.PropertyType.IsGenericType
                                && Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null
                                && Nullable.GetUnderlyingType(propertyInfo.PropertyType).IsEnum)
                            )
                        {                            
                            //string enumValue = propertyInfo.GetValue(item, null).ToString().Replace("_", "-");
                            string enumValue = propertyInfo.GetValue(item, null).ToString();
                            cmd.Parameters.Add(new MySqlParameter("@" + propertyInfo.Name, enumValue));
                        }
                        else
                        {
                            cmd.Parameters.Add(new MySqlParameter("@" + propertyInfo.Name, propertyInfo.GetValue(item, null)));
                        }
                    }
                }
            }
        }

        private List<string> GetColumnNames(T item)
        {
            List<string> columnNames = new List<string>();
            foreach (PropertyInfo propertyInfo in item.GetType().GetProperties())
            {
                object[] attrs = propertyInfo.GetCustomAttributes(typeof(BaseAttribute), true);
                foreach (BaseAttribute baseDataAttibute in attrs)
                {
                    if (baseDataAttibute.UsedInSql)
                        columnNames.Add(propertyInfo.Name);
                }
            }
            return columnNames;
        }

        public virtual int SaveOnly(T item, MySqlConnection connection = null, MySqlTransaction transaction = null)
        {
            InitializeConnection(ref connection, ref transaction);
            List<string> columnNames = GetColumnNames(item);
            BaseEntity itemAsBaseData = (BaseEntity)(object)item;
            if (itemAsBaseData.Id == BaseEntity.NEW_OBJECT_ID)
            {
                // insert new data
                string columnNamesAsString = string.Empty;
                string variableNamesAsString = string.Empty;
                foreach (string columnName in columnNames)
                {
                    if (!string.IsNullOrEmpty(columnNamesAsString))
                        columnNamesAsString += ", ";
                    columnNamesAsString += "`" + columnName + "`";
                    if (!string.IsNullOrEmpty(variableNamesAsString))
                        variableNamesAsString += ", ";
                    variableNamesAsString += "@" + columnName;
                }

                MySqlCommand cmdInsert = GetMySqlCommand(string.Format(@"
                    INSERT INTO {0}({1}) VALUES({2});",
                    TableName, columnNamesAsString, variableNamesAsString), connection, transaction);
                SetParameters(cmdInsert, item);
                cmdInsert.ExecuteScalar();
                itemAsBaseData.Id = (int)cmdInsert.LastInsertedId;
            }
            else
            {
                // update old data
                string s = string.Empty;
                foreach (string columnName in columnNames)
                {
                    if (!string.IsNullOrEmpty(s))
                        s += ", ";
                    s += "`" + columnName + "`" + " = @" + columnName;
                }

                MySqlCommand cmdInsert = GetMySqlCommand(string.Format(@" 
                    UPDATE {0} SET {1} WHERE Id = @Id",
                    TableName, s), connection, transaction);
                SetParameters(cmdInsert, item);
                cmdInsert.Parameters.Add(new MySqlParameter("@Id", itemAsBaseData.Id));
                cmdInsert.ExecuteNonQuery();
            }
            return itemAsBaseData.Id;
        }

        public virtual int Save(T item)
        {
            int id = BaseEntity.NEW_OBJECT_ID;
            MySqlConnection conn = GetConnectionAndOpenTransaction();
            try
            {
                id = SaveOnly(item);
                CloseConnectionTransactionCommit();
            }
            catch (Exception)
            {
                CloseConnectionTransactionRollback();
                throw;
            }
            return id;
        }

        public T LoadLastByColumn(string columnName, Nullable<int> x)
        {
            T item = default(T);
            OpenConnection();
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"SELECT * FROM " + TableName + " WHERE {0} = @{0} ORDER BY ID DESC", columnName));
            if (x == null)
                cmd.Parameters.Add(new MySqlParameter("@" + columnName, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter("@" + columnName, x.Value));
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    item = GetParameters(r);
                }
            }
            CloseConnection();
            return item;
        }

        public T LoadByColumnOnly(string columnName, Nullable<long> x)
        {
            return LoadByColumnEx(columnName, x, false);
        }

        public T LoadByColumn(string columnName, Nullable<long> x)
        {
            return LoadByColumnEx(columnName, x, true);
        }

        public T LoadByColumnEx(string columnName, Nullable<long> x, bool closeConnection)
        {
            T item = default(T);
            OpenConnection();
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"SELECT * FROM " + TableName + " WHERE {0} = @{0}", columnName));
            if (x == null)
                cmd.Parameters.Add(new MySqlParameter("@" + columnName, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter("@" + columnName, x.Value));
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    item = GetParameters(r);
                }
            }
            if (closeConnection) CloseConnection();
            return item;
        }

        public T LoadByColumn(string columnName, DateTime dt)
        {
            T item = default(T);
            OpenConnection();
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"SELECT * FROM " + TableName + " WHERE `{0}` = @{0}", columnName));
            cmd.Parameters.Add(new MySqlParameter("@" + columnName, dt));
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    item = GetParameters(r);
                }
            }
            CloseConnection();
            return item;
        }

        public T LoadByColumn(string columnName, string text)
        {
            if (string.IsNullOrEmpty(text))
                return default(T);

            T item = default(T);
            OpenConnection();
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"SELECT * FROM " + TableName + " WHERE `{0}` = @{0}", columnName));
            cmd.Parameters.Add(new MySqlParameter("@" + columnName, text));
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    item = GetParameters(r);
                }
            }
            CloseConnection();
            return item;
        }

        public virtual T LoadById(Nullable<int> id)
        {
            return LoadByColumn("Id", id);
        }

        public virtual T LoadByIdOnly(Nullable<int> id)
        {
            return LoadByColumnOnly("Id", id);
        }

        public bool ExistRowByColumn(string columnName, int x)
        {
            bool exist = false;
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"SELECT * FROM {0} WHERE {1} = @{1}", TableName, columnName));
            cmd.Parameters.Add(new MySqlParameter("@" + columnName, x));
            using (MySqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                    exist = true;
            CloseConnection();
            return exist;
        }

        public virtual List<T> LoadMultipleByColumn(string columnName, int x)
        {
            List<T> items = new List<T>();
            OpenConnection();
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"SELECT * FROM {0} WHERE {1} = @{1}", TableName, columnName));
            cmd.Parameters.Add(new MySqlParameter("@" + columnName, x));
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    T item = default(T);
                    item = GetParameters(r);
                    items.Add(item);
                }
            }
            CloseConnection();
            return items;
        }

        public List<T> LoadMultipleByColumn(string columnName, string s)
        {
            List<T> items = new List<T>();
            OpenConnection();
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"SELECT * FROM {0} WHERE {1} = @{1}", TableName, columnName));
            cmd.Parameters.Add(new MySqlParameter("@" + columnName, s));
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    T item = default(T);
                    item = GetParameters(r);
                    items.Add(item);
                }
            }
            CloseConnection();
            return items;
        }

        public List<T> LoadMultipleByIds(List<int> ids)
        {
            List<T> items = new List<T>();
            if (ids.Count == 0)
                return items;

            // create column names
            int counter = 0;
            string sIdNames = string.Empty;
            foreach (int id in ids)
            {
                if (!string.IsNullOrEmpty(sIdNames))
                    sIdNames += ",";
                sIdNames += "@ID_" + counter.ToString();
                counter++;
            }

            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"SELECT * FROM {0} WHERE Id IN ({1})", TableName, sIdNames));
            counter = 0;
            foreach (int id in ids)
            {
                cmd.Parameters.Add(new MySqlParameter("@ID_" + counter.ToString(), id));
                counter++;
            }
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    T item = default(T);
                    item = GetParameters(r);
                    items.Add(item);
                }
            }
            CloseConnection();
            return items;
        }

        public void DeleteByIdOnly(int id, MySqlConnection connection = null, MySqlTransaction transaction = null)
        {
            DeleteByColumnOnly("Id", id, connection, transaction);
        }

        public void DeleteById(int id)
        {
            DeleteByColumn("Id", id);
        }

        public void DeleteByColumnOnly(string columnName, int x, MySqlConnection connection = null, MySqlTransaction transaction = null)
        {
            InitializeConnection(ref connection, ref transaction); 
            MySqlCommand cmdDelete = GetMySqlCommand(string.Format("DELETE FROM {0} WHERE {1} = @{1}", TableName, columnName), connection, transaction);
            cmdDelete.Parameters.Add(new MySqlParameter("@" + columnName, x));
            cmdDelete.ExecuteNonQuery();
        }

        public void DeleteByColumn(string columnName, int x)
        {
            MySqlConnection conn = GetConnection();
            DeleteByColumnOnly(columnName, x);
            CloseConnection();
        }

        protected List<TT> SortList<TT>(List<TT> list, string ListSortField, bool ListSortAsc)
        {
            if (!string.IsNullOrEmpty(ListSortField)
                && list.Count() > 0)
            {
                Type t = list[0].GetType();
                if (ListSortAsc)
                {
                    list = list.OrderBy(a => t.InvokeMember(ListSortField, System.Reflection.BindingFlags.GetProperty, null, a, null)).ToList();
                }
                else
                {
                    list = list.OrderByDescending(a => t.InvokeMember(ListSortField, System.Reflection.BindingFlags.GetProperty, null, a, null)).ToList();
                }
            }
            return list;
        }

        public virtual int CountOfRows()
        {
            int CountOfRows = -1;
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = GetMySqlCommand(string.Format(@"select Count(*) as CountOfRows from {0}", TableName));
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    CountOfRows = Convert.ToInt32(r["CountOfRows"]);
                }
                else
                {
                    throw new Exception();
                }
            }
            CloseConnection();
            return CountOfRows;
        }

        public virtual List<T> Load()
        {
            return Load(string.Empty);
        }

        public virtual List<T> Load(string searchString)
        {
            return Load(searchString, new List<SortSetting>());
        }

        public virtual List<T> Load(string searchString, List<SortSetting> sortSettings)
        {
            return Load(searchString, sortSettings, new List<int>() { BaseAttribute.DefaultSearchGroup });
        }

        public static void AddSearchTextBoolean(ref string searchSQL, string searchString, string colName, bool addOr)
        {
            if (!string.IsNullOrEmpty(searchString) && searchString.IndexOf("ano", StringComparison.OrdinalIgnoreCase) != -1)
            {
                searchSQL += (addOr ? " OR " : "") + colName + " = '1' ";
                return;
            }
            if (!string.IsNullOrEmpty(searchString) && searchString.IndexOf("ne", StringComparison.OrdinalIgnoreCase) != -1)
            {
                searchSQL += (addOr ? " OR " : "") + colName + " = '0' ";
                return;
            }
            if (!addOr)
                searchSQL += " ( 1 != 1) ";
        }

        public virtual List<T> Load(string searchString, List<SortSetting> sortSettings, List<int> searchGroups)
        {
            List<T> items = (List<T>)Activator.CreateInstance(typeof(List<T>));
            MySqlConnection conn = GetConnection();

            // create search string
            string searchSQL = string.Empty;
            bool first = true;
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                object[] attrs = propertyInfo.GetCustomAttributes(typeof(BaseAttribute), true);
                foreach (BaseAttribute baseDataAttibute in attrs)
                {
                    if (searchGroups.Contains(baseDataAttibute.SearchGroup))
                    {
                        if (first)
                            searchSQL += " ( ";
                        else
                            searchSQL += " OR ";
                        if (propertyInfo.PropertyType == typeof(bool))
                        {
                            AddSearchTextBoolean(ref searchSQL, searchString, propertyInfo.Name, false);
                        }
                        else
                        {
                            searchSQL += String.Format(" `{0}` LIKE '%{1}%' ", propertyInfo.Name, searchString);
                        }
                        first = false;
                    }
                }
            }
            if (string.IsNullOrEmpty(searchSQL))
                searchSQL = String.Format(" (1 = 1) ");
            else
                searchSQL += " ) ";

            string sql = string.Format(@" SELECT * FROM {0} WHERE ", TableName);

            string sortSQL = string.Empty;
            foreach (SortSetting searchSetting in sortSettings)
            {
                if (string.IsNullOrEmpty(sortSQL))
                    sortSQL += " ORDER BY ";
                else
                    sortSQL += ", ";
                sortSQL += string.Format("`{0}`", searchSetting.ColumnName);
                bool asc = searchSetting.Asc;
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    object[] attrs = propertyInfo.GetCustomAttributes(typeof(BaseAttribute), true);
                    foreach (BaseAttribute baseDataAttibute in attrs)
                    {
                        if (propertyInfo.Name == searchSetting.ColumnName && baseDataAttibute.ReverseOrder)
                        {
                            asc = !asc;
                            break;
                        }
                    }
                }
                if (asc)
                    sortSQL += " ASC";
                else
                    sortSQL += " DESC";
            }
            sortSQL += " ";
            MySqlCommand cmd = GetMySqlCommand(sql + searchSQL + sortSQL);
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    T item = GetParameters(r);
                    items.Add(item);
                }
            }
            CloseConnection();
            return items;
        }

        public virtual int UpdateByColumnOnly<TT>(int id, string columnName, TT x)
        {
            if (id == BaseEntity.NEW_OBJECT_ID)
                throw new Exception();
            else
            {
                MySqlCommand cmdUpdate = GetMySqlCommand(string.Format(@"
                    UPDATE {0} SET `{1}` = @X WHERE Id = @Id",
                    TableName, columnName));
                if (x == null)
                    cmdUpdate.Parameters.Add(new MySqlParameter("@X", DBNull.Value));
                else
                    cmdUpdate.Parameters.Add(new MySqlParameter("@X", x));
                cmdUpdate.Parameters.Add(new MySqlParameter("@Id", id));
                cmdUpdate.ExecuteNonQuery();
                return id;
            }
        }

        public virtual int UpdateByColumn<TT>(int id, string columnName, TT x)
        {
            MySqlConnection conn = GetConnectionAndOpenTransaction();
            try
            {
                id = UpdateByColumnOnly(id, columnName, x);
                CloseConnectionTransactionCommit();
            }
            catch (Exception)
            {
                CloseConnectionTransactionRollback();
                throw;
            }
            return id;
        }

        /*var x = new ObjectType();
        // note that in this case we don't need to specify types of x and Property1
        var propName1 = GetPropertyName(() => x.Property1);
        // assumes Property2 is an int property
        var propName2 = GetPropertyName<ObjectType, int>(y => y.Property2);
        // requires only object type
        var propName3 = GetPropertyName<ObjectType>(y => y.Property3);*/

        // requires object instance, but you can skip specifying T
        public static string GetPropertyName<TT>(Expression<Func<TT>> exp)
        {
            return (((MemberExpression)(exp.Body)).Member).Name;
        }

        // requires explicit specification of both object type and property type
        public static string GetPropertyName<TObject, TResult>(Expression<Func<TObject, TResult>> exp)
        {
            // extract property name
            return (((MemberExpression)(exp.Body)).Member).Name;
        }

        // requires explicit specification of object type
        public static string GetPropertyName<TObject>(Expression<Func<TObject, object>> exp)
        {
            var body = exp.Body;
            var convertExpression = body as UnaryExpression;
            if (convertExpression != null)
            {
                if (convertExpression.NodeType != ExpressionType.Convert)
                {
                    throw new ArgumentException("Invalid property expression.", "exp");
                }
                body = convertExpression.Operand;
            }
            return ((MemberExpression)body).Member.Name;
        }

        public string CreateColumnNames(HashSet<long> ids)
        {
            int counter = 0;
            string sIdNames = string.Empty;
            foreach (long id in ids)
            {
                if (!string.IsNullOrEmpty(sIdNames))
                    sIdNames += ",";
                sIdNames += "@I" + counter.ToString();
                counter++;
            }
            return sIdNames;
        }

        public void AddColumnParameters(MySqlCommand cmd, HashSet<long> ids)
        {
            int counter = 0;
            foreach (long id in ids)
            {
                cmd.Parameters.Add(new MySqlParameter("@I" + counter.ToString(), id));
                counter++;
            }
        }
    }
}
