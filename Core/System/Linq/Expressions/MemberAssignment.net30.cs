#if NET20 || NET30

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using Theraot.Core.System.Dynamic.Utils;

namespace Theraot.Core.System.Linq.Expressions
{
    /// <summary>
    /// Represents assignment to a member of an object.
    /// </summary>
    public sealed class MemberAssignment : MemberBinding
    {
        private readonly Expression _expression;

        internal MemberAssignment(MemberInfo member, Expression expression)
#pragma warning disable CS0618 // El tipo o el miembro est�n obsoletos
            : base(MemberBindingType.Assignment, member)
#pragma warning restore CS0618 // El tipo o el miembro est�n obsoletos
        {
            _expression = expression;
        }

        /// <summary>
        /// Gets the <see cref="Expression"/> which represents the object whose member is being assigned to.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
        }

        /// <summary>
        /// Creates a new expression that is like this one, but using the
        /// supplied children. If all of the children are the same, it will
        /// return this expression.
        /// </summary>
        /// <param name="expression">The <see cref="Expression" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public MemberAssignment Update(Expression expression)
        {
            if (expression == Expression)
            {
                return this;
            }
            return Expression.Bind(Member, expression);
        }
    }

    public partial class Expression
    {
        /// <summary>
        /// Creates a <see cref="MemberAssignment"/> binding the specified value to the given member.
        /// </summary>
        /// <param name="member">The <see cref="MemberInfo"/> for the member which is being assigned to.</param>
        /// <param name="expression">The value to be assigned to <paramref name="member"/>.</param>
        /// <returns>The created <see cref="MemberAssignment"/>.</returns>
        public static MemberAssignment Bind(MemberInfo member, Expression expression)
        {
            ContractUtils.RequiresNotNull(member, "member");
            RequiresCanRead(expression, "expression");
            Type memberType;
            ValidateSettableFieldOrPropertyMember(member, out memberType);
            if (!memberType.IsAssignableFrom(expression.Type))
            {
                throw Error.ArgumentTypesMustMatch();
            }
            return new MemberAssignment(member, expression);
        }

        /// <summary>
        /// Creates a <see cref="MemberAssignment"/> binding the specified value to the given property.
        /// </summary>
        /// <param name="propertyAccessor">The <see cref="PropertyInfo"/> for the property which is being assigned to.</param>
        /// <param name="expression">The value to be assigned to <paramref name="propertyAccessor"/>.</param>
        /// <returns>The created <see cref="MemberAssignment"/>.</returns>
        public static MemberAssignment Bind(MethodInfo propertyAccessor, Expression expression)
        {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(expression, "expression");
            ValidateMethodInfo(propertyAccessor);
            return Bind(GetProperty(propertyAccessor), expression);
        }

        private static void ValidateSettableFieldOrPropertyMember(MemberInfo member, out Type memberType)
        {
            var fi = member as FieldInfo;
            if (fi == null)
            {
                var pi = member as PropertyInfo;
                if (pi == null)
                {
                    throw Error.ArgumentMustBeFieldInfoOrPropertInfo();
                }
                if (!pi.CanWrite)
                {
                    throw Error.PropertyDoesNotHaveSetter(pi);
                }
                memberType = pi.PropertyType;
            }
            else
            {
                memberType = fi.FieldType;
            }
        }
    }
}

#endif