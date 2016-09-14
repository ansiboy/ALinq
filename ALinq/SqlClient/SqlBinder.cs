using System;
using System.Collections.Generic;
using ALinq;
using ALinq.Mapping;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ALinq.SqlClient
{
    internal class SqlBinder
    {
        // Fields
        private SqlColumnizer columnizer;
        private bool optimizeLinkExpansions = true;
        private Func<SqlNode, SqlNode> prebinder;
        private bool simplifyCaseStatements = true;
        private SqlFactory sql;
        private Visitor visitor;
        private ConstColumns c;

        // Methods
        internal SqlBinder(Translator translator, SqlFactory sqlFactory, MetaModel model,
                           DataLoadOptions shape, SqlColumnizer columnizer)
        {
            this.sql = sqlFactory;
            this.columnizer = columnizer;
            this.visitor = new Visitor(this, translator, this.columnizer, this.sql, model, shape);
            c = new ConstColumns(translator.Provider.Mode);
        }

        internal SqlNode Bind(SqlNode node)
        {
            node = this.Prebind(node);
            node = this.visitor.Visit(node);
            return node;
        }

        private SqlNode Prebind(SqlNode node)
        {
            if (this.prebinder != null)
            {
                node = this.prebinder(node);
            }
            return node;
        }

        // Properties
        internal bool OptimizeLinkExpansions
        {
            get
            {
                return this.optimizeLinkExpansions;
            }
            set
            {
                this.optimizeLinkExpansions = value;
            }
        }

        internal Func<SqlNode, SqlNode> PreBinder
        {
            get
            {
                return this.prebinder;
            }
            set
            {
                this.prebinder = value;
            }
        }

        internal bool SimplifyCaseStatements
        {
            get
            {
                return this.simplifyCaseStatements;
            }
            set
            {
                this.simplifyCaseStatements = value;
            }
        }

        // Nested Types
        private class LinkOptimizationScope
        {
            // Fields
            private Dictionary<object, SqlExpression> map;
            private SqlBinder.LinkOptimizationScope previous;

            // Methods
            internal LinkOptimizationScope(SqlBinder.LinkOptimizationScope previous)
            {
                this.previous = previous;
            }

            internal void Add(object linkId, SqlExpression expr)
            {
                if (this.map == null)
                {
                    this.map = new Dictionary<object, SqlExpression>();
                }
                this.map.Add(linkId, expr);
            }

            internal bool TryGetValue(object linkId, out SqlExpression expr)
            {
                expr = null;
                return (((this.map != null) && this.map.TryGetValue(linkId, out expr)) || ((this.previous != null) && this.previous.TryGetValue(linkId, out expr)));
            }
        }

        private class Visitor : SqlVisitor
        {
            // Fields
            private SqlAggregateChecker aggregateChecker;
            private HashSet<MetaType> alreadyIncluded;
            private SqlBinder binder;
            private SqlColumnizer columnizer;
            private SqlAlias currentAlias;
            private SqlSelect currentSelect;
            private bool disableInclude;
            private SqlExpander expander;
            private bool inGroupBy;
            private SqlBinder.LinkOptimizationScope linkMap;
            private MetaModel model;
            private Dictionary<SqlAlias, SqlAlias> outerAliasMap;
            private DataLoadOptions shape;
            private SqlFactory sql;
            private Translator translator;
            private ITypeSystemProvider typeProvider;
            private ConstColumns c;

            // Methods
            internal Visitor(SqlBinder binder, Translator translator, SqlColumnizer columnizer, SqlFactory sqlFactory, MetaModel model, DataLoadOptions shape)
            {
                this.binder = binder;
                this.translator = translator;
                this.columnizer = columnizer;
                this.sql = sqlFactory;
                this.typeProvider = sqlFactory.TypeProvider;
                this.expander = new SqlExpander(this.sql);
                this.aggregateChecker = new SqlAggregateChecker();
                this.linkMap = new SqlBinder.LinkOptimizationScope(null);
                this.outerAliasMap = new Dictionary<SqlAlias, SqlAlias>();
                this.model = model;
                this.shape = shape;
                c = new ConstColumns(translator.Provider.Mode);
            }

            private SqlNode AccessMember(SqlMember m, SqlExpression expo)
            {
                SqlNew typeBinding;
                SqlValue value2;
                Func<MetaDataMember, bool> predicate = null;
                SqlExpression expr = expo;
                switch (expr.NodeType)
                {
                    case SqlNodeType.Element:
                    case SqlNodeType.ScalarSubSelect:
                        {
                            var select = (SqlSubSelect)expr;
                            var alias = new SqlAlias(select.Select);
                            var selection = new SqlAliasRef(alias);
                            SqlSelect currentSelect = this.currentSelect;
                            try
                            {
                                var select3 = new SqlSelect(selection, alias, select.SourceExpression);
                                this.currentSelect = select3;
                                SqlNode node2 = this.Visit(this.sql.Member(selection, m.Member));
                                SqlExpression expression10 = node2 as SqlExpression;
                                if (expression10 != null)
                                {
                                    select3.Selection = this.ConvertLinks(expression10);
                                    SqlNodeType nt = ((expression10 is SqlTypeCase) || !expression10.SqlType.CanBeColumn) ? SqlNodeType.Element : SqlNodeType.ScalarSubSelect;
                                    SqlSubSelect ss = this.sql.SubSelect(nt, select3);
                                    return this.FoldSubquery(ss);
                                }
                                SqlSelect select5 = node2 as SqlSelect;
                                if (select5 == null)
                                {
                                    throw Error.UnexpectedNode(node2.NodeType);
                                }
                                SqlAlias alias2 = new SqlAlias(select5);
                                SqlAliasRef exp = new SqlAliasRef(alias2);
                                select3.Selection = this.ConvertLinks(this.VisitExpression(exp));
                                select3.From = new SqlJoin(SqlJoinType.CrossApply, alias, alias2, null, m.SourceExpression);
                                return select3;
                            }
                            finally
                            {
                                this.currentSelect = currentSelect;
                            }
                            goto Label_07B9;
                        }
                    case SqlNodeType.Lift:
                        return this.AccessMember(m, ((SqlLift)expr).Expression);

                    case SqlNodeType.Grouping:
                        {
                            var grouping = (SqlGrouping)expr;
                            if (m.Member.Name == ConstColumns.Key)//ConstColumns.GetKey(translator.Provider.Mode))//"Key")
                            {
                                return grouping.Key;
                            }
                            goto Label_098C;
                        }
                    case SqlNodeType.ClientCase:
                        {
                            SqlClientCase @case = (SqlClientCase)expr;
                            Type clrType = null;
                            List<SqlExpression> matches = new List<SqlExpression>();
                            List<SqlExpression> values = new List<SqlExpression>();
                            foreach (SqlClientWhen when in @case.Whens)
                            {
                                SqlExpression item = (SqlExpression)this.AccessMember(m, when.Value);
                                if (clrType == null)
                                {
                                    clrType = item.ClrType;
                                }
                                else if (clrType != item.ClrType)
                                {
                                    throw Error.ExpectedClrTypesToAgree(clrType, item.ClrType);
                                }
                                matches.Add(when.Match);
                                values.Add(item);
                            }
                            return this.sql.Case(clrType, @case.Expression, matches, values, @case.SourceExpression);
                        }
                    case SqlNodeType.ClientParameter:
                        {
                            SqlClientParameter parameter = (SqlClientParameter)expr;
                            return new SqlClientParameter(m.ClrType, m.SqlType, Expression.Lambda(typeof(Func<,>).MakeGenericType(new Type[] { typeof(object[]), m.ClrType }), Expression.MakeMemberAccess(parameter.Accessor.Body, m.Member), parameter.Accessor.Parameters), parameter.SourceExpression);
                        }
                    case SqlNodeType.AliasRef:
                        {
                            SqlAliasRef ref2 = (SqlAliasRef)expr;
                            SqlTable table = ref2.Alias.Node as SqlTable;
                            if (table != null)
                            {
                                MetaDataMember member = GetRequiredInheritanceDataMember(table.RowType, m.Member);
                                string columnName = member.MappedName;
                                SqlColumn column = table.Find(columnName);
                                if (column == null)
                                {
                                    IProviderType sqlType = this.sql.Default(member);
                                    column = new SqlColumn(m.ClrType, sqlType, columnName, member, null, m.SourceExpression);
                                    column.Alias = ref2.Alias;
                                    table.Columns.Add(column);
                                }
                                return new SqlColumnRef(column);
                            }
                            SqlTableValuedFunctionCall call = ref2.Alias.Node as SqlTableValuedFunctionCall;
                            if (call == null)
                            {
                                goto Label_098C;
                            }
                            MetaDataMember requiredInheritanceDataMember = GetRequiredInheritanceDataMember(call.RowType, m.Member);
                            string mappedName = requiredInheritanceDataMember.MappedName;
                            SqlColumn column2 = call.Find(mappedName);
                            if (column2 == null)
                            {
                                IProviderType type4 = this.sql.Default(requiredInheritanceDataMember);
                                column2 = new SqlColumn(m.ClrType, type4, mappedName, requiredInheritanceDataMember, null, m.SourceExpression);
                                column2.Alias = ref2.Alias;
                                call.Columns.Add(column2);
                            }
                            return new SqlColumnRef(column2);
                        }
                    case SqlNodeType.OptionalValue:
                        return this.AccessMember(m, ((SqlOptionalValue)expr).Value);

                    case SqlNodeType.OuterJoinedValue:
                        {
                            SqlNode node = this.AccessMember(m, ((SqlUnary)expr).Operand);
                            SqlExpression expression = node as SqlExpression;
                            if (expression != null)
                            {
                                return this.sql.Unary(SqlNodeType.OuterJoinedValue, expression);
                            }
                            return node;
                        }
                    case SqlNodeType.New:
                        {
                            SqlNew new4 = (SqlNew)expr;
                            SqlExpression find = new4.Find(m.Member);

                            #region MyRegion
                            //if (find is SqlColumnRef && ((SqlColumnRef)find).Column is MySqlColumn)
                            //{
                            //    var match = ((SqlColumnRef)find).Column;
                            //    foreach (var alias in this.outerAliasMap.Keys)
                            //    {
                            //        //var alias = item.Value;
                            //        var tab = alias.Node as SqlTable;
                            //        if (tab != null)
                            //        {
                            //            var t1 = match.MetaMember.DeclaringType.Type;
                            //            var t2 = tab.RowType.Type;
                            //            if (t1 == t2 || t1.IsSubclassOf(t2))
                            //            {
                            //                tab.Columns.Add(match);

                            //                //SqlAlias alias;
                            //                //if (aliases.TryGetValue(tab, out alias))
                            //                match.Alias = alias;
                            //            }
                            //        }
                            //    }

                            //} 
                            #endregion

                            if (find != null)
                            {
                                return find;
                            }
                            if (predicate == null)
                            {
                                predicate = delegate(MetaDataMember p)
                                {
                                    return p.Member == m.Member;
                                };
                            }
                            MetaDataMember member4 = new4.MetaType.PersistentDataMembers.FirstOrDefault<MetaDataMember>(predicate);
                            if (!new4.SqlType.CanBeColumn && (member4 != null))
                            {
                                throw Error.MemberNotPartOfProjection(m.Member.DeclaringType, m.Member.Name);
                            }
                            goto Label_098C;
                        }
                    case SqlNodeType.SearchedCase:
                        {
                            SqlSearchedCase case3 = (SqlSearchedCase)expr;
                            List<SqlWhen> list5 = new List<SqlWhen>(case3.Whens.Count);
                            foreach (SqlWhen when3 in case3.Whens)
                            {
                                SqlExpression expression6 = (SqlExpression)this.AccessMember(m, when3.Value);
                                list5.Add(new SqlWhen(when3.Match, expression6));
                            }
                            SqlExpression @else = (SqlExpression)this.AccessMember(m, case3.Else);
                            return this.sql.SearchedCase(list5.ToArray(), @else, case3.SourceExpression);
                        }
                    case SqlNodeType.UserRow:
                        {
                            SqlUserRow row = (SqlUserRow)expr;
                            SqlUserQuery query = row.Query;
                            MetaDataMember member3 = GetRequiredInheritanceDataMember(row.RowType, m.Member);
                            string name = member3.MappedName;
                            SqlUserColumn column3 = query.Find(name);
                            if (column3 == null)
                            {
                                IProviderType type5 = this.sql.Default(member3);
                                column3 = new SqlUserColumn(m.ClrType, type5, query, name, member3.IsPrimaryKey, m.SourceExpression);
                                query.Columns.Add(column3);
                            }
                            return column3;
                        }
                    case SqlNodeType.Value:
                        goto Label_07B9;

                    case SqlNodeType.TypeCase:
                        {
                            SqlTypeCase case4 = (SqlTypeCase)expr;
                            typeBinding = case4.Whens[0].TypeBinding as SqlNew;
                            foreach (SqlTypeCaseWhen when4 in case4.Whens)
                            {
                                if (when4.TypeBinding.NodeType == SqlNodeType.New)
                                {
                                    SqlNew new3 = (SqlNew)when4.TypeBinding;
                                    if (m.Member.DeclaringType.IsAssignableFrom(new3.ClrType))
                                    {
                                        typeBinding = new3;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case SqlNodeType.SimpleCase:
                        {
                            SqlSimpleCase case2 = (SqlSimpleCase)expr;
                            Type type2 = null;
                            List<SqlExpression> list3 = new List<SqlExpression>();
                            List<SqlExpression> list4 = new List<SqlExpression>();
                            foreach (SqlWhen when2 in case2.Whens)
                            {
                                SqlExpression expression4 = (SqlExpression)this.AccessMember(m, when2.Value);
                                if (type2 == null)
                                {
                                    type2 = expression4.ClrType;
                                }
                                else if (type2 != expression4.ClrType)
                                {
                                    throw Error.ExpectedClrTypesToAgree(type2, expression4.ClrType);
                                }
                                list3.Add(when2.Match);
                                list4.Add(expression4);
                            }
                            return this.sql.Case(type2, case2.Expression, list3, list4, case2.SourceExpression);
                        }
                    default:
                        goto Label_098C;
                }
                return this.AccessMember(m, typeBinding);
            Label_07B9:
                value2 = (SqlValue)expr;
                if (value2.Value == null)
                {
                    return this.sql.Value(m.ClrType, m.SqlType, null, value2.IsClientSpecified, m.SourceExpression);
                }
                if (m.Member is PropertyInfo)
                {
                    PropertyInfo info = (PropertyInfo)m.Member;
                    return this.sql.Value(m.ClrType, m.SqlType, info.GetValue(value2.Value, null), value2.IsClientSpecified, m.SourceExpression);
                }
                FieldInfo info2 = (FieldInfo)m.Member;
                return this.sql.Value(m.ClrType, m.SqlType, info2.GetValue(value2.Value), value2.IsClientSpecified, m.SourceExpression);
            Label_098C:
                if (m.Expression == expr)
                {
                    return m;
                }
                return this.sql.Member(expr, m.Member);
            }

            private SqlExpression ApplyTreat(SqlExpression target, Type type)
            {
                switch (target.NodeType)
                {
                    case SqlNodeType.OptionalValue:
                        {
                            SqlOptionalValue value2 = (SqlOptionalValue)target;
                            return this.ApplyTreat(value2.Value, type);
                        }
                    case SqlNodeType.OuterJoinedValue:
                        {
                            SqlUnary unary = (SqlUnary)target;
                            return this.ApplyTreat(unary.Operand, type);
                        }
                    case SqlNodeType.TypeCase:
                        {
                            SqlTypeCase @case = (SqlTypeCase)target;
                            int num = 0;
                            foreach (SqlTypeCaseWhen when in @case.Whens)
                            {
                                when.TypeBinding = this.ApplyTreat(when.TypeBinding, type);
                                if (this.IsConstNull(when.TypeBinding))
                                {
                                    num++;
                                }
                            }
                            if (num == @case.Whens.Count)
                            {
                                @case.Whens[0].TypeBinding.SetClrType(type);
                                return @case.Whens[0].TypeBinding;
                            }
                            @case.SetClrType(type);
                            return target;
                        }
                    case SqlNodeType.New:
                        {
                            SqlNew new2 = (SqlNew)target;
                            if (!type.IsAssignableFrom(new2.ClrType))
                            {
                                return this.sql.TypedLiteralNull(type, target.SourceExpression);
                            }
                            return target;
                        }
                }
                SqlExpression expression = target;
                if (((expression != null) && !type.IsAssignableFrom(expression.ClrType)) && !expression.ClrType.IsAssignableFrom(type))
                {
                    return this.sql.TypedLiteralNull(type, target.SourceExpression);
                }
                return target;
            }

            private SqlExpression ConvertLinks(SqlExpression node)
            {
                if (node == null)
                {
                    return null;
                }
                SqlNodeType nodeType = node.NodeType;
                if (nodeType <= SqlNodeType.Column)
                {
                    switch (nodeType)
                    {
                        case SqlNodeType.ClientCase:
                            {
                                SqlClientCase @case = (SqlClientCase)node;
                                foreach (SqlClientWhen when in @case.Whens)
                                {
                                    SqlExpression expression3 = this.ConvertLinks(when.Value);
                                    when.Value = expression3;
                                    if (!@case.ClrType.IsAssignableFrom(when.Value.ClrType))
                                    {
                                        throw Error.DidNotExpectTypeChange(when.Value.ClrType, @case.ClrType);
                                    }
                                }
                                return node;
                            }
                        case SqlNodeType.Column:
                            {
                                SqlColumn column = (SqlColumn)node;
                                if (column.Expression != null)
                                {
                                    column.Expression = this.ConvertLinks(column.Expression);
                                }
                                return node;
                            }
                    }
                    return node;
                }
                switch (nodeType)
                {
                    case SqlNodeType.Link:
                        return this.ConvertToFetchedExpression((SqlLink)node);

                    case SqlNodeType.OuterJoinedValue:
                        {
                            SqlExpression operand = ((SqlUnary)node).Operand;
                            SqlExpression expression = this.ConvertLinks(operand);
                            if (expression == operand)
                            {
                                return node;
                            }
                            if (expression.NodeType != SqlNodeType.OuterJoinedValue)
                            {
                                return this.sql.Unary(SqlNodeType.OuterJoinedValue, expression);
                            }
                            return expression;
                        }
                }
                return node;
            }

            internal SqlExpression ConvertToExpression(SqlNode node)
            {
                if (node == null)
                {
                    return null;
                }
                SqlExpression expression = node as SqlExpression;
                if (expression != null)
                {
                    return expression;
                }
                SqlSelect select = node as SqlSelect;
                if (select == null)
                {
                    throw Error.UnexpectedNode(node.NodeType);
                }
                return this.sql.SubSelect(SqlNodeType.Multiset, select);
            }

            internal SqlExpression ConvertToFetchedExpression(SqlNode node)
            {
                if (node == null)
                {
                    return null;
                }
                switch (node.NodeType)
                {
                    case SqlNodeType.ClientCase:
                        {
                            SqlClientCase clientCase = (SqlClientCase)node;
                            List<SqlNode> sequences = new List<SqlNode>();
                            bool flag = true;
                            foreach (SqlClientWhen when in clientCase.Whens)
                            {
                                SqlNode item = this.ConvertToFetchedExpression(when.Value);
                                flag = flag && (item is SqlExpression);
                                sequences.Add(item);
                            }
                            if (flag)
                            {
                                List<SqlExpression> matches = new List<SqlExpression>();
                                List<SqlExpression> values = new List<SqlExpression>();
                                int num = 0;
                                int count = sequences.Count;
                                while (num < count)
                                {
                                    SqlExpression expression3 = (SqlExpression)sequences[num];
                                    if (!clientCase.ClrType.IsAssignableFrom(expression3.ClrType))
                                    {
                                        throw Error.DidNotExpectTypeChange(clientCase.ClrType, expression3.ClrType);
                                    }
                                    matches.Add(clientCase.Whens[num].Match);
                                    values.Add(expression3);
                                    num++;
                                }
                                node = this.sql.Case(clientCase.ClrType, clientCase.Expression, matches, values, clientCase.SourceExpression);
                            }
                            else
                            {
                                node = this.SimulateCaseOfSequences(clientCase, sequences);
                            }
                            goto Label_04DE;
                        }
                    case SqlNodeType.Link:
                        {
                            SqlExpression expression5;
                            SqlLink link = (SqlLink)node;
                            if (link.Expansion != null)
                            {
                                return this.VisitLinkExpansion(link);
                            }
                            if (this.linkMap.TryGetValue(link.Id, out expression5))
                            {
                                return this.VisitExpression(expression5);
                            }
                            node = this.translator.TranslateLink(link, true);
                            node = this.binder.Prebind(node);
                            node = this.ConvertToExpression(node);
                            node = this.Visit(node);
                            if ((((this.currentSelect != null) && (node != null)) && ((node.NodeType == SqlNodeType.Element) && link.Member.IsAssociation)) && this.binder.OptimizeLinkExpansions)
                            {
                                if (link.Member.Association.IsNullable || !link.Member.Association.IsForeignKey)
                                {
                                    var select = (SqlSubSelect)node;
                                    select.Select.Selection = new SqlOptionalValue(new SqlColumn(ConstColumns.Test, sql.Unary(SqlNodeType.OuterJoinedValue, sql.Value(typeof(int?), typeProvider.From(typeof(int)), 1, false, link.SourceExpression))), select.Select.Selection);
                                    SqlExpression cond = select.Select.Where;
                                    select.Select.Where = null;
                                    var alias = new SqlAlias(select.Select);
                                    this.currentSelect.From = new SqlJoin(SqlJoinType.LeftOuter, this.currentSelect.From, alias, cond, select.SourceExpression);
                                    SqlExpression expression7 = new SqlAliasRef(alias);
                                    this.linkMap.Add(link.Id, expression7);
                                    return this.VisitExpression(expression7);
                                }
                                var select2 = (SqlSubSelect)node;
                                SqlExpression where = select2.Select.Where;
                                select2.Select.Where = null;
                                var right = new SqlAlias(select2.Select);
                                this.currentSelect.From = new SqlJoin(SqlJoinType.Inner, this.currentSelect.From, right, where, select2.SourceExpression);
                                SqlExpression expr = new SqlAliasRef(right);
                                this.linkMap.Add(link.Id, expr);
                                return this.VisitExpression(expr);
                            }
                            goto Label_04DE;
                        }
                    case SqlNodeType.OuterJoinedValue:
                        {
                            SqlExpression operand = ((SqlUnary)node).Operand;
                            SqlExpression expression2 = this.ConvertLinks(operand);
                            if (expression2 == operand)
                            {
                                return (SqlExpression)node;
                            }
                            return expression2;
                        }
                    case SqlNodeType.SearchedCase:
                        {
                            SqlSearchedCase case3 = (SqlSearchedCase)node;
                            foreach (SqlWhen when3 in case3.Whens)
                            {
                                when3.Match = this.ConvertToFetchedExpression(when3.Match);
                                when3.Value = this.ConvertToFetchedExpression(when3.Value);
                            }
                            case3.Else = this.ConvertToFetchedExpression(case3.Else);
                            break;
                        }
                    case SqlNodeType.TypeCase:
                        {
                            SqlTypeCase case2 = (SqlTypeCase)node;
                            List<SqlNode> list4 = new List<SqlNode>();
                            foreach (SqlTypeCaseWhen when2 in case2.Whens)
                            {
                                SqlNode node3 = this.ConvertToFetchedExpression(when2.TypeBinding);
                                list4.Add(node3);
                            }
                            int num3 = 0;
                            int num4 = list4.Count;
                            while (num3 < num4)
                            {
                                SqlExpression expression4 = (SqlExpression)list4[num3];
                                case2.Whens[num3].TypeBinding = expression4;
                                num3++;
                            }
                            goto Label_04DE;
                        }
                }
            Label_04DE:
                return (SqlExpression)node;
            }

            internal SqlNode ConvertToFetchedSequence(SqlNode node)
            {
                if (node != null)
                {
                    while (node.NodeType == SqlNodeType.OuterJoinedValue)
                    {
                        node = ((SqlUnary)node).Operand;
                    }
                    SqlExpression expression = node as SqlExpression;
                    if (expression != null)
                    {
                        if (!TypeSystem.IsSequenceType(expression.ClrType))
                        {
                            throw Error.SequenceOperatorsNotSupportedForType(expression.ClrType);
                        }
                        if (expression.NodeType == SqlNodeType.Value)
                        {
                            throw Error.QueryOnLocalCollectionNotSupported();
                        }
                        if (expression.NodeType == SqlNodeType.Link)
                        {
                            SqlLink link = (SqlLink)expression;
                            if (link.Expansion != null)
                            {
                                return this.VisitLinkExpansion(link);
                            }
                            node = this.translator.TranslateLink(link, false);
                            node = this.binder.Prebind(node);
                            node = this.Visit(node);
                        }
                        else if (expression.NodeType == SqlNodeType.Grouping)
                        {
                            node = ((SqlGrouping)expression).Group;
                        }
                        else if (expression.NodeType == SqlNodeType.ClientCase)
                        {
                            SqlClientCase clientCase = (SqlClientCase)expression;
                            List<SqlNode> sequences = new List<SqlNode>();
                            bool flag = false;
                            bool flag2 = true;
                            foreach (SqlClientWhen when in clientCase.Whens)
                            {
                                SqlNode item = this.ConvertToFetchedSequence(when.Value);
                                flag = flag || (item != when.Value);
                                sequences.Add(item);
                                flag2 = flag2 && SqlComparer.AreEqual(when.Value, clientCase.Whens[0].Value);
                            }
                            if (flag)
                            {
                                if (flag2)
                                {
                                    node = sequences[0];
                                }
                                else
                                {
                                    node = this.SimulateCaseOfSequences(clientCase, sequences);
                                }
                            }
                        }
                        SqlSubSelect select = node as SqlSubSelect;
                        if (select != null)
                        {
                            node = select.Select;
                        }
                    }
                    return node;
                }
                return node;
            }

            internal SqlExpression ExpandExpression(SqlExpression expression)
            {
                SqlExpression exp = this.expander.Expand(expression);
                if (exp != expression)
                {
                    exp = this.VisitExpression(exp);
                }
                return exp;
            }

            internal SqlExpression FetchExpression(SqlExpression expr)
            {
                return this.ConvertToExpression(this.ConvertToFetchedExpression(this.ConvertLinks(this.VisitExpression(expr))));
            }

            private SqlExpression FoldSubquery(SqlSubSelect ss)
            {
            Label_0000:
                while ((ss.NodeType == SqlNodeType.Element) && (ss.Select.Selection.NodeType == SqlNodeType.Multiset))
                {
                    SqlSubSelect selection = (SqlSubSelect)ss.Select.Selection;
                    SqlAlias alias = new SqlAlias(selection.Select);
                    SqlAliasRef exp = new SqlAliasRef(alias);
                    SqlSelect select = ss.Select;
                    select.Selection = this.ConvertLinks(this.VisitExpression(exp));
                    select.From = new SqlJoin(SqlJoinType.CrossApply, select.From, alias, null, ss.SourceExpression);
                    ss = this.sql.SubSelect(SqlNodeType.Multiset, select, ss.ClrType);
                }
                if ((ss.NodeType == SqlNodeType.Element) && (ss.Select.Selection.NodeType == SqlNodeType.Element))
                {
                    SqlSubSelect select4 = (SqlSubSelect)ss.Select.Selection;
                    SqlAlias alias2 = new SqlAlias(select4.Select);
                    SqlAliasRef ref3 = new SqlAliasRef(alias2);
                    SqlSelect select5 = ss.Select;
                    select5.Selection = this.ConvertLinks(this.VisitExpression(ref3));
                    select5.From = new SqlJoin(SqlJoinType.CrossApply, select5.From, alias2, null, ss.SourceExpression);
                    ss = this.sql.SubSelect(SqlNodeType.Element, select5);
                    goto Label_0000;
                }
                return ss;
            }

            private MetaType[] GetPossibleTypes(SqlExpression typeExpression)
            {
                if (!typeof(Type).IsAssignableFrom(typeExpression.ClrType))
                {
                    return new MetaType[0];
                }
                if (typeExpression.NodeType == SqlNodeType.DiscriminatedType)
                {
                    SqlDiscriminatedType type = (SqlDiscriminatedType)typeExpression;
                    List<MetaType> list = new List<MetaType>();
                    foreach (MetaType type2 in type.TargetType.InheritanceTypes)
                    {
                        if (!type2.Type.IsAbstract)
                        {
                            list.Add(type2);
                        }
                    }
                    return list.ToArray();
                }
                if (typeExpression.NodeType == SqlNodeType.Value)
                {
                    SqlValue value2 = (SqlValue)typeExpression;
                    MetaType metaType = this.model.GetMetaType((Type)value2.Value);
                    return new MetaType[] { metaType };
                }
                if (typeExpression.NodeType != SqlNodeType.SearchedCase)
                {
                    throw Error.UnexpectedNode(typeExpression.NodeType);
                }
                SqlSearchedCase @case = (SqlSearchedCase)typeExpression;
                HashSet<MetaType> source = new HashSet<MetaType>();
                foreach (SqlWhen when in @case.Whens)
                {
                    source.UnionWith(this.GetPossibleTypes(when.Value));
                }
                return source.ToArray<MetaType>();
            }

            private static MetaDataMember GetRequiredInheritanceDataMember(MetaType type, MemberInfo mi)
            {
                MetaType inheritanceType = type.GetInheritanceType(mi.DeclaringType);
                if (inheritanceType == null)
                {
                    throw Error.UnmappedDataMember(mi, mi.DeclaringType, type);
                }
                return inheritanceType.GetDataMember(mi);
            }

            private SqlSelect GetSourceSelect(SqlSource source)
            {
                SqlAlias alias = source as SqlAlias;
                if (alias == null)
                {
                    return null;
                }
                return (alias.Node as SqlSelect);
            }

            private bool IsConstNull(SqlExpression sqlExpr)
            {
                SqlValue value2 = sqlExpr as SqlValue;
                if (value2 == null)
                {
                    return false;
                }
                return ((value2.Value == null) && !value2.IsClientSpecified);
            }

            private SqlExpression PushDownExpression(SqlExpression expr)
            {
                if ((expr.NodeType == SqlNodeType.Value) && expr.SqlType.CanBeColumn)
                {
                    expr = new SqlColumn(expr.ClrType, expr.SqlType, null, null, expr, expr.SourceExpression);
                }
                else
                {
                    expr = this.columnizer.ColumnizeSelection(expr);
                }
                SqlSelect node = new SqlSelect(expr, this.currentSelect.From, expr.SourceExpression);
                this.currentSelect.From = new SqlAlias(node);
                return this.ExpandExpression(expr);
            }

            private SqlSelect SimulateCaseOfSequences(SqlClientCase clientCase, List<SqlNode> sequences)
            {
                if (sequences.Count == 1)
                {
                    return (SqlSelect)sequences[0];
                }
                SqlNode right = null;
                SqlSelect left = null;
                int num = clientCase.Whens.Count - 1;
                int num2 = (clientCase.Whens[num].Match == null) ? 1 : 0;
                SqlExpression expression = null;
                for (int i = 0; i < (sequences.Count - num2); i++)
                {
                    left = (SqlSelect)sequences[i];
                    SqlExpression expression2 = this.sql.Binary(SqlNodeType.EQ, clientCase.Expression, clientCase.Whens[i].Match);
                    left.Where = this.sql.AndAccumulate(left.Where, expression2);
                    expression = this.sql.AndAccumulate(expression, this.sql.Binary(SqlNodeType.NE, clientCase.Expression, clientCase.Whens[i].Match));
                    if (right == null)
                    {
                        right = left;
                    }
                    else
                    {
                        right = new SqlUnion(left, right, true);
                    }
                }
                if (num2 == 1)
                {
                    left = (SqlSelect)sequences[num];
                    left.Where = this.sql.AndAccumulate(left.Where, expression);
                    if (right == null)
                    {
                        right = left;
                    }
                    else
                    {
                        right = new SqlUnion(left, right, true);
                    }
                }
                SqlAlias from = new SqlAlias(right);
                return new SqlSelect(new SqlAliasRef(from), from, right.SourceExpression);
            }

            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                SqlAlias alias2;
                SqlAlias currentAlias = this.currentAlias;
                if (a.Node.NodeType == SqlNodeType.Table)
                {
                    this.outerAliasMap[a] = this.currentAlias;
                }
                this.currentAlias = a;
                try
                {
                    a.Node = this.ConvertToFetchedSequence(this.Visit(a.Node));
                    alias2 = a;
                }
                finally
                {
                    this.currentAlias = currentAlias;
                }
                return alias2;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                return this.ExpandExpression(aref);
            }

            internal override SqlStatement VisitAssign(SqlAssign sa)
            {
                sa.LValue = this.FetchExpression(sa.LValue);
                sa.RValue = this.FetchExpression(sa.RValue);
                return sa;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                switch (bo.NodeType)
                {
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        if (!this.IsConstNull(bo.Left) || TypeSystem.IsNullableType(bo.ClrType))
                        {
                            if (!this.IsConstNull(bo.Right) || TypeSystem.IsNullableType(bo.ClrType))
                            {
                                break;
                            }
                            return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNull, bo.Left, bo.SourceExpression));
                        }
                        return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNull, bo.Right, bo.SourceExpression));

                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                        if (!this.IsConstNull(bo.Left) || TypeSystem.IsNullableType(bo.ClrType))
                        {
                            if (this.IsConstNull(bo.Right) && !TypeSystem.IsNullableType(bo.ClrType))
                            {
                                return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNotNull, bo.Left, bo.SourceExpression));
                            }
                            break;
                        }
                        return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNotNull, bo.Right, bo.SourceExpression));
                }
                bo.Left = this.VisitExpression(bo.Left);
                bo.Right = this.VisitExpression(bo.Right);
                switch (bo.NodeType)
                {
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                        {
                            SqlValue left = bo.Left as SqlValue;
                            SqlValue right = bo.Right as SqlValue;
                            bool flag = (left != null) && (left.Value is bool);
                            bool flag2 = (right != null) && (right.Value is bool);
                            if (!flag && !flag2)
                            {
                                break;
                            }
                            bool flag3 = (bo.NodeType != SqlNodeType.NE) && (bo.NodeType != SqlNodeType.NE2V);
                            SqlNodeType nt = ((bo.NodeType == SqlNodeType.EQ2V) || (bo.NodeType == SqlNodeType.NE2V)) ? SqlNodeType.Not2V : SqlNodeType.Not;
                            if (!flag || flag2)
                            {
                                if (!flag && flag2)
                                {
                                    bool flag6 = (bool)right.Value;
                                    if (flag6 ^ flag3)
                                    {
                                        return this.VisitUnaryOperator(new SqlUnary(nt, bo.ClrType, bo.SqlType, this.sql.DoNotVisitExpression(bo.Left), bo.SourceExpression));
                                    }
                                    if (bo.Left.ClrType == typeof(bool))
                                    {
                                        return bo.Left;
                                    }
                                }
                                else if (flag && flag2)
                                {
                                    bool flag7 = (bool)left.Value;
                                    bool flag8 = (bool)right.Value;
                                    if (flag3)
                                    {
                                        return this.sql.ValueFromObject(flag7 == flag8, false, bo.SourceExpression);
                                    }
                                    return this.sql.ValueFromObject(flag7 != flag8, false, bo.SourceExpression);
                                }
                                break;
                            }
                            bool flag5 = (bool)left.Value;
                            if (flag5 ^ flag3)
                            {
                                return this.VisitUnaryOperator(new SqlUnary(nt, bo.ClrType, bo.SqlType, this.sql.DoNotVisitExpression(bo.Right), bo.SourceExpression));
                            }
                            if (bo.Right.ClrType != typeof(bool))
                            {
                                break;
                            }
                            return bo.Right;
                        }
                }
                switch (bo.NodeType)
                {
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        {
                            SqlExpression exp = this.translator.TranslateLinkEquals(bo);
                            if (exp != bo)
                            {
                                return this.VisitExpression(exp);
                            }
                            break;
                        }
                    case SqlNodeType.Or:
                        {
                            SqlValue value6 = bo.Left as SqlValue;
                            SqlValue value7 = bo.Right as SqlValue;
                            if ((value6 == null) || (value7 != null))
                            {
                                if ((value6 == null) && (value7 != null))
                                {
                                    if (!((bool)value7.Value))
                                    {
                                        return bo.Left;
                                    }
                                    return this.sql.ValueFromObject(true, false, bo.SourceExpression);
                                }
                                if ((value6 != null) && (value7 != null))
                                {
                                    return this.sql.ValueFromObject(((bool)value6.Value) || ((bool)value7.Value), false, bo.SourceExpression);
                                }
                                break;
                            }
                            if (!((bool)value6.Value))
                            {
                                return bo.Right;
                            }
                            return this.sql.ValueFromObject(true, false, bo.SourceExpression);
                        }
                    case SqlNodeType.And:
                        {
                            SqlValue value4 = bo.Left as SqlValue;
                            SqlValue value5 = bo.Right as SqlValue;
                            if ((value4 != null) && (value5 == null))
                            {
                                if ((bool)value4.Value)
                                {
                                    return bo.Right;
                                }
                                return this.sql.ValueFromObject(false, false, bo.SourceExpression);
                            }
                            if ((value4 == null) && (value5 != null))
                            {
                                if ((bool)value5.Value)
                                {
                                    return bo.Left;
                                }
                                return this.sql.ValueFromObject(false, false, bo.SourceExpression);
                            }
                            if ((value4 == null) || (value5 == null))
                            {
                                break;
                            }
                            return this.sql.ValueFromObject(((bool)value4.Value) && ((bool)value5.Value), false, bo.SourceExpression);
                        }
                }
                bo.Left = this.ConvertToFetchedExpression(bo.Left);
                bo.Right = this.ConvertToFetchedExpression(bo.Right);
                switch (bo.NodeType)
                {
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                        {
                            SqlExpression expression2 = this.translator.TranslateEquals(bo);
                            if (expression2 != bo)
                            {
                                return this.VisitExpression(expression2);
                            }
                            if (typeof(Type).IsAssignableFrom(bo.Left.ClrType))
                            {
                                SqlExpression typeSource = TypeSource.GetTypeSource(bo.Left);
                                SqlExpression typeExpression = TypeSource.GetTypeSource(bo.Right);
                                MetaType[] possibleTypes = this.GetPossibleTypes(typeSource);
                                MetaType[] typeArray2 = this.GetPossibleTypes(typeExpression);
                                bool flag9 = false;
                                for (int i = 0; i < possibleTypes.Length; i++)
                                {
                                    for (int j = 0; j < typeArray2.Length; j++)
                                    {
                                        if (possibleTypes[i] == typeArray2[j])
                                        {
                                            flag9 = true;
                                            break;
                                        }
                                    }
                                }
                                if (!flag9)
                                {
                                    return this.VisitExpression(this.sql.ValueFromObject(bo.NodeType == SqlNodeType.NE, false, bo.SourceExpression));
                                }
                                if ((possibleTypes.Length == 1) && (typeArray2.Length == 1))
                                {
                                    return this.VisitExpression(this.sql.ValueFromObject((bo.NodeType == SqlNodeType.EQ) == (possibleTypes[0] == typeArray2[0]), false, bo.SourceExpression));
                                }
                                SqlDiscriminatedType type2 = bo.Left as SqlDiscriminatedType;
                                SqlDiscriminatedType type3 = bo.Right as SqlDiscriminatedType;
                                if ((type2 != null) && (type3 != null))
                                {
                                    return this.VisitExpression(this.sql.Binary(bo.NodeType, type2.Discriminator, type3.Discriminator));
                                }
                            }
                            if (TypeSystem.IsSequenceType(bo.Left.ClrType))
                            {
                                throw Error.ComparisonNotSupportedForType(bo.Left.ClrType);
                            }
                            if (TypeSystem.IsSequenceType(bo.Right.ClrType))
                            {
                                throw Error.ComparisonNotSupportedForType(bo.Right.ClrType);
                            }
                            return bo;
                        }
                }
                return bo;
            }

            internal override SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof)
            {
                SqlExpression operand = this.FetchExpression(dof.Object);
                while ((operand.NodeType == SqlNodeType.OptionalValue) || (operand.NodeType == SqlNodeType.OuterJoinedValue))
                {
                    if (operand.NodeType == SqlNodeType.OptionalValue)
                    {
                        operand = ((SqlOptionalValue)operand).Value;
                    }
                    else
                    {
                        operand = ((SqlUnary)operand).Operand;
                    }
                }
                if (operand.NodeType == SqlNodeType.TypeCase)
                {
                    SqlTypeCase @case = (SqlTypeCase)operand;
                    List<SqlExpression> matches = new List<SqlExpression>();
                    List<SqlExpression> values = new List<SqlExpression>();
                    MetaType inheritanceDefault = @case.RowType.InheritanceDefault;
                    object inheritanceCode = inheritanceDefault.InheritanceCode;
                    foreach (SqlTypeCaseWhen when in @case.Whens)
                    {
                        matches.Add(when.Match);
                        if (when.Match == null)
                        {
                            SqlExpression item = this.sql.Value(inheritanceCode.GetType(), @case.Whens[0].Match.SqlType, inheritanceDefault.InheritanceCode, true, @case.SourceExpression);
                            values.Add(item);
                        }
                        else
                        {
                            values.Add(this.sql.Value(inheritanceCode.GetType(), when.Match.SqlType, ((SqlValue)when.Match).Value, true, @case.SourceExpression));
                        }
                    }
                    return this.sql.Case(@case.Discriminator.ClrType, @case.Discriminator, matches, values, @case.SourceExpression);
                }
                MetaType inheritanceRoot = this.model.GetMetaType(operand.ClrType).InheritanceRoot;
                if (inheritanceRoot.HasInheritance)
                {
                    return this.VisitExpression(this.sql.Member(dof.Object, inheritanceRoot.Discriminator.Member));
                }
                return this.sql.TypedLiteralNull(dof.ClrType, dof.SourceExpression);
            }

            internal override SqlExpression VisitExpression(SqlExpression expr)
            {
                return this.ConvertToExpression(this.Visit(expr));
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                int num = 0;
                int count = fc.Arguments.Count;
                while (num < count)
                {
                    fc.Arguments[num] = this.FetchExpression(fc.Arguments[num]);
                    num++;
                }
                return fc;
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g)
            {
                g.Key = this.FetchExpression(g.Key);
                g.Group = this.FetchExpression(g.Group);
                return g;
            }

            internal override SqlNode VisitIncludeScope(SqlIncludeScope scope)
            {
                SqlNode node;
                this.alreadyIncluded = new HashSet<MetaType>();
                try
                {
                    node = this.Visit(scope.Child);
                }
                finally
                {
                    this.alreadyIncluded = null;
                }
                return node;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                if ((join.JoinType == SqlJoinType.CrossApply) || (join.JoinType == SqlJoinType.OuterApply))
                {
                    join.Left = this.VisitSource(join.Left);
                    SqlSelect currentSelect = this.currentSelect;
                    try
                    {
                        this.currentSelect = this.GetSourceSelect(join.Left);
                        join.Right = this.VisitSource(join.Right);
                        this.currentSelect = null;
                        join.Condition = this.VisitExpression(join.Condition);
                        return join;
                    }
                    finally
                    {
                        this.currentSelect = currentSelect;
                    }
                }
                return base.VisitJoin(join);
            }

            internal override SqlExpression VisitLike(SqlLike like)
            {
                like.Expression = this.FetchExpression(like.Expression);
                like.Pattern = this.FetchExpression(like.Pattern);
                return base.VisitLike(like);
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                link = (SqlLink)base.VisitLink(link);
                if ((!this.disableInclude && (this.shape != null)) && (this.alreadyIncluded != null))
                {
                    MetaDataMember member = link.Member;
                    MemberInfo info = member.Member;
                    if (this.shape.IsPreloaded(info) && (member.LoadMethod == null))
                    {
                        MetaType inheritanceRoot = member.DeclaringType.InheritanceRoot;
                        if (!this.alreadyIncluded.Contains(inheritanceRoot))
                        {
                            this.alreadyIncluded.Add(inheritanceRoot);
                            SqlNode node = this.ConvertToFetchedExpression(link);
                            this.alreadyIncluded.Remove(inheritanceRoot);
                            return node;
                        }
                    }
                }
                if (this.inGroupBy && (link.Expansion != null))
                {
                    return this.VisitLinkExpansion(link);
                }
                return link;
            }

            private SqlExpression VisitLinkExpansion(SqlLink link)
            {
                SqlAlias alias;
                SqlAliasRef expansion = link.Expansion as SqlAliasRef;
                if (((expansion != null) && (expansion.Alias.Node.NodeType == SqlNodeType.Table)) && this.outerAliasMap.TryGetValue(expansion.Alias, out alias))
                {
                    return this.VisitAliasRef(new SqlAliasRef(alias));
                }
                return this.VisitExpression(link.Expansion);
            }

            protected override SqlNode VisitMember(SqlMember m)
            {
                return this.AccessMember(m, this.FetchExpression(m.Expression));
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                mc.Object = this.FetchExpression(mc.Object);
                int num = 0;
                int count = mc.Arguments.Count;
                while (num < count)
                {
                    mc.Arguments[num] = this.FetchExpression(mc.Arguments[num]);
                    num++;
                }
                return mc;
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                int num = 0;
                int count = sox.Args.Count;
                while (num < count)
                {
                    if (this.inGroupBy)
                    {
                        sox.Args[num] = this.VisitExpression(sox.Args[num]);
                    }
                    else
                    {
                        sox.Args[num] = this.FetchExpression(sox.Args[num]);
                    }
                    num++;
                }
                int num3 = 0;
                int num4 = sox.Members.Count;
                while (num3 < num4)
                {
                    SqlMemberAssign assign = sox.Members[num3];
                    MetaDataMember dataMember = sox.MetaType.GetDataMember(assign.Member);
                    MetaType inheritanceRoot = dataMember.DeclaringType.InheritanceRoot;
                    if (((dataMember.IsAssociation && (assign.Expression != null)) && ((assign.Expression.NodeType != SqlNodeType.Link) && (this.shape != null))) && ((this.shape.IsPreloaded(dataMember.Member) && (dataMember.LoadMethod == null)) && ((this.alreadyIncluded != null) && !this.alreadyIncluded.Contains(inheritanceRoot))))
                    {
                        this.alreadyIncluded.Add(inheritanceRoot);
                        assign.Expression = this.VisitExpression(assign.Expression);
                        this.alreadyIncluded.Remove(inheritanceRoot);
                    }
                    else if (dataMember.IsAssociation || dataMember.IsDeferred)
                    {
                        assign.Expression = this.VisitExpression(assign.Expression);
                    }
                    else
                    {
                        assign.Expression = this.FetchExpression(assign.Expression);
                    }
                    num3++;
                }
                return sox;
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                if (((c.ClrType == typeof(bool)) || (c.ClrType == typeof(bool?))) && ((c.Whens.Count == 1) && (c.Else != null)))
                {
                    SqlValue @else = c.Else as SqlValue;
                    SqlValue value3 = c.Whens[0].Value as SqlValue;
                    if ((@else != null) && !((bool)@else.Value))
                    {
                        return this.VisitExpression(this.sql.Binary(SqlNodeType.And, c.Whens[0].Match, c.Whens[0].Value));
                    }
                    if ((value3 != null) && ((bool)value3.Value))
                    {
                        return this.VisitExpression(this.sql.Binary(SqlNodeType.Or, c.Whens[0].Match, c.Else));
                    }
                }
                return base.VisitSearchedCase(c);
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlBinder.LinkOptimizationScope linkMap = this.linkMap;
                SqlSelect currentSelect = this.currentSelect;
                bool inGroupBy = this.inGroupBy;
                this.inGroupBy = false;
                try
                {
                    bool flag2 = true;
                    if (this.binder.optimizeLinkExpansions && (((select.GroupBy.Count > 0) || this.aggregateChecker.HasAggregates(select)) || select.IsDistinct))
                    {
                        flag2 = false;
                        this.linkMap = new SqlBinder.LinkOptimizationScope(this.linkMap);
                    }
                    select.From = this.VisitSource(select.From);
                    this.currentSelect = select;
                    select.Where = this.VisitExpression(select.Where);
                    this.inGroupBy = true;
                    int num = 0;
                    int count = select.GroupBy.Count;
                    while (num < count)
                    {
                        select.GroupBy[num] = this.VisitExpression(select.GroupBy[num]);
                        num++;
                    }
                    this.inGroupBy = false;
                    select.Having = this.VisitExpression(select.Having);
                    int num3 = 0;
                    int num4 = select.OrderBy.Count;
                    while (num3 < num4)
                    {
                        select.OrderBy[num3].Expression = this.VisitExpression(select.OrderBy[num3].Expression);
                        num3++;
                    }
                    select.Top = this.VisitExpression(select.Top);
                    select.Row = (SqlRow)this.Visit(select.Row);
                    select.Selection = this.VisitExpression(select.Selection);
                    select.Selection = this.columnizer.ColumnizeSelection(select.Selection);
                    if (flag2)
                    {
                        select.Selection = this.ConvertLinks(select.Selection);
                    }
                    if (((select.Where != null) && (select.Where.NodeType == SqlNodeType.Value)) && ((bool)((SqlValue)select.Where).Value))
                    {
                        select.Where = null;
                    }
                }
                finally
                {
                    this.currentSelect = currentSelect;
                    this.linkMap = linkMap;
                    this.inGroupBy = inGroupBy;
                }
                return select;
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared)
            {
                shared.Expression = this.VisitExpression(shared.Expression);
                if (shared.Expression.NodeType != SqlNodeType.ColumnRef)
                {
                    shared.Expression = this.PushDownExpression(shared.Expression);
                }
                return shared.Expression;
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
            {
                return (SqlExpression)SqlDuplicator.Copy(sref.SharedExpression.Expression);
            }

            internal override SqlExpression VisitSimpleExpression(SqlSimpleExpression simple)
            {
                simple.Expression = this.VisitExpression(simple.Expression);
                if (SimpleExpression.IsSimple(simple.Expression))
                {
                    return simple.Expression;
                }
                return this.PushDownExpression(simple.Expression);
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                SqlExpression expression;
                SqlBinder.LinkOptimizationScope linkMap = this.linkMap;
                SqlSelect currentSelect = this.currentSelect;
                try
                {
                    this.linkMap = new SqlBinder.LinkOptimizationScope(this.linkMap);
                    this.currentSelect = null;
                    expression = base.VisitSubSelect(ss);
                }
                finally
                {
                    this.linkMap = linkMap;
                    this.currentSelect = currentSelect;
                }
                return expression;
            }

            internal override SqlExpression VisitTreat(SqlUnary a)
            {
                return this.VisitUnaryOperator(a);
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                uo.Operand = this.VisitExpression(uo.Operand);
                if ((uo.NodeType == SqlNodeType.IsNull) || (uo.NodeType == SqlNodeType.IsNotNull))
                {
                    SqlExpression exp = this.translator.TranslateLinkIsNull(uo);
                    if (exp != uo)
                    {
                        return this.VisitExpression(exp);
                    }
                    if (uo.Operand.NodeType == SqlNodeType.OuterJoinedValue)
                    {
                        SqlUnary unary = uo.Operand as SqlUnary;
                        if (unary.Operand.NodeType == SqlNodeType.OptionalValue)
                        {
                            SqlOptionalValue value2 = (SqlOptionalValue)unary.Operand;
                            return this.VisitUnaryOperator(new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, new SqlUnary(SqlNodeType.OuterJoinedValue, value2.ClrType, value2.SqlType, value2.HasValue, value2.SourceExpression), uo.SourceExpression));
                        }
                        if (unary.Operand.NodeType == SqlNodeType.TypeCase)
                        {
                            SqlTypeCase @case = (SqlTypeCase)unary.Operand;
                            return new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, new SqlUnary(SqlNodeType.OuterJoinedValue, @case.Discriminator.ClrType, @case.Discriminator.SqlType, @case.Discriminator, @case.SourceExpression), uo.SourceExpression);
                        }
                    }
                }
                uo.Operand = this.ConvertToFetchedExpression(uo.Operand);
                if (((uo.NodeType == SqlNodeType.Not) || (uo.NodeType == SqlNodeType.Not2V)) && (uo.Operand.NodeType == SqlNodeType.Value))
                {
                    SqlValue value3 = (SqlValue)uo.Operand;
                    return this.sql.Value(typeof(bool), value3.SqlType, !((bool)value3.Value), value3.IsClientSpecified, value3.SourceExpression);
                }
                if (uo.NodeType == SqlNodeType.Not2V)
                {
                    if (SqlExpressionNullability.CanBeNull(uo.Operand) != false)
                    {
                        SqlSearchedCase left = new SqlSearchedCase(typeof(int), new SqlWhen[] { new SqlWhen(uo.Operand, this.sql.ValueFromObject(1, false, uo.SourceExpression)) }, this.sql.ValueFromObject(0, false, uo.SourceExpression), uo.SourceExpression);
                        return this.sql.Binary(SqlNodeType.EQ, left, this.sql.ValueFromObject(0, false, uo.SourceExpression));
                    }
                    return this.sql.Unary(SqlNodeType.Not, uo.Operand);
                }
                if ((uo.NodeType == SqlNodeType.Convert) && (uo.Operand.NodeType == SqlNodeType.Value))
                {
                    SqlValue value4 = (SqlValue)uo.Operand;
                    return this.sql.Value(uo.ClrType, uo.SqlType, DBConvert.ChangeType(value4.Value, uo.ClrType), value4.IsClientSpecified, value4.SourceExpression);
                }
                if ((uo.NodeType != SqlNodeType.IsNull) && (uo.NodeType != SqlNodeType.IsNotNull))
                {
                    if (uo.NodeType == SqlNodeType.Treat)
                    {
                        return this.ApplyTreat(this.VisitExpression(uo.Operand), uo.ClrType);
                    }
                    return uo;
                }
                if (SqlExpressionNullability.CanBeNull(uo.Operand) == false)
                {
                    return this.sql.ValueFromObject(uo.NodeType == SqlNodeType.IsNotNull, false, uo.SourceExpression);
                }
                SqlExpression operand = uo.Operand;
                SqlNodeType nodeType = operand.NodeType;
                if (nodeType <= SqlNodeType.Element)
                {
                    switch (nodeType)
                    {
                        case SqlNodeType.ClientCase:
                            {
                                SqlClientCase case3 = (SqlClientCase)uo.Operand;
                                List<SqlExpression> matches = new List<SqlExpression>();
                                List<SqlExpression> values = new List<SqlExpression>();
                                foreach (SqlClientWhen when in case3.Whens)
                                {
                                    matches.Add(when.Match);
                                    values.Add(this.VisitUnaryOperator(this.sql.Unary(uo.NodeType, when.Value, when.Value.SourceExpression)));
                                }
                                return this.sql.Case(case3.ClrType, case3.Expression, matches, values, case3.SourceExpression);
                            }
                        case SqlNodeType.ClientParameter:
                            return uo;

                        case SqlNodeType.ClientQuery:
                            {
                                SqlClientQuery query = (SqlClientQuery)operand;
                                if (query.Query.NodeType != SqlNodeType.Element)
                                {
                                    return this.sql.ValueFromObject(uo.NodeType == SqlNodeType.IsNotNull, false, uo.SourceExpression);
                                }
                                operand = this.sql.SubSelect(SqlNodeType.Exists, query.Query.Select);
                                if (uo.NodeType == SqlNodeType.IsNull)
                                {
                                    operand = this.sql.Unary(SqlNodeType.Not, operand, operand.SourceExpression);
                                }
                                return operand;
                            }
                        case SqlNodeType.Element:
                            operand = this.sql.SubSelect(SqlNodeType.Exists, ((SqlSubSelect)operand).Select);
                            if (uo.NodeType == SqlNodeType.IsNull)
                            {
                                operand = this.sql.Unary(SqlNodeType.Not, operand, operand.SourceExpression);
                            }
                            return operand;
                    }
                    return uo;
                }
                switch (nodeType)
                {
                    case SqlNodeType.OptionalValue:
                        uo.Operand = ((SqlOptionalValue)operand).HasValue;
                        return uo;

                    case SqlNodeType.TypeCase:
                        {
                            SqlTypeCase case4 = (SqlTypeCase)uo.Operand;
                            List<SqlExpression> list3 = new List<SqlExpression>();
                            List<SqlExpression> list4 = new List<SqlExpression>();
                            foreach (SqlTypeCaseWhen when2 in case4.Whens)
                            {
                                SqlUnary unary2 = new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, when2.TypeBinding, when2.TypeBinding.SourceExpression);
                                SqlExpression item = this.VisitUnaryOperator(unary2);
                                if (item is SqlNew)
                                {
                                    throw Error.DidNotExpectTypeBinding();
                                }
                                list3.Add(when2.Match);
                                list4.Add(item);
                            }
                            return this.sql.Case(uo.ClrType, case4.Discriminator, list3, list4, case4.SourceExpression);
                        }
                    case SqlNodeType.Value:
                        {
                            SqlValue value5 = (SqlValue)uo.Operand;
                            return this.sql.Value(typeof(bool), this.typeProvider.From(typeof(int)), (value5.Value == null) == (uo.NodeType == SqlNodeType.IsNull), value5.IsClientSpecified, uo.SourceExpression);
                        }
                }
                return uo;
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                this.disableInclude = true;
                return base.VisitUserQuery(suq);
            }
        }

        private class SqlFetcher : SqlVisitor
        {
            readonly List<SqlTable> tables;

            SqlFetcher()
            {
                tables = new List<SqlTable>();
            }

            public static List<SqlTable> FetchTables(SqlNode node)
            {
                var instance = new SqlFetcher();
                instance.Visit(node);
                return instance.tables;
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                tables.Add(tab);
                return base.VisitTable(tab);
            }
        }
    }




}