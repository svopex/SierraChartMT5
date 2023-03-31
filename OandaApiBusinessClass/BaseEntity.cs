using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace OandaApiBusinessClass
{
    [AttributeUsage(AttributeTargets.All)]
    public class BaseAttribute : System.Attribute
    {
        public const int DefaultSearchGroup = 1;
        public bool UsedInSql { get; set; }
        public int SearchGroup { get; set; }
        public bool ReverseOrder { get; set; }
    }

    public class BaseEntity
    {
        public const int NEW_OBJECT_ID = 0;

        public int Id { get; set; }

        /*
        public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            MemberExpression me = propertyLambda.Body as MemberExpression;
            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            string result = string.Empty;
            do
            {
                result = me.Member.Name + "." + result;
                me = me.Expression as MemberExpression;
            } while (me != null);

            result = result.Remove(result.Length - 1); // remove the trailing "."
            return result;
        }
        */

        private PropertyInfo GetPropertyByName(string name)
        {
            foreach (PropertyInfo propertyInfo in GetType().GetProperties())
            {
                object[] attrs = propertyInfo.GetCustomAttributes(typeof(BaseAttribute), true);
                foreach (BaseAttribute BaseAttribute in attrs)
                {
                    if (BaseAttribute.UsedInSql)
                    {
                        if (propertyInfo.Name == name)
                            return propertyInfo;
                    }
                }
            }
            return null;
        }

        private object GetValue(PropertyInfo propertyInfo, BaseEntity item)
        {
            if (propertyInfo.GetValue(item, null) == null)
            {
                return null;
            }
            else if (propertyInfo.PropertyType.IsEnum
                || (
                    propertyInfo.PropertyType.IsGenericType
                    && Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null
                    && Nullable.GetUnderlyingType(propertyInfo.PropertyType).IsEnum
                ))
                {
                    string enumValue = propertyInfo.GetValue(item, null).ToString().Replace("_", "-");
                    return enumValue;
                }
            else
            {
                return propertyInfo.GetValue(item, null);
            }
        }


        public bool IsSame(BaseEntity item)
        {
            if (item == null)
                return false;

            foreach (PropertyInfo propertyInfoItem1 in item.GetType().GetProperties())
            {
                object[] attrs = propertyInfoItem1.GetCustomAttributes(typeof(BaseAttribute), true);
                foreach (BaseAttribute BaseAttribute in attrs)
                {
                    if (BaseAttribute.UsedInSql)
                    {
                        PropertyInfo propertyInfoItem2 = GetPropertyByName(propertyInfoItem1.Name);
                        if (!GetValue(propertyInfoItem1, item).Equals(GetValue(propertyInfoItem2, this)))
                            return false;
                    }
                }
            }
            return true;
        }
    }

    public class BaseEntities<T> : List<T>
    {
        public BaseEntities()
        {
        }

        public BaseEntities(T[] itemArray)
        {
            AddRange(itemArray);
        }

        public BaseEntities(List<T> items)
        {
            AddRange(items);
        }

        public BaseEntities<T> SortList(string ListSortField, bool ListSortAsc)
        {
            BaseEntities<T> list = new BaseEntities<T>();
            if (!string.IsNullOrEmpty(ListSortField)
                && this.Count() > 0)
            {                
                Type t = typeof(T);
                if (ListSortAsc)
                {
                    list = new BaseEntities<T>(this.OrderBy(a => t.InvokeMember(ListSortField, System.Reflection.BindingFlags.GetProperty, null, a, null)).ToList());
                }
                else
                {
                    list = new BaseEntities<T>(this.OrderByDescending(a => t.InvokeMember(ListSortField, System.Reflection.BindingFlags.GetProperty, null, a, null)).ToList());
                }
            }
            return list;
        }
    }
}
