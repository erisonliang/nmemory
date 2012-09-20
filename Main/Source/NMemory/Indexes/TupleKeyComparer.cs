﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Indexes;
using NMemory.Common;

namespace NMemory.Indexes
{
    internal class TupleKeyComparer<T> : IComparer<T>
    {
        private Func<T, T, int> comparer;

        public TupleKeyComparer(SortOrder[] sortOrders)
        {
            if (!ReflectionHelper.IsTuple(typeof(T)))
            {
                throw new InvalidOperationException("The specified generic type is not a tuple");
            }

            PropertyInfo[] properties = typeof(T).GetProperties()
                .Where(p => p.Name.StartsWith("Item", StringComparison.InvariantCulture))
                .ToArray();

            if (sortOrders == null)
            {
                sortOrders = Enumerable.Repeat(SortOrder.Ascending, properties.Length).ToArray();
            }

            if (sortOrders.Length != properties.Length)
            {
                throw new ArgumentException("The count of sort ordering values does not match the count of anonymous type propeties", "sortOrders");
            }

            ParameterExpression x = Expression.Parameter(typeof(T), "x");
            ParameterExpression y = Expression.Parameter(typeof(T), "y");

            ParameterExpression variable = Expression.Variable(typeof(int), "var");

            List<Expression> blockBody = new List<Expression>();

            for (int i = 0; i < properties.Length; i++)
			{
                PropertyInfo p = properties[i];

			    if (i == 0)
	            {
		            blockBody.Add(
                        Expression.Assign(
                            variable, 
                            CreateComparsion(p, x, y, sortOrders[i])));
	            }
                else
                {
                    blockBody.Add(
                        Expression.IfThen(
                            Expression.Equal(variable, Expression.Constant(0)),

                            Expression.Assign(variable, CreateComparsion(p, x, y, sortOrders[i]))
                                
                            ));
                }
			}

            // Eval the last variable
            blockBody.Add(variable);

            var lambda = Expression.Lambda<Func<T, T, int>>(
                Expression.Block(
                    new ParameterExpression[] { variable },
                    blockBody),
                x,
                y);

            this.comparer = lambda.Compile();
        }

        private Expression CreateComparsion(PropertyInfo p, ParameterExpression x, ParameterExpression y, SortOrder ordering)
        {
            MethodInfo compareMethod = 
                ReflectionHelper.GetStaticMethodInfo(() => 
                    PrimitiveKeyComparer.Compare<object>(null, null, SortOrder.Ascending));

            compareMethod = compareMethod.GetGenericMethodDefinition().MakeGenericMethod(p.PropertyType);

            return Expression.Call(
                compareMethod,
                Expression.Property(x, p),
                Expression.Property(y, p),
                Expression.Constant(ordering, typeof(SortOrder)));
        }

        public int Compare(T x, T y)
        {
            if (x == null)
            {
                throw new ArgumentException("Tuple key cannot be null", "x");
            }

            if (y == null)
            {
                throw new ArgumentException("Typle key cannot be null", "y");
            }

            int result = this.comparer(x, y);

            return result;
        }


    }
}