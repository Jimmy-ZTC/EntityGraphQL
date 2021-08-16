using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityGraphQL.Schema;

namespace EntityGraphQL.Compiler
{
    public class GraphQLMutationField : BaseGraphQLQueryField
    {
        private readonly MutationType mutationType;
        private readonly Dictionary<string, ExpressionResult> args;
        private readonly BaseGraphQLQueryField resultSelection;

        public BaseGraphQLQueryField ResultSelection { get => resultSelection; }

        public GraphQLMutationField(string name, MutationType mutationType, Dictionary<string, ExpressionResult> args, BaseGraphQLQueryField resultSelection)
        {
            Name = name;
            this.mutationType = mutationType;
            this.args = args;
            this.resultSelection = resultSelection;
        }

        public async Task<object> ExecuteMutationAsync<TContext>(TContext context, GraphQLValidator validator, IServiceProvider serviceProvider, Func<string, string> fieldNamer)
        {
            try
            {
                return await mutationType.CallAsync(context, args, validator, serviceProvider, fieldNamer);
            }
            catch (EntityQuerySchemaException e)
            {
                throw new EntityQuerySchemaException($"Error applying mutation: {e.Message}", e);
            }
        }

        public override ExpressionResult GetNodeExpression(IServiceProvider serviceProvider, List<GraphQLFragmentStatement> fragments, ParameterExpression schemaContext, bool withoutServiceFields, Expression replaceContextWith = null, bool isRoot = false, bool useReplaceContextDirectly = false)
        {
            return resultSelection.GetNodeExpression(serviceProvider, fragments, schemaContext, withoutServiceFields, replaceContextWith);
        }
    }
}
