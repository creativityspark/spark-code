using Fluid.Ast;
using System.Collections.Generic;
using System.Linq;

namespace SparkCode.Templates
{
    public class IdentifierVisitor : AstVisitor
    {
        public HashSet<string> Identifiers { get; } = new HashSet<string>();

        protected override Expression VisitMemberExpression(MemberExpression memberExpression)
        {
            var firstSegment = memberExpression.Segments.FirstOrDefault() as IdentifierSegment;
            if (firstSegment != null)
            {
                Identifiers.Add(firstSegment.Identifier);
            }

            return base.VisitMemberExpression(memberExpression);
        }
    }
}