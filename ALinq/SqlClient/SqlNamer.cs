using System;
using System.Collections.Generic;

namespace ALinq.SqlClient
{
    internal class SqlNamer
    {
        // Fields
        private readonly Visitor visitor;
        private SqlProvider.ProviderMode mode;
        private ConstColumns c;

        public SqlNamer(SqlProvider.ProviderMode mode)
        {
            visitor = new Visitor(this);
            this.mode = mode;
            c = new ConstColumns(mode);
        }

        // Methods

        internal SqlNode AssignNames(SqlNode node)
        {
            return visitor.Visit(node);
        }

        internal string DiscoverName(SqlExpression e)
        {
            if (e != null)
            {
                string name;
                switch (e.NodeType)
                {
                    case SqlNodeType.Column:
                        name = DiscoverName(((SqlColumn)e).Expression);
                        return name;
                    case SqlNodeType.ColumnRef:
                        {
                            var ref2 = (SqlColumnRef)e;
                            if (ref2.Column.Name != null)
                            {
                                return ref2.Column.Name;
                            }
                            name = DiscoverName(ref2.Column);
                            return name;
                        }
                    case SqlNodeType.ExprSet:
                        {
                            var set = (SqlExprSet)e;
                            name = DiscoverName(set.Expressions[0]);
                            return name;
                        }
                }
            }
            return ConstColumns.GetValue(mode);
        }

        // Nested Types
        private class ColumnNameGatherer : SqlVisitor
        {
            // Fields

            // Methods
            public ColumnNameGatherer()
            {
                this.Names = new HashSet<string>();
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                if (!string.IsNullOrEmpty(col.Name))
                {
                    this.Names.Add(col.Name);
                }
                return base.VisitColumn(col);
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                this.Visit(cref.Column);
                return base.VisitColumnRef(cref);
            }

            // Properties
            public HashSet<string> Names { get; set; }
        }

        private class Visitor : SqlVisitor
        {
            // Fields
            private SqlAlias alias;
            private int aliasCount;
            private string lastName;
            private bool makeUnique = true;
            private bool useMappedNames = false;
            private SqlNamer sqlNamer;
            private ConstColumns c;

            // Methods
            internal Visitor(SqlNamer sqlNamer)
            {
                this.sqlNamer = sqlNamer;
                c = new ConstColumns(sqlNamer.mode);
            }

            private ICollection<string> GetColumnNames(IEnumerable<SqlOrderExpression> orderList)
            {
                var gatherer = new ColumnNameGatherer();
                foreach (SqlOrderExpression expression in orderList)
                {
                    gatherer.Visit(expression.Expression);
                }
                return gatherer.Names;
            }

            internal string GetNextAlias()
            {
                return ("t" + aliasCount++);
            }

            private static bool IsSimpleColumn(SqlColumn c, string name)
            {
                if (c.Expression != null)
                {
                    if (c.Expression.NodeType != SqlNodeType.ColumnRef)
                    {
                        return false;
                    }
                    var expression = c.Expression as SqlColumnRef;
                    if (!string.IsNullOrEmpty(name))
                    {
                        return (string.Compare(name, expression.Column.Name, StringComparison.OrdinalIgnoreCase) == 0);
                    }
                }
                return true;
            }

            private bool IsUniqueName(IList<SqlColumn> columns, ICollection<string> reservedNames, SqlColumn c,
                                      string name)
            {
                foreach (SqlColumn column in columns)
                {
                    if ((column != c) && (string.Compare(column.Name, name, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return false;
                    }
                }
                if (!IsSimpleColumn(c, name))
                {
                    return !reservedNames.Contains(name);
                }
                return true;
            }

            internal override SqlAlias VisitAlias(SqlAlias sqlAlias)
            {
                SqlAlias alias = this.alias;
                this.alias = sqlAlias;
                sqlAlias.Node = this.Visit(sqlAlias.Node);
                sqlAlias.Name = this.GetNextAlias();
                this.alias = alias;
                return sqlAlias;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                if ((cref.Column.Name == null) && (this.lastName != null))
                {
                    cref.Column.Name = this.lastName;
                }
                return cref;
            }

            internal override SqlExpression VisitExpression(SqlExpression expr)
            {
                SqlExpression expression;
                string lastName = this.lastName;
                this.lastName = null;
                try
                {
                    expression = (SqlExpression)this.Visit(expr);
                }
                finally
                {
                    this.lastName = lastName;
                }
                return expression;
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g)
            {
                //if (sqlNamer.mode == SqlProvider.ProviderMode.Firebird ||
                //    sqlNamer.mode == SqlProvider.ProviderMode.Oracle)
                //{
                //    g.Key = VisitNamedExpression(g.Key, "\"Key\"");
                //    g.Group = this.VisitNamedExpression(g.Group, "\"Group\"");
                //}
                //else
                //{
                g.Key = this.VisitNamedExpression(g.Key, ConstColumns.Key);//"Key");
                g.Group = this.VisitNamedExpression(g.Group, ConstColumns.Group);// "Group");
                //}
                return g;
            }

            internal override SqlStatement VisitInsert(SqlInsert insert)
            {
                bool makeUnique = this.makeUnique;
                this.makeUnique = false;
                bool useMappedNames = this.useMappedNames;
                this.useMappedNames = true;
                SqlStatement statement = base.VisitInsert(insert);
                this.makeUnique = makeUnique;
                this.useMappedNames = useMappedNames;
                return statement;
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                mc.Object = this.VisitExpression(mc.Object);
                System.Reflection.ParameterInfo[] parameters = mc.Method.GetParameters();
                int index = 0;
                int count = mc.Arguments.Count;
                while (index < count)
                {
                    mc.Arguments[index] = this.VisitNamedExpression(mc.Arguments[index], parameters[index].Name);
                    index++;
                }
                return mc;
            }

            private SqlExpression VisitNamedExpression(SqlExpression expr, string name)
            {
                SqlExpression expression;
                string lastName = this.lastName;
                this.lastName = name;
                try
                {
                    expression = (SqlExpression)this.Visit(expr);
                }
                finally
                {
                    this.lastName = lastName;
                }
                return expression;
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                if (sox.Constructor != null)
                {
                    System.Reflection.ParameterInfo[] parameters = sox.Constructor.GetParameters();
                    int index = 0;
                    int count = sox.Args.Count;
                    while (index < count)
                    {
                        sox.Args[index] = this.VisitNamedExpression(sox.Args[index], parameters[index].Name);
                        index++;
                    }
                }
                else
                {
                    int num3 = 0;
                    int num4 = sox.Args.Count;
                    while (num3 < num4)
                    {
                        sox.Args[num3] = this.VisitExpression(sox.Args[num3]);
                        num3++;
                    }
                }
                foreach (SqlMemberAssign assign in sox.Members)
                {
                    string name = assign.Member.Name;
                    if (this.useMappedNames)
                    {
                        name = sox.MetaType.GetDataMember(assign.Member).MappedName;
                    }
                    assign.Expression = this.VisitNamedExpression(assign.Expression, name);
                }
                return sox;
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov)
            {
                sov.HasValue = this.VisitNamedExpression(sov.HasValue, ConstColumns.Test);
                sov.Value = this.VisitExpression(sov.Value);
                return sov;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                base.VisitScalarSubSelect(ss);
                if (ss.Select.Row.Columns.Count > 0)
                {
                    ss.Select.Row.Columns[0].Name = "";
                }
                return ss;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select = base.VisitSelect(select);
                string[] strArray = new string[select.Row.Columns.Count];
                int index = 0;
                int length = strArray.Length;
                while (index < length)
                {
                    SqlColumn e = select.Row.Columns[index];
                    string name = e.Name;
                    if (name == null)
                    {
                        name = sqlNamer.DiscoverName(e);
                    }
                    strArray[index] = name;
                    e.Name = null;
                    index++;
                }
                ICollection<string> columnNames = this.GetColumnNames(select.OrderBy);
                int num3 = 0;
                int count = select.Row.Columns.Count;
                while (num3 < count)
                {
                    SqlColumn c = select.Row.Columns[num3];
                    string str2 = strArray[num3];
                    string str3 = str2;
                    if (this.makeUnique)
                    {
                        int num5 = 1;
                        while (!this.IsUniqueName(select.Row.Columns, columnNames, c, str3))
                        {
                            num5++;
                            str3 = str2 + num5;
                        }
                    }
                    c.Name = str3;
                    c.Ordinal = num3;
                    num3++;
                }
                return select;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate update)
            {
                bool makeUnique = this.makeUnique;
                this.makeUnique = false;
                bool useMappedNames = this.useMappedNames;
                this.useMappedNames = true;
                SqlStatement statement = base.VisitUpdate(update);
                this.makeUnique = makeUnique;
                this.useMappedNames = useMappedNames;
                return statement;
            }
        }
    }
}