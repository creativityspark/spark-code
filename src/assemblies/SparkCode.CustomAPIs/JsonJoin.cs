using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace SparkCode.CustomAPIs
{
    // based on
    // https://learn.microsoft.com/en-us/dotnet/csharp/linq/how-to-build-dynamic-queries
    // https://dotnettutorials.net/lesson/linq-joins-in-csharp/
    public class JsonJoin : IPlugin
    {
        private Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Custom API Inputs
            string inputObject1str = context.InputParameters["InputObject1"] as string ?? throw new ArgumentNullException($"InputObject1 is required");
            string inputObject2str = context.InputParameters["InputObject2"] as string ?? throw new ArgumentNullException($"InputObject2 is required");
            string joinKey1 = context.InputParameters["JoinKey1"] as string ?? throw new ArgumentNullException($"JoinKey1 is required");
            string joinKey2 = context.InputParameters["JoinKey2"] as string ?? throw new ArgumentNullException($"JoinKey2 is required");


            string outputObject = Join(inputObject1str, inputObject2str, joinKey1, joinKey2);

            // Set OutputParameters values
            context.OutputParameters["OutputObject"] = outputObject;
        }

        public string Join(string inputObject1str, string inputObject2str, string joinKey1, string joinKey2)
        {
            string outputObject = null;
            // Trace input parameters
            ctx.Trace($"InputObject1: {inputObject1str}");
            ctx.Trace($"InputObject2: {inputObject2str}");
            ctx.Trace($"JoinKey1: {joinKey1}");
            ctx.Trace($"JoinKey2: {joinKey2}");

            // parse input JSON strings into dynamic objects
            var list1 = JsonConvert.DeserializeObject<List<ExpandoObject>>(inputObject1str);
            var list2 = JsonConvert.DeserializeObject<List<ExpandoObject>>(inputObject2str);

            // Create parameter expressions
            var param1 = Expression.Parameter(typeof(ExpandoObject), "x");
            var param2 = Expression.Parameter(typeof(ExpandoObject), "y");

            // Create dynamic property access
            var keySelector1 = Expression.PropertyOrField(param1, joinKey1);
            var keySelector2 = Expression.PropertyOrField(param2, joinKey2);

            // Compile expressions
            var lambda1 = Expression.Lambda<Func<dynamic, object>>(Expression.Convert(keySelector1, typeof(object)), param1).Compile();
            var lambda2 = Expression.Lambda<Func<dynamic, object>>(Expression.Convert(keySelector2, typeof(object)), param2).Compile();

            // Lambda result selector containing all the properties from list 1 and 2
            var resultSelector = Expression.Lambda<Func<ExpandoObject, ExpandoObject, object>>(
                Expression.New(
                    typeof(object).GetConstructor(new Type[] { }),
                    Expression.MemberInit(
                        Expression.New(typeof(object)),
                        Expression.Bind(typeof(object).GetProperty("Name"), Expression.Property(param1, "Name")),
                        Expression.Bind(typeof(object).GetProperty("Age"), Expression.Property(param2, "Age"))
                    )
                ),
                param1, param2
            ).Compile();

            // Perform join
            var result = list1.Join(list2, lambda1, lambda2, resultSelector);

            ctx.Trace($"OutputObject: {outputObject}");
            return outputObject;
        }
    }
}
