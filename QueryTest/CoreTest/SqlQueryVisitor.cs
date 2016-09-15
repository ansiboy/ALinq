using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.Dynamic.Test.CoreTest
{
    class SqlQueryVisitor
    {
        private TokenCursor tokenCursor;

        public SqlQueryVisitor(TokenCursor tokenCursor)
        {
            this.tokenCursor = tokenCursor;
        }

        public virtual void Visit()
        {
            if(Token.Keyword == Keyword.Select)
            {
                VisitSelect();
            }
        }

        private Token Token
        {
            get { return tokenCursor.Current; }
        }

        private void NextToken()
        {
             tokenCursor.NextToken();
        }

        public virtual void VisitSelect()
        {
            Token.Validate(Keyword.Select);
            NextToken();
            VisitSeection();

            Token.Validate(Keyword.From);
            VisitFrom();

            if (Token.Keyword == Keyword.Where)
                VisitWhere();

            if (Token.Keyword == Keyword.Order)
                VisitOrderBy();
        }

        private void VisitOrderBy()
        {
            
        }

        private void VisitWhere()
        {
            
        }

        private void VisitFrom()
        {
            
        }

        private void VisitSeection()
        {
            switch (Token.Keyword)
            {
                case Keyword.Value:
                    VisitValue();
                    break;
                case Keyword.Row:
                    VisitRow();
                    break;
                case Keyword.None:
                    VisitRow();
                    break;
                default:
                    throw Error.GenericSyntaxError(Token);
            } 
        }

        private void VisitRow()
        {
            
        }

        private void VisitValue()
        {
            
        }
    }
}
