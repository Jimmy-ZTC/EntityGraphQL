using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityGraphQL.Compiler
{
    /// <summary>
    /// Once we parse the GQL document we get a graph of BaseGraphQLField objects where each one is ObjectProjectionField
    /// ListSelectionField, ScalarField or FragmentField. Example of each below in a GQL document
    /// {
    ///     singleEntity { # ObjectProjectionField
    ///         this # ScalarField
    ///         that # ScalarField
    ///     }
    ///     listOfThings { # ListSelectionField
    ///         ...someFrag # FragmentField
    ///     }
    /// }
    /// </summary>
    public abstract class BaseGraphQLField : IGraphQLNode
    {
        /// <summary>
        /// Name of the field
        /// </summary>
        /// <value></value>
        public string Name { get; protected set; }
        /// <summary>
        /// Any values for a parameter that had a constant value in the query document.
        /// They are extracted out to parameters instead of inline ConstantExpression for future query caching possibilities
        /// </summary>
        protected Dictionary<ParameterExpression, object> constantParameters = new Dictionary<ParameterExpression, object>();

        internal Dictionary<ParameterExpression, object> ConstantParameters { get => constantParameters; }
        public ParameterExpression RootFieldParameter { get; set; }
        public List<Type> Services { get; } = new List<Type>();
        /// <summary>
        /// If this node has any services at all in its graph.
        /// </summary>
        /// <value></value>
        public abstract bool HasAnyServices(IEnumerable<GraphQLFragmentStatement> fragments);

        /// <summary>
        /// The dotnet Expression for this node. Could be as simple as (Person p) => p.Name
        /// Or as complex as (DbContext ctx) => ctx.People.Where(...).Select(p => new {...}).First()
        /// If there is a object selection (new {} in a Select() or not) we will build the NodeExpression on
        /// Execute() so we can look up any query fragment selections
        /// </summary>
        /// <param name="serviceProvider">Service provider to resolve services </param>
        /// <param name="fragments">Fragments in the query document</param>
        /// <param name="withoutServiceFields">If true th expression builds selection without fields that require services</param>
        /// <param name="replaceContextWith">A replacement context from a selection without service fields</param>
        /// <param name="isRoot">If this field is a Query root field</param>
        /// <param name="useReplaceContextDirectly">Use the replaceContextWith instead of running through replacer. Used for fields gone from collection to single when running services seperately</param>
        /// <returns></returns>
        public abstract ExpressionResult GetNodeExpression(IServiceProvider serviceProvider, List<GraphQLFragmentStatement> fragments, ParameterExpression schemaContext, bool withoutServiceFields, Expression replaceContextWith = null, bool isRoot = false, bool useReplaceContextDirectly = false);

        public abstract IEnumerable<BaseGraphQLField> Expand(List<GraphQLFragmentStatement> fragments, bool withoutServiceFields);

        public void AddServices(IEnumerable<Type> services)
        {
            if (services == null)
                return;
            Services.AddRange(services);
        }

    }
}